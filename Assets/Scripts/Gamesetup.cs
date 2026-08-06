using UnityEngine;
using System.Collections.Generic;

public class GameSetup : MonoBehaviour
{
    public List<string> playerNames = new List<string>();
    public Dictionary<string, int> finalScores = new Dictionary<string, int>();
    public int totalScore = 0;

    void Awake()
    {
        if (FindObjectsByType<GameSetup>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}