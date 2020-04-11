using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Landblock : MonoBehaviour
{

    public Block block;

    /* Following variabls are used for custom initialization.
     * Their values will be referred to in contructor of the block.
     */
    // number of healthy population in the block
    // will have no effect if random generation flag in VirusModel is set
    public int population;

    // number of infected population in the block
    public int infected;

    // number of virus in the block
    public int virus;

    // number of material in the block
    public int material;

    // determine type of block

    public BlockType type;

    public GameObject[] customOutLandBlocks; 
    

    private bool showInfo;

    // GUI related parameters, used in OnGUI()
    GUIStyle HPStyle;
    GUIStyle IPStyle;
    GUIStyle MStyle;
    
    // Start is called before the first frame update
    void Start()
    {

        showInfo = false;
        HPStyle = new GUIStyle();
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
        foreach (Text i in this.GetComponentInChildren<Canvas>().GetComponentsInChildren<Text>())
        {
            switch (i.name)
            {
                case "HPUI":
                    i.text = (block.HPCount.Data + block.NIPCount.Data).ToString();
                    break;
                case "CIPUI":
                    i.text = block.CIPCount.Data.ToString();
                    break;                  
                case "MPUI":
                    i.text = block.MaterialCount.Data.ToString();
                    break;
                default:
                    break;
            }
        }
    }
}
