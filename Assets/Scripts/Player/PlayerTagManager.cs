using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerTagManager : MonoBehaviour
{
    [Header("References")]
    public GameObject tagPrefab;
    public RectTransform canvasRect;

    [Header("Posities (boven elke tuin)")]
    public Vector3[] worldPositions = new Vector3[4];

    private List<RectTransform> activeTags = new List<RectTransform>();

    void Start()
    {
        if (canvasRect == null)
            canvasRect = GetComponent<RectTransform>();
    }

    public void SetPlayerTags(List<string> playerNames)
    {
        foreach (RectTransform tag in activeTags)
        {
            Destroy(tag.gameObject);
        }
        activeTags.Clear();

        if (worldPositions.Length < playerNames.Count)
        {
            Debug.LogWarning("⚠️ Niet genoeg posities!");
            return;
        }

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject tag = Instantiate(tagPrefab, canvasRect);
            RectTransform rect = tag.GetComponent<RectTransform>();

            TextMeshProUGUI text = tag.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = playerNames[i];
            }

            Vector3 worldPos = worldPositions[i];
            worldPos.y += 3f;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            rect.position = screenPos;

            activeTags.Add(rect);
        }
    }
}