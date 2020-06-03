using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Landscape : MonoBehaviour
{

    [Header("Title")]
    public GameObject successTitle;
    public GameObject failTitle;

    public GameObject gameOverDialog;
    private Button functionButton;
    private Button returnButton;
    private Text scoreInfo;
    private Text functionText;
    private GameObject blockPanel;

    [Header("condition")]
    public int failLimitedTime;

    [Header("card")]
    public Card quarantineCard;
    public Card stopWorkingCard;
    public Card startWorkingCard;
    public Card aidCard;
    public Card taxingCard;

    /* population related */
    public InfectedTimer CTimer = null; // Current generation counter
    public InfectedTimer NTimer = null; // Next generation counter

    public GameObject blockPrefab;

    // deprecated variable, just for runtime landscape generation
    public ArrayList instanceList;

    // deprecated constants, just for runtime landscape generation
    const int MAX_BLOCK_ROW = 10;
    const int MAX_BLOCK_COL = 10;

    // will be filled with child Landblocks when constructed
    public List<GameObject> landblocks;
    public List<Block> blocks;

    // global counter, just for reference
    int totalMaterialCount = 0;
    int playerMaterialCount = 0;
    int dayCounter = 0;

    // block to manipulate, selected by player
    private GameObject selected;

    // GUI related parameters, used in OnGUI()
    GUIStyle materialStyle = new GUIStyle();    

    /* Initialization section 
     * following functions are for initialization when the level is started.
     */
    void PaintBackground()
    {
        Tilemap tilemap = GameObject.FindGameObjectWithTag("Background").GetComponent<Tilemap>();
        tilemap.ClearAllTiles();
        for (int i= -10;i<13; ++i)
        {
            for (int j = -10; j < 13; ++j)
            {
                int scale = 70;
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                Sprite tmp = Utility.GetSprite(SpriteType.RANDOM_ROAD);
                tile.sprite = tmp;
                tilemap.SetTile(new Vector3Int(i*scale,j*scale,0),tile); 
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // UI init section
        successTitle = GameObject.Find("text/success");
        failTitle = GameObject.Find("text/fail");
        gameOverDialog = GameObject.Find("Canvas/GameOverBackground");
        blockPanel = GameObject.Find("Canvas/BlockPanel");
        functionButton = Utility.GetCanvasComponent<Button>(UIPath.GameOver, "Functional");
        returnButton = Utility.GetCanvasComponent<Button>(UIPath.GameOver, "Return");
        scoreInfo = Utility.GetCanvasComponent<Text>(UIPath.GameOver, "Score");
        functionText = Utility.GetCanvasComponent<Text>(UIPath.GameOver, "Functional/Text");

        functionButton.onClick.AddListener(OnFunctional);
        returnButton.onClick.AddListener(OnReturn);

        successTitle.SetActive(false);
        failTitle.SetActive(false);
        gameOverDialog.SetActive(false);

        blockPanel.SetActive(false);

        // taxingCard.gameObject.SetActive(false);
        GameObject.Find("Canvas/CardInfo/CardTitle").GetComponent<Text>().text = "";
        GameObject.Find("Canvas/CardInfo/CardIntro").GetComponent<Text>().text = "";

        // MODEL init section
        FileParameterManager.Init();
        FileParameterManager.LoadData();
        // FileParameterManager.DumpData(); // for data dumping test
        FileParameterManager.LoadCardData();
        // FileParameterManager.DumpCardData(); // for data dumping test

        VirusModel model = gameObject.GetComponent<VirusModel>();
        if (model.enableUIVer2)
        {
            PaintBackground();
        }
        NTimer = new InfectedTimer(model.timerStagePeriod);
        CTimer = new InfectedTimer(model.timerStagePeriod);
        NTimer.AddStage(InfectedStage.stages[0]);
        NTimer.AddStage(InfectedStage.stages[1]);
        CTimer.AddStage(InfectedStage.stages[2]);
        CTimer.AddStage(InfectedStage.stages[3]);

        landblocks = new List<GameObject>();
        foreach (Transform child in gameObject.transform)
        {
            landblocks.Add(child.gameObject);
        }

        
        blocks = new List<Block>();
        foreach (GameObject landblock in landblocks)
        {
            Block block = new Block(landblock);
            /* TODO: initialize block property here */
            /* don't init here! init in constructor of block */
            /*
            Landblock lb = landblock.GetComponent<Landblock>();
            // initialize infected population
            block.HPIPInit(lb.infected);
            */
            blocks.Add(block);
        }

        /* build block links */
        foreach (GameObject landblock in landblocks)
        {
            Landblock lb = landblock.GetComponent<Landblock>();
            foreach (GameObject outb in lb.customOutLandBlocks)
            {
                Landblock outlb = outb.GetComponent<Landblock>();
                lb.block.AddOutBlock(outlb.block);
            }
        }

        UpdateTotalMaterialCount();

        materialStyle.fontSize = 30;
    }

    /* Trigger section 
     * Functions below are for event dispatching and handling.
     * Events can be mouse/keyboard input, card selection, button behavior, etc.
     */

    void OnGameOver()
    {
        if (gameState != GameState.SUCCESS && gameState != GameState.FAILURE)
            return; // happen?
        /* TODO: handle with score recording. 
         * It should be directly written to local files  */
        int score = playerMaterialCount;
        if (gameState == GameState.FAILURE) score = 0;
        if (Utility.GetVirusModel().enableUIVer2)
        {
            if (gameState == GameState.SUCCESS)
            {
                functionText.text = "Next Level";
            }
            else
            {
                functionText.text = "Try Again";
            }
            scoreInfo.text = string.Format("score: {0}", score);
            gameOverDialog.SetActive(true);
            return;
        }
        if (gameState == GameState.SUCCESS)
        {
            successTitle.SetActive(true);
            failTitle.SetActive(false);
        }
        else if (gameState == GameState.FAILURE)
        {
            successTitle.SetActive(false);
            failTitle.SetActive(true);
        }
    }

    public void OnFunctional()
    {
        if (gameState == GameState.SUCCESS)
        {
            OnNextLevel();
        }
        else if (gameState == GameState.FAILURE)
        {
            OnRetry();
        }
    }

    // for going to the next level
    public void OnNextLevel()
    {
        Debug.Log("New levels coming soon");
        SceneManager.LoadScene("SelectLevel");
    }
        
    // for restarting the level
    public void OnRetry()
    {
        Scene scene = SceneManager.GetActiveScene();
        LevelManager.selectedLevel = scene.name;
        SceneManager.LoadScene("Progressing");
    }

    // for returning to the level selecting scene
    public void OnReturn()
    {
        /* TODO:  Maybe there should be a confirm dialog... */
        SceneManager.LoadScene("SelectLevel");
    }
         
    public void BlockClicked(GameObject obj)
    {
        if (selected == obj)
            selected = null;
        else
            selected = obj;
        BroadcastMessage("UpdateSelected", obj); 
    }

    public void OperateBlock(Block block)
    {
        Debug.Log("what? I'm called?");
    }

    public int CardEventDispatched(Card card)
    {
        Debug.Log(card.type);
        if (selected == null)
            return 1;

        if (playerMaterialCount + card.Cost < 0)
        {
            return 2;
        }

        Landblock lb = selected.GetComponent<Landblock>();
        Block block = lb.block;

        switch (card.type)
        {
            case CardType.QUARANTINE:
                {
                    block.Quarantined(10); //TODO parameterize
                    playerMaterialCount += card.Cost;
                    break;
                }
            case CardType.STOP_WORKING:
                {
                    if (block.type != BlockType.FACTORY)
                    {
                        return 3;
                    }
                    block.StopWorking();
                    playerMaterialCount += card.Cost;
                    break;
                }
            case CardType.START_WORKING:
                {
                    if (block.type != BlockType.FACTORY)
                    {
                        return 3;
                    }
                    block.StartWorking();
                    playerMaterialCount += card.Cost;
                    break;
                }
            case CardType.SPECIAL_AID:
                {
                    block.Aided();
                    playerMaterialCount += card.Cost;
                    break;
                }
            case CardType.TAXING:
                {
                    if (Utility.GetVirusModel().autoGlobalTaxing) break;
                    playerMaterialCount += block.Taxed();
                    playerMaterialCount += card.Cost;
                    break;
                }
            default:
                break;
        }
        return 0;
    }


    /* Iteration section 
     * Functions below are for data iterations every round.
     */
    public void UpdateTotalMaterialCount()
    {
        totalMaterialCount = 0;
        if (Utility.GetVirusModel().autoGlobalTaxing)
        {
            foreach (Block block in blocks)
            {
                playerMaterialCount += block.TaxAll();
            }
            return;
        }

        foreach (Block block in blocks)
        {
            totalMaterialCount += block.MaterialCount.Data;
        }

    }

    int totalDeathToll = 0;
    bool firstDeath = false;
    public void UpdateDeathToll()
    {
        foreach (Block block in blocks)
        {
            totalDeathToll += block.GetDeathToll();
        }
        if (totalDeathToll != 0)
        {
            if (firstDeath == false)
            {
                noticedEvent.Add("第一位死者出现了");
            }
            firstDeath = true;
        }
        
    }

    int totalConfirmed = 0;
    bool confirmed = false;
    public void UpdateTotalConfirmed()
    {
        totalConfirmed = 0;
        foreach (Block block in blocks)
        {
            totalConfirmed += block.GetConfirmedInfectedPopulation();
        }
        if (totalConfirmed != 0)
        {
            if (confirmed == false)
            {
                noticedEvent.Add("出现第一例感染者");
            }
            confirmed = true;
        }
    }

    void endRound()
    {
        dayCounter++;
        int develop = NTimer.Tick() + CTimer.Tick();
        
        foreach (Block block in blocks)
        {
            block.EndInBlock(develop);
        }
        foreach (Block block in blocks)
        {
            block.EndRound();
        }
        foreach (Block block in blocks)
        {
            block.Commit();
        }
        UpdateTotalMaterialCount();
        UpdateDeathToll();
        UpdateTotalConfirmed();
    }
    

    // Update is called once per frame
    int counter;
    // game state, defined in Basics
    GameState gameState = GameState.RUNNING;
    // judgment of success and failure
    GameState CheckSucceedOrFail()
    {
        if (gameState != GameState.RUNNING)
            return gameState;
        if (failLimitedTime < 0)
        {
            return GameState.FAILURE;
        }

        int healthCnt = 0;
        foreach (Block block in blocks)
        {
            healthCnt += block.GetInfectedPopulation();
        }
        if (healthCnt == 0)
        {
            
            return GameState.SUCCESS;
        }

        return GameState.RUNNING;
    }

    void Update()
    {
        GameState s = gameState;
        gameState = CheckSucceedOrFail();
        if (s == GameState.RUNNING && 
            (gameState == GameState.SUCCESS || gameState == GameState.FAILURE))
        {
            OnGameOver();
        }
        if (gameState != GameState.RUNNING)
            return;

        counter++;
        if (counter > 60 * 3)
        {
            endRound();
            counter = 0;
            failLimitedTime--;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)){ CardEventDispatched(quarantineCard);}
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { CardEventDispatched(stopWorkingCard); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { CardEventDispatched(startWorkingCard); }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { CardEventDispatched(aidCard); }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) { CardEventDispatched(taxingCard); }
    }

    List<string> noticedEvent = new List<string>();

    private void OnGUI()
    {
        GameObject.Find("Canvas/Panel/ResourceUI").GetComponent<Text>().text = "死亡人数：" + totalDeathToll;
        GameObject.Find("Canvas/Panel/TaxUI").GetComponent<Text>().text = "政府资源：" + playerMaterialCount;
        GameObject.Find("Canvas/Panel/DayUI").GetComponent<Text>().text = "天数：" + dayCounter;
        GameObject.Find("Canvas/Panel/TimerUI").GetComponent<Text>().text = "病毒升级倒计时：" + NTimer.countdown;

        Text notice = GameObject.Find("Canvas/Notice/NoticeText").GetComponent<Text>();
        while (noticedEvent.Count > 3) noticedEvent.RemoveAt(0);
        string displayed = "";
        foreach (var ele in noticedEvent) displayed += ele + "\n";
        notice.text = displayed;

        if (selected == null)
        {
            blockPanel.SetActive(false);
            return;
        }

        blockPanel.SetActive(true);

        Block block = selected.GetComponent<Landblock>().block;
        string blockType = Utility.BlockTypeToChineseString(block.type);
        Utility.GetCanvasComponent<Text>(UIPath.SelectedBasic, "BlockType").text = "类型：" + blockType;
        Utility.GetCanvasComponent<Text>(UIPath.SelectedBasic, "POPCount").text = "总人口：" + block.GetTotalPopulation().ToString();
        Utility.GetCanvasComponent<Text>(UIPath.SelectedBasic, "CPCount").text = "感染者：" + block.GetConfirmedInfectedPopulation().ToString();
        Utility.GetCanvasComponent<Text>(UIPath.SelectedBasic, "MCost").text = "资源消耗：" + Mathf.Abs(block.GetMCost()).ToString();
        Utility.GetCanvasComponent<Text>(UIPath.SelectedBasic, "VirusCount").text = "病原体浓度：" + block.GetVirusCount().ToString();


        Utility.GetCanvasObject(UIPath.SelectedSpec, "Quarantine").SetActive(block.IsQuarantined);
        Utility.GetCanvasObject(UIPath.SelectedSpec, "StopWorking").SetActive(block.type == BlockType.FACTORY && !block.IsWorking);

        string specialNote = "";
        if (block.IsQuarantined)
            specialNote += "隔离期结束还有" + block.QuarantineCounter + "天";

        Utility.GetCanvasComponent<Text>(UIPath.SelectedSpec, "SpecialNote").text = specialNote;

        
    }

    // temorarily deprecated
    private void DynamicInit()
    {
        instanceList = new ArrayList();
        blocks = new List<Block>();
        int infectedStartX = Random.Range(0, MAX_BLOCK_ROW);
        int infectedStartY = Random.Range(0, MAX_BLOCK_COL);
        // initialize all blocks
        for (int i = 0; i < MAX_BLOCK_ROW; ++i)
        {
            for (int j = 0; j < MAX_BLOCK_COL; ++j)
            {
                GameObject obj = Instantiate(blockPrefab) as GameObject;
                instanceList.Add(obj);
                obj.transform.position = new Vector2((float)(-2.0 + 1.2 * i), (float)(2.0 - 1.2 * j));
                Block block = new Block(obj);

                /*
                // set origin of infected
                if (i == infectedStartX && j == infectedStartY)
                    block.HPIPInit(1);
                else
                    block.HPIPInit(0);
                */

                // set block links
                if (i >= 1)
                {
                    block.AddOutBlock(blocks[(i - 1) * MAX_BLOCK_COL + j]);
                    (blocks[(i - 1) * MAX_BLOCK_COL + j]).AddOutBlock(block);
                }
                if (j >= 1)
                {
                    block.AddOutBlock(blocks[i * MAX_BLOCK_COL + j - 1]);
                    (blocks[i * MAX_BLOCK_COL + j - 1]).AddOutBlock(block);
                }
                blocks.Add(block);
                // obj.GetComponent<Landblock>().block = block;
            }
        }
    }

}
