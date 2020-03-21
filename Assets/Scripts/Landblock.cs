using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landblock : MonoBehaviour
{
    public GameObject blockObject;
    public Block block;
    private bool showInfo;

    GUIStyle HPStyle;
    GUIStyle IPStyle;
    
    // Start is called before the first frame update
    void Start()
    {
        showInfo = false;
        HPStyle = new GUIStyle();
        // titleStyle2.fontSize = 20;
        HPStyle.normal.textColor = new Color(46f / 256f, 163f / 256f, 256f / 256f, 256f / 256f);
        IPStyle = new GUIStyle();
        IPStyle.normal.textColor = Color.red;

    }

    // Update is called once per frame
    void Update()
    {
        if (block == null) return;
        foreach (Block target in block.outBlocks)
        {
            Vector2 sLoc = block.blockUI.transform.position;
            Vector2 tLoc = target.blockUI.transform.position;
            Debug.DrawLine(sLoc, tLoc, Color.yellow);
        }

    }

    private void OnMouseOver()
    {
        showInfo = true;
    }

    private void OnMouseExit()
    {
        showInfo = false;
    }

    private void OnGUI()
    {
        if (showInfo)
        {
            GUI.Label(new Rect(Input.mousePosition.x, Screen.height-Input.mousePosition.y-15, 30, 30), block.HPCount.Data.ToString(),HPStyle);
        }
        if (block == null) return;
        Vector2 pos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(pos.x - 10, Screen.height - pos.y - 15, 30, 30), block.HPCount.Data.ToString(), HPStyle);
        GUI.Label(new Rect(pos.x - 10, Screen.height - pos.y + 5, 30, 30), block.IPCount.Data.ToString(), IPStyle);
    }
}
