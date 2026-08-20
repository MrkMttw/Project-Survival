using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject PlayScreen;
    public GameObject CreateWorld;
    void Start()
    {
        MainMenu.SetActive(true);
        PlayScreen.SetActive(false);
        CreateWorld.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenu.SetActive(true);
            PlayScreen.SetActive(false);
            CreateWorld.SetActive(false);
        }
    }
    public void OnClickPlay()
    {
        PlayScreen.SetActive(true);
        MainMenu.SetActive(false);
        CreateWorld.SetActive(false);
    }

    public void OnClickBack()
    {
        MainMenu.SetActive(true);
        PlayScreen.SetActive(false);
        CreateWorld.SetActive(false);
    }
    
    public void OnClickCreateWorldBack()
    {
        MainMenu.SetActive(false);
        PlayScreen.SetActive(true);
        CreateWorld.SetActive(false);
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
    }
}
