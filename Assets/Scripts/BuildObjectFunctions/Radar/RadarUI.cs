using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadarUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text progressText;
    public Image progressBar;
    public TMP_Text coordinatesText;

    private RadarController radarController;

    private void Start()
    {
        radarController =
            FindFirstObjectByType<RadarController>();

        if (radarController != null)
        {
            radarController.RegisterRadarUI(this);
            UpdateCoordinatesUI();
        }
        else
        {
            Debug.LogError("[RADAR UI] RadarController NOT FOUND!");
        }
    }

    public void SetCoordinatesVisible(bool visible)
    {
        if (coordinatesText == null)
            return;

        coordinatesText.gameObject.SetActive(visible);
    }

    public void UpdateUI(
        int level,
        int currentIron,
        int requiredIron
    )
    {
        if (levelText != null)
            levelText.text = "Radar (Level " + level + ")";

        if (progressText != null)
            progressText.text = currentIron + " / " + requiredIron;

        if (progressBar != null)
        {
            progressBar.fillAmount = requiredIron > 0
                ? Mathf.Clamp01((float)currentIron / requiredIron)
                : 1f;
        }

        UpdateCoordinatesUI();
    }

    private void UpdateCoordinatesUI()
    {
        if (coordinatesText == null || radarController == null)
            return;

        coordinatesText.text =
            radarController.GetUnlockedCoordinatesText();
    }

    public void RefreshCoordinatesUI()
    {
        UpdateCoordinatesUI();
    }
}