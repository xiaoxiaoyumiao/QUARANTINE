using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Landscape : MonoBehaviour
{
    public GameObject blockPrefab;
    public ArrayList instanceList;

    const int MAX_BLOCK_ROW = 10;
    const int MAX_BLOCK_COL = 10;
    public List<Block> blocks;
    
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
    }
    // Start is called before the first frame update
    void Start()
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
                if (i == infectedStartX && j == infectedStartY)
                    block.HPIPInit(1);
                else
                    block.HPIPInit(0);

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

    // Update is called once per frame
    int counter;
    void Update()
    {
        counter++;
        if (counter > 60 * 2)
        {
            endRound();
            counter = 0;
        }
    }
    
}
