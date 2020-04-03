using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    QUARANTINE,
    STOP_WORKING,
    START_WORKING,
    SPECIAL_AID,
    TAXING
}

public class Card : MonoBehaviour
{
    public CardType type;
    public int cost;

    public string cardName;
    GUIStyle titleStyle;

    // Start is called before the first frame update
    void Start()
    {
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 16;
        titleStyle.normal.textColor = Color.black;

        switch (type)
        {
            case CardType.QUARANTINE:
                {
                    cardName = "隔离(1)";
                    break;
                }
            case CardType.STOP_WORKING:
                {
                    cardName = "停工(2)";
                    break;
                }
            case CardType.START_WORKING:
                {
                    cardName = "开工(3)";
                    break;
                }
            case CardType.SPECIAL_AID:
                {
                    cardName = "援助(4)";
                    break;
                }
            case CardType.TAXING:
                {
                    cardName = "征税(5)";
                    break;
                }
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    private void OnMouseUpAsButton()
    {
        GameObject[] res = GameObject.FindGameObjectsWithTag("GameController");
        if (res.Length >= 1)
        {
            res[0].GetComponent<Landscape>().CardEventDispatched(this);
        }
    }

    private void OnGUI()
    {
        Vector2 pos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(pos.x - 40, Screen.height - pos.y - 35, 60, 30), cardName,titleStyle);
        GUI.Label(new Rect(pos.x - 40, Screen.height - pos.y + 5, 60, 30), "资源" + cost.ToString(), titleStyle);   
    }

}
