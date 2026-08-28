using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    [SerializeField] GameObject menuCanvas;

    public Image[] tabImages;

    public GameObject[] pages;

    void Start()
    {
        ActiveTab(1);
    }

    public void ActiveTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.gray;
        }

        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }

    public void SettingsButton()
    {
        menuCanvas.SetActive(!menuCanvas.activeSelf);
        ActiveTab(3);
    }
}