using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePainter : MonoBehaviour
{
    public Material material;
    public Color lineColor;
    public float lineSize = 1f;

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

    LineRenderer myRenderer;

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

    List<GameObject> lines = new List<GameObject>();

    public void PaintLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject();
        lines.Add(line);
        line.transform.parent = transform;
        myRenderer = line.AddComponent<LineRenderer>();

        myRenderer.material = material;
        myRenderer.startColor = lineColor;
        myRenderer.endColor = lineColor;

        myRenderer.startWidth = lineSize;
        myRenderer.endWidth = lineSize;

        myRenderer.positionCount = 2;
        myRenderer.SetPosition(0, start);
        myRenderer.SetPosition(1, end);

    }

    // Update is called once per frame
    void Update()
    {
       foreach (var ele in lines)
        {
            Destroy(ele);
        }
       foreach (var block in Blocks)
        {
            Vector3 start = block.blockUI.transform.position;
            foreach (var outb in block.outBlocks)
            {
                Vector3 end = outb.blockUI.transform.position;
                PaintLine(start, end);
            }
        }
    }
}
