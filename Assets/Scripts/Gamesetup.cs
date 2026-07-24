using UnityEngine;
using System.Collections.Generic;

public class GameSetup : MonoBehaviour
{
    public List<string> playerNames = new List<string>();

    void Awake()
    {
        if (FindObjectsOfType<GameSetup>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}