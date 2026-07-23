using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Garden Settings")]
    public GameObject gardenPrefab;
    public int numberOfGardens = 4;

    [Header("Spacing (automatic)")]
    public float spacing = 2f;

    [Header("Fixed Positions (override automatic)")]
    public bool useFixedPositions = false;
    public Vector3[] fixedPositions = new Vector3[4];

    void Start()
    {
        CreateBoard();
    }

    public void CreateBoard()
    {
        for (int i = 0; i < numberOfGardens; i++)
        {
            Vector3 position = fixedPositions[i];
            CreateGarden(position, i + 1);
        }
    }


    void CreateGarden(Vector3 position, int index)
    {
        GameObject garden = Instantiate(gardenPrefab, position, Quaternion.identity, transform);
        garden.name = "Garden_" + index;

        Garden gardenScript = garden.GetComponent<Garden>();
        if (gardenScript != null)
        {
            gardenScript.CreateGarden();
        }
        else
        {
            Debug.LogError($"❌ No Garden script on {garden.name}!");
        }
    }
}