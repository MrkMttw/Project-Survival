using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public GameObject MainMenu; 
    public GameObject PlayScreen;
    public GameObject CreateWorld;
    public GameObject WorldSettings;

    void Start()
    {
        MainMenu.SetActive(true);
        PlayScreen.SetActive(false);
        CreateWorld.SetActive(false);
        WorldSettings.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenu.SetActive(true);
            PlayScreen.SetActive(false);
            CreateWorld.SetActive(false);
            WorldSettings.SetActive(false);
        }
    }

    public void OnClickPlay()
    {
        if (!WorldSaveManager.Initialize())
        {
            Debug.LogError("FAILED TO INITIALIZE WORLD SAVE SYSTEM.");
            return;
        }

        PlayScreen.SetActive(true);
        MainMenu.SetActive(false);
        CreateWorld.SetActive(false);
        WorldSettings.SetActive(false);
    }

    public void OnClickBack()
    {
        MainMenu.SetActive(true);
        PlayScreen.SetActive(false);
        CreateWorld.SetActive(false);
        WorldSettings.SetActive(false);
    }

    public void OnClickCreateWorldBack()
    {
        MainMenu.SetActive(false);
        PlayScreen.SetActive(true);
        CreateWorld.SetActive(false);
        WorldSettings.SetActive(false);
    }

    public void OnClickWorldSettingBack()
    {
        CreateWorld.SetActive(true);
        MainMenu.SetActive(false);
        PlayScreen.SetActive(false);
        WorldSettings.SetActive(false);
    }

    public void OnExitClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    public void OnClickLoadScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickCreateWorld()
    {
        CreateWorld.SetActive(true);
        MainMenu.SetActive(false);
        PlayScreen.SetActive(false);
        WorldSettings.SetActive(false);
    }

    public void OnClickWorldSetting()
    {
        CreateWorld.SetActive(false);
        MainMenu.SetActive(false);
        PlayScreen.SetActive(false);
        WorldSettings.SetActive(true);
    }
}