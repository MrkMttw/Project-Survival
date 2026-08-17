using Cinemachine;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Define save location
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        LoadGame();
    }

    // Update is called once per frame
    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
            /*add when map boundary is implemented
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectofType<CinemachineConfiner>().m_BoundingShape2D.name
            */
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }
    
    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
            /*add when map boundary is implemented
            FindObjectofType<CinemachineConfiner>().m_BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();
            */
        }
        else
        {
            SaveGame();
        }
    }
}
