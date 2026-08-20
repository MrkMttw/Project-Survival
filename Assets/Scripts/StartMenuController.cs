using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject PlayScreen;
    void Start()
    {
        MainMenu.SetActive(true);
        PlayScreen.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenu.SetActive(true);
            PlayScreen.SetActive(false);
        }
    }
    public void OnClickPlay()
    {
        PlayScreen.SetActive(true);
        MainMenu.SetActive(!MainMenu.activeSelf);
    }

    public void OnClickBack()
    {
        MainMenu.SetActive(true);
        PlayScreen.SetActive(!PlayScreen.activeSelf);
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
}
