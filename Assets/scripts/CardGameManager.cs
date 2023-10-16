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
    private GameObject MenuPanel;

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
        MenuPanel.SetActive(false);
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
        // if game is odd, we should have 1 card less
        int isOdd = gameSize % 2;

        cards = new Card[gameSize * gameSize - isOdd];
        // remove all gameobject from parent
        foreach (Transform child in cardContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        // calculate position between each card & start position of each card based on the Panel
        RectTransform panelsize = gameObject.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f / gameSize;
        float xInc = row_size / gameSize;
        float yInc = col_size / gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if (isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }
        float initialX = curX;
        // for each in y-axis
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            // for each in x-axis
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                // if is the last card and game is odd, we instead move the middle card on the panel to last spot
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    // create card prefab
                    c = Instantiate(cardPrefab);
                    // assign parent
                    c.transform.parent = cardContainer.transform;

                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<Card>();
                    cards[index].CardID = index;
                    // modify its size
                    c.transform.localScale = new Vector3(scale, scale);
                }
                // assign location
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;

            }
            curY += yInc;
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
            cards[i].FrontSpriteID = -1;
            cards[i].ResetCardRotation();
        }

        for (int i = 0; i < cards.Length / 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].FrontSpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].FrontSpriteID = selectedIDs[i];
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
                AudioManager.Instance.PlayAudioClip(1);
                cards[selectedCardIndex].DeactivateCard();
                cards[cardId].DeactivateCard();
                remainingCards -= 2;
                CheckGameWin();
            }
            else
            {
                AudioManager.Instance.PlayAudioClip(2);
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
        AudioManager.Instance.PlayAudioClip(3);
    }
}
