using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CardType
{
    QUARANTINE = 0,
    STOP_WORKING = 1,
    START_WORKING = 2,
    SPECIAL_AID = 3,
    TAXING = 4,
    NONE = 5
}

public class CardInfo
{
    // Card cardGrid = null;
    
    public static Dictionary<CardType, CardInfo> cards = new Dictionary<CardType, CardInfo>();
    public static CardInfo GetCardInfo(CardType type)
    {
        if (!cards.ContainsKey(type))
            cards[type] = new CardInfo(type);
        return cards[type];
    }

    public object GetVal(string property)
    {
        if (property == "type")
        {
            return Utility.CardTypeToString(type);
        }
        return Utility.GetVal(this, property);
        // return this.GetType().GetProperty(property).GetValue(this);
    }
    public void SetVal(string property, string value)
    {
        if (property == "type") return;
        Utility.SetVal(this, property, value);
    }

    public static List<string> GetPropertyList()
    {
        string[] arr = new string[] { "Cost", "CardName", "CardIntro" };
        return new List<string>(arr);
    }

    public List<string> GetValList(List<string> properties)
    {
        List<string> ret = new List<string>();
        foreach (var ele in properties)
        {
            ret.Add(GetVal(ele).ToString());
        }
        return ret;
    }
    public void SetValList(Dictionary<string, string> values)
    {
        foreach (var pair in values)
        {
            SetVal(pair.Key, pair.Value);
        }
    }

    public CardType type;
    public CardInfo(CardType mtype)
    {
        type = mtype;
        Cost = 0;
        CardName = "";
        CardIntro = "";
    }

    public int Cost { get; set; }
    public string CardName { get; set; }
    public string CardIntro { get; set; }

}

public class CardManager : MonoBehaviour
{
    // These are all cards that are available in the current level
    public CardType[] cardTypes;
    public Card[] cardGrids = new Card[5];

    int currentPage = 0;
    int maxPage = 0;

    Button up;
    Button down;
    Text pageInfo;
    // Start is called before the first frame update
    void Start()
    {
        maxPage = (cardTypes.Length+4) / 5;
        up = GameObject.Find("Canvas/CardPanel/Up").GetComponent<Button>();
        down = GameObject.Find("Canvas/CardPanel/Down").GetComponent<Button>();
        pageInfo = GameObject.Find("Canvas/CardPanel/Page").GetComponent<Text>();
        up.onClick.AddListener(OnPrevPage);
        down.onClick.AddListener(OnNextPage);
    }

    public void OnPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            // UpdateCard();
        }
    }

    public void OnNextPage()
    {
        if ((currentPage + 1) * 5 < cardTypes.Length)
        {
            currentPage++;
            // UpdateCard();
        }
    }

    void UpdateCard()
    {
        pageInfo.text = (currentPage + 1).ToString() + "/" + maxPage.ToString();
        for (int i = 0; i < 5; ++i)
        {
            if (currentPage * 5 + i >= cardTypes.Length) break;
            cardGrids[i].type = cardTypes[currentPage * 5 + i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCard();
    }
}
