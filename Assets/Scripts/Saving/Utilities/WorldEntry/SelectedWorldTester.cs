using UnityEngine;

public class SelectedWorldTester : MonoBehaviour
{
    private void Start()
    {
        if (SelectedWorldData.world == null)
        {
            Debug.LogError("NO SELECTED WORLD RECEIVED!");
            return;
        }

        Debug.Log("SELECTED WORLD RECEIVED!");
        Debug.Log("WORLD NAME: " + SelectedWorldData.world.worldName);
        Debug.Log("WORLD ID: " + SelectedWorldData.world.worldID);
        Debug.Log("WORLD SEED: " + SelectedWorldData.world.worldSeed);
    }
}