using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameManager : MonoBehaviour
{
    public static CardGameManager Instance;
    public static int gameSize = 2;
   
    [SerializeField]
    private Sprite cardBackSprite;

    [SerializeField]
    private Sprite[] cardFrontSprites;

    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private GameObject cardContainer;


    private Card[] cards;

    [SerializeField]
    private GameObject gamePanel;

    [SerializeField]
    private GameObject winPanel;

    [SerializeField]
    private Text sizeTextLabel;


    private bool isGameStarted;

    private int selectedSpriteIndex;
    private int selectedCardIndex;
    private int remainingCards;

    [SerializeField]
    private InputField sizeInputField;

    [SerializeField]
    private GameObject StartBtn;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        isGameStarted = false;
        gamePanel.SetActive(false);
    }
    public void StartCardGame()
    {
        if (isGameStarted)
            return;

        isGameStarted = true;
        gamePanel.SetActive(true);
        winPanel.SetActive(false);
        SetGamePanel();
        selectedCardIndex = selectedSpriteIndex = -1;
        remainingCards = cards.Length;
        SpriteCardAllocation();
        StartCoroutine(HideCardFaces());
    }

    private void SetGamePanel()
    {
        int isOdd = gameSize % 2;
        cards = new Card[gameSize * gameSize - isOdd];

        foreach (Transform child in cardContainer.transform)
        {
            Destroy(child.gameObject);
        }

        RectTransform panelSize = gamePanel.transform.GetComponent<RectTransform>();
        float rowSize = panelSize.sizeDelta.x;
        float colSize = panelSize.sizeDelta.y;
        float scale = 1.0f / gameSize;
        float xIncrement = rowSize / gameSize;
        float yIncrement = colSize / gameSize;
        float currentX = -xIncrement * (float)(gameSize / 2);
        float currentY = -yIncrement * (float)(gameSize / 2);

        if (isOdd == 0)
        {
            currentX += xIncrement / 2;
            currentY += yIncrement / 2;
        }
        float initialX = currentX;

        for (int i = 0; i < gameSize; i++)
        {
            currentX = initialX;

            for (int j = 0; j < gameSize; j++)
            {
                GameObject card;

                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    card = cards[index].gameObject;
                }
                else
                {
                    card = Instantiate(cardPrefab);
                    card.transform.parent = cardContainer.transform;
                    int index = i * gameSize + j;
                    cards[index] = card.GetComponent<Card>();
                    cards[index].CardID = index;
                    card.transform.localScale = new Vector3(scale, scale);
                }

                card.transform.localPosition = new Vector3(currentX, currentY, 0);
                currentX += xIncrement;
            }
            currentY += yIncrement;
        }
    }
    void ResetCardFaces()
    {
        for (int i = 0; i < gameSize; i++)
            cards[i].ResetCardRotation();
    }

    IEnumerator HideCardFaces()
    {
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < cards.Length; i++)
            cards[i].FlipCard();

        yield return new WaitForSeconds(0.5f);
    }


    private void SpriteCardAllocation()
    {
        int[] selectedIDs = new int[cards.Length / 2];

        for (int i = 0; i < cards.Length / 2; i++)
        {
            int value = Random.Range(0, cardFrontSprites.Length - 1);

            for (int j = i; j > 0; j--)
            {
                if (selectedIDs[j - 1] == value)
                    value = (value + 1) % cardFrontSprites.Length;
            }
            selectedIDs[i] = value;
        }

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].ActivateCard();
            cards[i].CardID = -1;
            cards[i].ResetCardRotation();
        }

        for (int i = 0; i < cards.Length / 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].CardID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].CardID = selectedIDs[i];
            }
        }
    }

    public void SetGameSize()
    {
        string inputText = sizeInputField.text;
        string[] sizeParts = inputText.Split('x');

        if (sizeParts.Length == 2 && int.TryParse(sizeParts[0], out int numRows) && int.TryParse(sizeParts[1], out int numCols))
        {
            gameSize = (numRows * numCols) / 2;
            sizeTextLabel.text = numRows + " X " + numCols;
            StartBtn.SetActive(true);

        }
        else
        {
            sizeTextLabel.text = "Correct_Format 2x2,2x3,etc";
        }
    }

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


    public bool CanClick()
    {
        return isGameStarted;
    }

    public void CardClicked(int spriteId, int cardId)
    {
        if (selectedSpriteIndex == -1)
        {
            selectedSpriteIndex = spriteId;
            selectedCardIndex = cardId;
        }
        else
        {
            if (selectedSpriteIndex == spriteId)
            {
                cards[selectedCardIndex].DeactivateCard();
                cards[cardId].DeactivateCard();
                remainingCards -= 2;
                CheckGameWin();
            }
            else
            {
                cards[selectedCardIndex].FlipCard();
                cards[cardId].FlipCard();
            }
            selectedCardIndex = selectedSpriteIndex = -1;
        }
    }
    private void CheckGameWin()
    {
        if (remainingCards == 0)
        {
            EndGame();
        }
    }
    public void DisplayInfoPanel(bool show)
    {
        winPanel.SetActive(show);
    }

    private void EndGame()
    {
        isGameStarted = false;
        gamePanel.SetActive(false);
    }
}
