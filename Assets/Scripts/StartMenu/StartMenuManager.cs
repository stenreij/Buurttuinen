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
        ItemDatabase oldDB = FindObjectOfType<ItemDatabase>();
        if (oldDB != null)
        {
            Destroy(oldDB.gameObject);
            Debug.Log("🗑️ Oude ItemDatabase verwijderd!");
        }

        GameSetup oldSetup = FindObjectOfType<GameSetup>();
        if (oldSetup != null)
        {
            Destroy(oldSetup.gameObject);
            Debug.Log("🗑️ Oude GameSetup verwijderd!");
        }

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