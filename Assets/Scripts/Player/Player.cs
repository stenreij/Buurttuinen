using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerIndex;
    public string playerName;
    public int score;

    private Inventory inventory;

    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log(playerName + " heeft nu " + score + " punten");
    }
}