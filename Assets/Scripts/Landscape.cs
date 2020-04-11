using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Landscape : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        VirusModel model = gameObject.GetComponent<VirusModel>();
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

        materialStyle.fontSize = 30;
    }

    public void UpdateTotalMaterialCount()
    {
        totalMaterialCount = 0;
        foreach (Block block in blocks)
        {
            totalMaterialCount += block.MaterialCount.Data;
        }
    }

    public void BlockClicked(GameObject obj)
    {
        selected = obj;
        Debug.Log(obj.name);
        BroadcastMessage("UpdateSelected", obj); 
    }

    public int CardEventDispatched(Card card)
    {
        Debug.Log(card.type);
        if (selected == null)
            return 1;

        if (playerMaterialCount + card.cost < 0)
        {
            return 2;
        }
        
        Landblock lb = selected.GetComponent<Landblock>();
        Block block = lb.block;
        
        switch (card.type)
        {
            case CardType.QUARANTINE:
                {
                    block.Quarantined(10);
                    playerMaterialCount += card.cost;
                    break;
                }
            case CardType.STOP_WORKING:
                {
                    if (block.type != BlockType.FACTORY)
                    {
                        return 3;
                    }
                    block.StopWorking();
                    playerMaterialCount += card.cost;
                    break;
                }
            case CardType.START_WORKING:
                {
                    if (block.type != BlockType.FACTORY)
                    {
                        return 3;
                    }
                    block.StartWorking();
                    playerMaterialCount += card.cost;
                    break;
                }
            case CardType.SPECIAL_AID:
                {
                    block.Aided();
                    playerMaterialCount += card.cost;
                    break;
                }
            case CardType.TAXING:
                {
                    playerMaterialCount += block.Taxed();
                    playerMaterialCount += card.cost;
                    break;
                }
            default:
                break;
        }
        return 0;
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
    }
    

    // Update is called once per frame
    int counter;
    void Update()
    {
        counter++;
        if (counter > 60 * 3)
        {
            endRound();
            counter = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (selected == null) return;
            Landblock lb = selected.GetComponent<Landblock>();
            Block block = lb.block;
            playerMaterialCount += block.Taxed(); 
        }
    }

    private void OnGUI()
    {
        GameObject.Find("Canvas/Panel/ResourceUI").GetComponent<Text>().text = "资源总计：" + totalMaterialCount;
        GameObject.Find("Canvas/Panel/TaxUI").GetComponent<Text>().text = "政府资源：" + playerMaterialCount;
        GameObject.Find("Canvas/Panel/DayUI").GetComponent<Text>().text = "天数：" + dayCounter;
        GameObject.Find("Canvas/Panel/TimerUI").GetComponent<Text>().text = "病毒升级倒计时：" + NTimer.countdown;
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
