using UnityEngine;
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
    public TextMeshProUGUI estheticText;

    void Start()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    public void ShowTooltip(
        string itemName,
        string itemType,
        int score,
        int water,
        int soil,
        int biodiversity,
        int esthetic,
        int bonusScore = 0,
        int bonusWater = 0,
        int bonusSoil = 0,
        int bonusBiodiversity = 0,
        int bonusEsthetic = 0)
    {
        if (tooltipPanel == null) return;

        tooltipPanel.SetActive(true);

        itemNameText.text = itemName;
        itemTypeText.text = $"Type: {itemType}";

        string scoreDisplay = score.ToString();
        if (bonusScore != 0)
        {
            scoreDisplay += $" (+{bonusScore})";
        }
        scoreText.text = $"Score: {scoreDisplay}";

        string waterDisplay = water.ToString();
        if (bonusWater != 0)
        {
            waterDisplay += $" (+{bonusWater})";
        }
        waterText.text = $"Water: {waterDisplay}";

        string soilDisplay = soil.ToString();
        if (bonusSoil != 0)
        {
            soilDisplay += $" (+{bonusSoil})";
        }
        soilText.text = $"Soil: {soilDisplay}";

        string biodiversityDisplay = biodiversity.ToString();
        if (bonusBiodiversity != 0)
        {
            biodiversityDisplay += $" (+{bonusBiodiversity})";
        }
        biodiversityText.text = $"Biodiversity: {biodiversityDisplay}";

        string estheticDisplay = esthetic.ToString();
        if (bonusEsthetic != 0)
        {
            estheticDisplay += $" (+{bonusEsthetic})";
        }
        estheticText.text = $"Esthetic: {estheticDisplay}";

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

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}