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
    private RectTransform cardContainer;


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


    public RectTransform panelRow;

    private int score = 0;
    public Text MainMenuTxt, GamePlayTxt;

    void Awake()
    {
        Instance = this;

    }

    void Start()
    {
        isGameStarted = false;
        gamePanel.SetActive(false);
        scoreUpdate();
    }
    void scoreUpdate()
    {
        MainMenuTxt.text = PlayerPrefs.GetInt("PlayerScore").ToString();
    }
    public void StartCardGame()
    {
        if (isGameStarted)
            return;

        isGameStarted = true;
        MenuPanel.SetActive(false);
        gamePanel.SetActive(true);
       
        SetGamePanel();
        selectedCardIndex = selectedSpriteIndex = -1;
        remainingCards = cards.Length;
        SpriteCardAllocation();
        StartCoroutine(HideCardFaces());
    }

    public void ClearGrid()
    {
        for (int count = 0; count < cardContainer.childCount; count++)
        {
            Destroy(cardContainer.GetChild(count).gameObject);
        }
    }

    private void SetGamePanel()
    {
        ClearGrid();



        GameObject cellInputField;
        RectTransform rowParent;
        
        cards = new Card[gameSize];
        for (int rowIndex = 0; rowIndex < Rows; rowIndex++)
        {
            rowParent = (RectTransform)Instantiate(panelRow);
            rowParent.transform.SetParent(cardContainer);
            rowParent.transform.localScale = Vector3.one;
            for (int colIndex = 0; colIndex < Cols; colIndex++)
            {
                cellInputField = (GameObject)Instantiate(cardPrefab);
                cellInputField.transform.SetParent(rowParent);
                cellInputField.GetComponent<RectTransform>().localScale = Vector3.one;
                // Create a new card
                Card c= cellInputField.GetComponent<Card>();

                // Set the CardID for the card
                int index = rowIndex * Cols + colIndex;
                c.CardID = index;

                // Store the card in the cards array
                cards[index] = c;
            }
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
            
            if (numRows % 2 == 1 && numCols % 2 == 1)
            {
                sizeTextLabel.text = "Worng both are Odd number Not Possible";
               
            }
            else
            {
                gameSize = (numRows * numCols);
                sizeTextLabel.text = numRows + " X " + numCols;
                StartBtn.SetActive(true);
                Rows = numRows;
                Cols = numCols;
            }
            
        }
        else
        {
            sizeTextLabel.text = "Correct_Format 2x2,2x3,etc";
        }
    }

    int Rows, Cols;
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
                AddToScore(10); // Add 10 points on each match.
                SaveScore();    // Save the score.
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
    private void AddToScore(int pointsToAdd)
    {
        score += pointsToAdd;
       GamePlayTxt.text = "Score : "+ score.ToString();
    }

    private void SaveScore()
    {
        PlayerPrefs.SetInt("PlayerScore", score);
        PlayerPrefs.Save(); 
    }

    private void EndGame()
    {
        isGameStarted = false;
        gamePanel.SetActive(false);
        MenuPanel.SetActive(true);
        StartBtn.SetActive(false);
        scoreUpdate();
        AudioManager.Instance.PlayAudioClip(3);
    }
}
