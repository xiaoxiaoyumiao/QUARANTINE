using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landblock : MonoBehaviour
{

    public Block block;
    
    // determine if this block is infected in the beginning
    public int infected;

    // determine material amount in the beginning
    public int material;

    // determine type of block

    public BlockType type;

    public GameObject[] customOutLandBlocks; // for custom initialization
    

    private bool showInfo;

    GUIStyle HPStyle;
    GUIStyle IPStyle;
    GUIStyle MStyle;
    
    // Start is called before the first frame update
    void Start()
    {
        showInfo = false;
        HPStyle = new GUIStyle();
        // titleStyle2.fontSize = 20;
        HPStyle.normal.textColor = new Color(46f / 256f, 163f / 256f, 256f / 256f, 256f / 256f);
        IPStyle = new GUIStyle();
        IPStyle.normal.textColor = Color.red;
        MStyle = new GUIStyle();
        MStyle.normal.textColor = Color.green;
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

    private void OnMouseDown()
    {
        gameObject.SendMessageUpwards("BlockClicked", gameObject);
    }

    private void OnMouseUpAsButton()
    {
        // gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void BlockClicked(GameObject obj)
    {
        return;
    }

    public void UpdateSelected(GameObject obj)
    {
        if (obj == gameObject)
            gameObject.GetComponent<SpriteRenderer>().color = Color.gray;
        else
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void OnGUI()
    {
        if (showInfo)
        {
            GUI.Label(new Rect(Input.mousePosition.x, Screen.height-Input.mousePosition.y-15, 30, 30), block.HPCount.Data.ToString(),HPStyle);
        }
        if (block == null) return;
        Vector2 pos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(pos.x - 10, Screen.height - pos.y - 20, 30, 30), (block.HPCount.Data+block.NIPCount.Data).ToString(), HPStyle);
        GUI.Label(new Rect(pos.x - 10, Screen.height - pos.y + 0, 30, 30), block.CIPCount.Data.ToString(), IPStyle);
        GUI.Label(new Rect(pos.x - 10, Screen.height - pos.y + 20, 30, 30), block.MaterialCount.Data.ToString(), MStyle);
    }
}
