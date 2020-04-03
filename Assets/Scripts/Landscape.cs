using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Landscape : MonoBehaviour
{
    /* perhaps there shouldn't be global timers...
    public static readonly InfectedTimer currentTimer = new InfectedTimer(3); // UPDATE EVERY 3 ROUND
    public static readonly InfectedTimer nextTimer = new InfectedTimer(3); // UPDATE EVERY 3 ROUND
    */

    public GameObject blockPrefab;

    public ArrayList instanceList;
    const int MAX_BLOCK_ROW = 10;
    const int MAX_BLOCK_COL = 10;

    // public GameObject[] landblocks; // for custom initialization
    public List<GameObject> landblocks;
    public List<Block> blocks;

    // just for reference
    public int totalMaterialCount = 0;

    public int playerMaterialCount = 0;

    private GameObject selected;


    GUIStyle materialStyle;

    // Start is called before the first frame update
    void Start()
    {
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


        materialStyle = new GUIStyle();
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

        if (playerMaterialCount < card.cost)
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
        foreach (Block block in blocks)
        {
            block.EndInBlock();
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
        GUI.Label(new Rect(30, 30, 60, 30), "资源总计：" + totalMaterialCount, materialStyle);
        GUI.Label(new Rect(30, 60, 60, 30), "政府资源：" + playerMaterialCount, materialStyle);

    }

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
