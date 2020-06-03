using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfo
{
    Card cardGrid = null;
    public CardType type;

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
