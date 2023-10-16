using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private int frontSpriteID; // Identifier for the card's front sprite
    private int cardID; // Identifier for the card
    private bool isFlipped; // Whether the card is facing front or back
    private bool isTurning; // Whether the card is currently in the process of turning
    [SerializeField]
    private SpriteRenderer cardRenderer; // Reference to the card's SpriteRenderer component

    // Coroutine for a 90-degree flip animation
    // If changeSprite is true, it will switch to the back/front sprite before flipping another 90 degrees
    private IEnumerator Flip90Degrees(Transform thisTransform, float time, bool changeSprite)
    {
        Quaternion startRotation = thisTransform.rotation;
        Quaternion endRotation = thisTransform.rotation * Quaternion.Euler(new Vector3(0, 90, 0));
        float rate = 1.0f / time;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            thisTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }
        // Change the sprite and flip another 90 degrees if needed
        if (changeSprite)
        {
            isFlipped = !isFlipped;
            ChangeCardSprite();
            StartCoroutine(Flip90Degrees(transform, time, false));
        }
        else
            isTurning = false;
    }

    // Perform a 180-degree flip
    public void FlipCard()
    {
        isTurning = true;
        StartCoroutine(Flip90Degrees(transform, 0.25f, true));
    }

    // Toggle between front and back sprite
    private void ChangeCardSprite()
    {
        if (frontSpriteID == -1 || cardRenderer == null) return;
        if (isFlipped)
            cardRenderer.sprite = CardGameManager.Instance.GetCardFrontSprite(frontSpriteID);
        else
            cardRenderer.sprite = CardGameManager.Instance.GetCardBackSprite();
    }

    // Call a fade animation to make the card inactive
    public void DeactivateCard()
    {
        StartCoroutine(FadeCard());
    }

    // Play a fade animation by changing the alpha of the card's color
    private IEnumerator FadeCard()
    {
        float rate = 1.0f / 2.5f;
        float t = 0.0f;
        Color originalColor = cardRenderer.color;
        Color targetColor = Color.clear;
        while (t < 1.0f)
        {
            t += Time.deltaTime * rate;
            cardRenderer.color = Color.Lerp(originalColor, targetColor, t);

            yield return null;
        }
    }

    // Set the card to be active by restoring its color
    public void ActivateCard()
    {
        if (cardRenderer)
            cardRenderer.color = Color.white;
    }

    // FrontSpriteID getter and setter
    public int FrontSpriteID
    {
        set
        {
            frontSpriteID = value;
            isFlipped = true;
            ChangeCardSprite();
        }
        get { return frontSpriteID; }
    }

    // CardID getter and setter
    public int CardID
    {
        set { cardID = value; }
        get { return cardID; }
    }

    // Reset the card's default rotation to face the player
    public void ResetCardRotation()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        isFlipped = true;
    }

    // Card button click event handler
    public void OnCardClick()
    {
        if (isFlipped || isTurning) return;
        if (!CardGameManager.Instance.CanClick()) return;
        FlipCard();
        StartCoroutine(NotifyGameManagerOfSelection());
    }

    // Inform the GameManager that a card is selected with a slight delay
    private IEnumerator NotifyGameManagerOfSelection()
    {
        yield return new WaitForSeconds(0.5f);
        CardGameManager.Instance.CardClicked(frontSpriteID, cardID);
    }
}
