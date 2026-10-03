using UnityEngine;

public class RadarController : MonoBehaviour
{
    [Header("Radar Level")]
    public int progressionIron = 0;

    [SerializeField]
    private int radarLevel = 0;

    private RadarUI radarUI;

    public int RadarLevel
    {
        get { return radarLevel; }
    }

    public float RadarRadius
    {
        get
        {
            switch (radarLevel)
            {
                case 1:
                    return 30f;

                case 2:
                    return 60f;

                case 3:
                    return 90f;

                case 4:
                    return 120f;

                case 5:
                    return 150f;

                default:
                    return 0f;
            }
        }
    }

    public void AddIron(int amount)
    {
        if (amount <= 0)
            return;

        if (radarLevel >= 5)
            return;

        progressionIron += amount;

        UpdateRadarLevel();
        UpdateRadarUI();
    }

    private void UpdateRadarLevel()
    {
        if (radarLevel >= 5)
            return;

        int requiredIron = GetNextLevelRequirement();

        if (progressionIron >= requiredIron)
        {
            progressionIron -= requiredIron;

            radarLevel++;

            UpdateRadarLevel();
        }
    }

    private int GetNextLevelRequirement()
    {
        switch (radarLevel)
        {
            case 0:
                return 3;

            case 1:
                return 6;

            case 2:
                return 9;

            case 3:
                return 12;

            case 4:
                return 15;

            case 5:
                return 15;

            default:
                return 3;
        }
    }

    public void RegisterRadarUI(RadarUI ui)
    {
        radarUI = ui;
        UpdateRadarUI();
    }

    private void UpdateRadarUI()
    {
        if (radarUI == null)
            return;

        int requiredIron = GetNextLevelRequirement();

        int displayedIron =
            Mathf.Min(
                progressionIron,
                requiredIron
            );

        radarUI.UpdateUI(
            radarLevel,
            displayedIron,
            requiredIron
        );
    }
}