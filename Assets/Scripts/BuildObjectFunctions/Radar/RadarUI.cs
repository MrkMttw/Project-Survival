using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadarUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text progressText;
    public Image progressBar;

    private void Start()
    {
        RadarController radarController =
            FindFirstObjectByType<RadarController>();

        if (radarController != null)
        {
            radarController.RegisterRadarUI(this);
        }
    }

    public void UpdateUI(
        int level,
        int currentIron,
        int requiredIron
    )
    {
        levelText.text =
            "Radar (Level " + level + ")";

        progressText.text =
            currentIron + " / " + requiredIron;

        if (requiredIron > 0)
        {
            progressBar.fillAmount =
                Mathf.Clamp01(
                    (float)currentIron / requiredIron
                );
        }
        else
        {
            progressBar.fillAmount = 1f;
        }
    }
}