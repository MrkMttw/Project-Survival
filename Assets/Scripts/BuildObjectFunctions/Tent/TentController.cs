
using UnityEngine;

public class TentController : MonoBehaviour
{
    [Header("World")]
    [SerializeField] private WorldGenerator worldGenerator;

    
    [Header("Tent Item Prefabs")]
    public Item[] tentItems;

    public int PlacedTentCount
    {
        get
        {
            if (worldGenerator == null)
                return 0;

            
            int[] tentItemIDs = new int[tentItems.Length];

            for (int i = 0; i < tentItems.Length; i++)
            {
                tentItemIDs[i] = GetItemID(tentItems[i]);
            }

            return worldGenerator.CountPlacedBuildingsWithItemIDs(
                tentItemIDs
            );
        }
    }

    public int DaysToAdvance => 1 + PlacedTentCount;

    private void Awake()
    {
        if (worldGenerator == null)
            worldGenerator = FindObjectOfType<WorldGenerator>();
    }

    public void DebugTentCount()
    {
        Debug.Log(
            "Placed Tents: " + PlacedTentCount +
            " | Days to Advance: " + DaysToAdvance
        );
    }

    private int GetItemID(Item item)
    {
        return item != null ? item.ID : 0;
    }
}
