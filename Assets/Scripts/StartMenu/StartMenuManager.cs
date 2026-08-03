using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField[] nameInputs;
    public TextMeshProUGUI countText;
    public Button startButton;
    public Button minusButton;
    public Button plusButton;

    private int playerCount = 2;
    private const int minPlayers = 2;
    private const int maxPlayers = 4;

    void Start()
    {
        UpdatePlayerCount();
        UpdateNameInputs();

        minusButton.onClick.AddListener(OnMinusClicked);
        plusButton.onClick.AddListener(OnPlusClicked);
        startButton.onClick.AddListener(OnStartClicked);
    }

    void OnMinusClicked()
    {
        if (playerCount > minPlayers)
        {
            playerCount--;
            UpdatePlayerCount();
            UpdateNameInputs();
        }
    }

    void OnPlusClicked()
    {
        if (playerCount < maxPlayers)
        {
            playerCount++;
            UpdatePlayerCount();
            UpdateNameInputs();
        }
    }

    void UpdatePlayerCount()
    {
        countText.text = playerCount.ToString();
    }

    void UpdateNameInputs()
    {
        for (int i = 0; i < nameInputs.Length; i++)
        {
            nameInputs[i].gameObject.SetActive(i < playerCount);
        }
    }

    void OnStartClicked()
    {
        List<string> playerNames = new List<string>();
        for (int i = 0; i < playerCount; i++)
        {
            string name = nameInputs[i].text;
            if (string.IsNullOrEmpty(name))
            {
                name = "Speler " + (i + 1);
            }
            playerNames.Add(name);
            Debug.Log($"📝 Player {i + 1}: {name}");
        }

        // Save player names to GameSetup and load the main game scene
        GameSetup setup = FindObjectOfType<GameSetup>();
        if (setup == null)
        {
            GameObject setupGO = new GameObject("GameSetup");
            setup = setupGO.AddComponent<GameSetup>();
            DontDestroyOnLoad(setupGO);
        }
        setup.playerNames = playerNames;

        SceneManager.LoadScene("TheHood");
    }
}