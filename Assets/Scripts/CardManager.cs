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

public class CardInfo
{
    // Card cardGrid = null;
    public static Dictionary<CardType, string> cardType2str = new Dictionary<CardType, string>
    {
        { CardType.QUARANTINE, "Quarantine" },
        { CardType.STOP_WORKING, "StopWorking" },
        { CardType.START_WORKING, "StartWorking" },
        { CardType.SPECIAL_AID, "SpecialAid" },
        { CardType.TAXING, "Taxing" }
    };
    public static Dictionary<string, CardType> str2CardType = new Dictionary<string, CardType>();
    public static void Init()
    {
        foreach (var pair in cardType2str)
        {
            str2CardType[pair.Value] = pair.Key;
        }
    }
    public static Dictionary<CardType, CardInfo> cards = new Dictionary<CardType, CardInfo>();
    public static CardInfo GetCardInfo(CardType type)
    {
        if (!cards.ContainsKey(type))
            cards[type] = new CardInfo(type);
        return cards[type];
    }
    public static List<string> GetPropertyList()
    {
        string[] arr = new string[] { "type", "Cost", "CardName", "CardIntro" };
        return new List<string>(arr);
    }
    public List<string> GetValList(List<string> properties)
    {
        return null;
    }
    public CardType type;
    public CardInfo(CardType mtype)
    {
        type = mtype;
    }

    public int Cost { get; set; }
    public string CardName { get; set; }
    public string CardIntro { get; set; }

}

public class CardManager : MonoBehaviour
{
    Card[] cards;
    public Card[] cardGrids = new Card[5];
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
