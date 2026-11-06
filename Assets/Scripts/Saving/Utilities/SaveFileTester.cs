using UnityEngine;

public class SaveFileTester : MonoBehaviour
{
    [System.Serializable]
    public class TestData
    {
        public string worldName;
        public int worldSeed;
    }

    private void Start()
    {
        string path = Application.persistentDataPath + "/testWorld.json";

        bool restored = SaveBackupHandler.RestoreLatestBackup<TestData>(
            path,
            data => !string.IsNullOrEmpty(data.worldName) && data.worldSeed > 0
        );
        Debug.Log("Recovery successful: " + restored);

        TestData data = SaveFileHandler.LoadFromJson<TestData>(path);

        if (data != null)
        {
            Debug.Log("World Name: " + data.worldName);
            Debug.Log("World Seed: " + data.worldSeed);
        }
    }
}