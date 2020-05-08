using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        titleStyle.fontSize = 14;
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
        // You can use Utility.GetManager()(defined in Basics.cs)
        GameObject[] res = GameObject.FindGameObjectsWithTag("GameController");
        if (res.Length >= 1)
        {
            res[0].GetComponent<Landscape>().CardEventDispatched(this);
        }
    }

    private void OnGUI()
    {
        Canvas[] cvs = GetComponentsInChildren<Canvas>(true);
        Canvas cv = null;
        string name = "Canvas";
        if (Utility.GetVirusModel().enableUIVer2)
        {
            name = "CanvasVer2";
        }
        foreach (Canvas ele in cvs)
        {
            if (ele.name == name)
            {
                ele.gameObject.SetActive(true);
                cv = ele;
            } else
            {
                ele.gameObject.SetActive(false);
            }
        }
        if (cv == null) return;
        if (name == "CanvasVer2")
        {
            SpriteRenderer sr = this.GetComponent<SpriteRenderer>();
            sr.sprite = Utility.GetSprite(SpriteType.CARD);
            Image icon = cv.GetComponentInChildren<Image>();
            icon.sprite = Utility.GetSprite(Utility.CardToSpriteType(type));

            foreach (Text i in cv.GetComponentsInChildren<Text>())
            {
                switch (i.name)
                {
                    case "costUI":
                        i.text = cost.ToString();
                        i.fontSize = 14;
                        break;
                    case "Title":
                        i.text = cardName;
                        break;
                    default:
                        break;
                }
            }
            return;
        }
        foreach (Text i in cv.GetComponentsInChildren<Text>())
        {
            switch (i.name)
            {
                case "costUI":
                    i.text = cost.ToString();
                    i.fontSize = 14;
                    break;
                case "Title":
                    i.text = cardName;
                    break;              
                default:
                    break;
            }
        }
    }

}
