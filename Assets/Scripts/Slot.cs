using UnityEngine;

/// <summary>
/// Represents a single slot in the inventory system.
/// Each slot can hold one item and tracks the currently assigned item.
/// </summary>
public class Slot : MonoBehaviour
{
    /// <summary>
    /// The item GameObject currently occupying this slot.
    /// Set to null when the slot is empty.
    /// </summary>
    public GameObject currentItem;
}
