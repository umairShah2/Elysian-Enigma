using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGameManager : MonoBehaviour
{
    public static CardGameManager Instance;

    // Sprite for the card back
    [SerializeField]
    private Sprite cardBackSprite;

    // Array of possible card front sprites
    [SerializeField]
    private Sprite[] cardFrontSprites;


    // Get the card front sprite based on its ID
    public Sprite GetCardFrontSprite(int spriteId)
    {
        return cardFrontSprites[spriteId];
    }

    // Get the card back sprite
    public Sprite GetCardBackSprite()
    {
        return cardBackSprite;
    }

}
