using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePainter : MonoBehaviour
{
    public Material material;
    public Color lineColor;
    public Color cutLineColor;
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

    public void PaintLine(Vector3 start, Vector3 end, Color startColor, Color endColor)
    {
        GameObject line = new GameObject();
        lines.Add(line);
        line.transform.parent = transform;
        myRenderer = line.AddComponent<LineRenderer>();

        myRenderer.material = material;
        
        myRenderer.startColor = startColor;
        myRenderer.endColor = endColor;

        myRenderer.startWidth = lineSize;
        myRenderer.endWidth = lineSize;

        myRenderer.positionCount = 2;
        myRenderer.SetPosition(0, start);
        myRenderer.SetPosition(1, end);

    }

    
    void UpdateLines()
    {
        Utility.DirtyLines = false;
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
                Color startColor = cutLineColor;
                Color endColor = cutLineColor;

                if (block.GetPermission(outb))
                    startColor = lineColor;
                if (outb.GetPermission(block))
                    endColor = lineColor;
                    ;
                PaintLine(start, end, startColor, endColor);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Utility.DirtyLines)
            UpdateLines();
    }
}
