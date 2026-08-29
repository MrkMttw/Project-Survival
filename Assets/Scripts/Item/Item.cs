using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;

    public virtual void UseItem()
    {
        Debug.Log("Using item: " + name);
    }
}