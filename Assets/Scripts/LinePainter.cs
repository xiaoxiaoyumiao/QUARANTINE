using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePainter : MonoBehaviour
{
    Landscape landscape;
    List<GameObject> landblocks;

    List<Block> blocks;

    List<Block> Blocks
    {
        get
        {
            if (landblocks == null) return null;
            if (blocks == null)
            {
                blocks = new List<Block>();
                foreach (var ele in landblocks)
                {
                    blocks.Add(ele.GetComponent<Landblock>().block);
                }    
            }
            return blocks;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        landscape = GetComponent<Landscape>();

        landblocks = new List<GameObject>();
        foreach (Transform child in gameObject.transform)
        {
            if (child.GetComponent<Landblock>() != null)
                landblocks.Add(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
