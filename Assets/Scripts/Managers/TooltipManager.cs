using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    [Header("References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemTypeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waterText;
    public TextMeshProUGUI soilText;
    public TextMeshProUGUI biodiversityText;
    public TextMeshProUGUI aestheticText;

    private bool isInventoryTooltip = false;

    void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);

            Canvas canvas = tooltipPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 100;
            }
        }
    }

    void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            if (isInventoryTooltip)
            {
                PositionTooltipAbove();
            }
            else
            {
                PositionTooltipUnder();
            }
        }
    }

    public void ShowTooltip(
        string itemName,
        string itemType,
        int score,
        int water,
        int soil,
        int biodiversity,
        int aesthetic,
        int bonusScore = 0)
    {
        if (tooltipPanel == null) return;

        tooltipPanel.SetActive(true);
        isInventoryTooltip = false;

        itemNameText.text = itemName;
        itemTypeText.text = $"Type: {itemType}";

        string scoreDisplay = score.ToString();
        if (bonusScore != 0)
        {
            scoreDisplay += $" (+{bonusScore})";
        }
        scoreText.text = $"Score: {scoreDisplay}";

        waterText.text = $"Water: {water}";
        soilText.text = $"Soil: {soil}";
        biodiversityText.text = $"Biodiversity: {biodiversity}";
        aestheticText.text = $"Aesthetic: {aesthetic}";

        PositionTooltipUnder();
    }

    public void ShowInventoryTooltip(
        string itemName,
        string itemType,
        int score = 0,
        int water = 0,
        int soil = 0,
        int biodiversity = 0,
        int aesthetic = 0,
        bool isPowerUp = false,
        bool isObstacle = false,
        bool isSabotage = false)
    {
        if (tooltipPanel == null) return;

        tooltipPanel.SetActive(true);
        isInventoryTooltip = true;

        itemNameText.text = itemName;

        if (isPowerUp)
        {
            itemTypeText.text = $"{itemType}";
            scoreText.text = "";
            waterText.text = "";
            soilText.text = "";
            biodiversityText.text = "";
            aestheticText.text = "";
            PositionTooltipAbove();
            return;
        }

        if (isSabotage)
        {
            itemTypeText.text = $"{itemType}";
            scoreText.text = "";
            waterText.text = "";
            soilText.text = "";
            biodiversityText.text = "";
            aestheticText.text = "";
            PositionTooltipAbove();
            return;
        }

        if (isObstacle)
        {
            itemTypeText.text = $"{itemType}";
        }
        else
        {
            itemTypeText.text = $"{itemType}";
        }

        scoreText.text = $"Score: {score}";
        waterText.text = $"Water: {water}";
        soilText.text = $"Soil: {soil}";
        biodiversityText.text = $"Biodiversity: {biodiversity}";
        aestheticText.text = $"Aesthetic: {aesthetic}";

        PositionTooltipAbove();
    }

    void PositionTooltipUnder()
    {
        RectTransform rectTransform = tooltipPanel.GetComponent<RectTransform>();
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            Input.mousePosition,
            null,
            out mousePos
        );

        rectTransform.anchoredPosition = mousePos + new Vector2(0, -110);
    }

    void PositionTooltipAbove()
    {
        RectTransform rectTransform = tooltipPanel.GetComponent<RectTransform>();
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            Input.mousePosition,
            null,
            out mousePos
        );

        rectTransform.anchoredPosition = mousePos + new Vector2(0, 110);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
        isInventoryTooltip = false;
    }
}