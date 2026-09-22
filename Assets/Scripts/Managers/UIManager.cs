using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [Header("References")]
    public InventoryUI inventoryUI;
    public ScoreManager scoreManager;
    public CommunityManager communityManager;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void RefreshAllUI()
    {
        if (instance == null)
        {
            Debug.LogWarning("UIManager instance not found");
            return;
        }

        if (instance.inventoryUI != null)
            instance.inventoryUI.RefreshUI();

        if (instance.scoreManager != null)
            instance.scoreManager.UpdateScores();

        if (instance.communityManager != null)
            instance.communityManager.UpdateCommunityGoalScore();
    }
}