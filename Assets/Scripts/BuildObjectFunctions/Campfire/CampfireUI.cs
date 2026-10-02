using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampfireUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text progressText;
    public Image progressBar;

    private void Start()
    {
        CampfireController campfireController =
            FindFirstObjectByType<CampfireController>();

        if (campfireController != null)
        {
            campfireController.RegisterCampfireUI(this);
        }
    }

    public void UpdateUI(
        int level,
        int currentWood,
        int requiredWood
    )
    {
        levelText.text =
            "Campfire (Level " + level + ")";

        progressText.text =
            currentWood + " / " + requiredWood;

        if (requiredWood > 0)
        {
            progressBar.fillAmount =
                Mathf.Clamp01(
                    (float)currentWood / requiredWood
                );
        }
        else
        {
            progressBar.fillAmount = 1f;
        }
    }
}