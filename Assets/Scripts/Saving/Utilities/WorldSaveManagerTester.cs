using UnityEngine;

public class WorldSaveManagerTester : MonoBehaviour
{
    private void Start()
    {
        // INITIALIZE SAVE SYSTEM
        if (!WorldSaveManager.Initialize())
        {
            Debug.LogError("INITIALIZATION FAILED");
            return;
        }

        // CHECK REGISTERED WORLDS
        var worlds = WorldSaveManager.GetAllWorlds();

        Debug.Log("INITIALIZATION SUCCESSFUL");
        Debug.Log("TOTAL REGISTERED WORLDS: " + worlds.Count);

        foreach (var world in worlds)
        {
            Debug.Log(
                "WORLD: " + world.worldName +
                " | ID: " + world.worldID +
                " | SEED: " + world.worldSeed
            );
        }
    }
}