using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Buurttuinen/Item")]
public class ItemData : ScriptableObject
{
    public Sprite icon;
    public string itemName;
    public ItemType type;
    public int score;
    public int water;
    public int soilHealth;
    public int biodiversity;
    public int esthetic;
    public Color color;
    public int maxAmount;
    public GameObject prefab;
}