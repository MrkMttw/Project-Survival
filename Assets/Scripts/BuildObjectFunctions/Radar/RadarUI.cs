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
    public string coordinatesObjectName = "CoordinatesText";

    private RadarController radarController;

    private void Start()
    {
        radarController =
            FindFirstObjectByType<RadarController>();

        if (coordinatesText == null)
        {
            TMP_Text[] texts = FindObjectsByType<TMP_Text>(
                FindObjectsSortMode.None
            );

            foreach (TMP_Text text in texts)
            {
                if (text.gameObject.name == coordinatesObjectName)
                {
                    coordinatesText = text;
                    break;
                }
            }
        }

        if (radarController != null)
        {
            radarController.RegisterRadarUI(this);
            UpdateCoordinatesUI();
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