using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerIndex;
    public string playerName;
    public int score = 0;
    public Garden assignedGarden;

    private Inventory inventory;
    public int soilBoost = 0;
    public int weatherBoost = 0;
    public int communityPenalty = 0;
    public int communityBoost = 0;

    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"{playerName} now has {score} points");
    }

    public void ResetBonuses()
    {
        soilBoost = 0;
        weatherBoost = 0;
        communityPenalty = 0;
        communityBoost = 0;
    }
}