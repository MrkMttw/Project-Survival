using UnityEngine;
using TMPro;

public class GameClock : MonoBehaviour
{
    [Header("Clock Settings")]
    [Tooltip("How many real seconds equal 1 in-game hour.")]
    public float realSecondsPerGameHour = 30f;

    [Tooltip("Starting hour (0-23).")]
    [Range(0, 23)]
    public int startingHour = 6;

    [Tooltip("Starting minute (0-59).")]
    [Range(0, 59)]
    public int startingMinute = 0;

    [Header("Starting Day")]
    public int startingDay = 1;

    [Header("Time Format")]
    [Tooltip("ON = 12-hour format (AM/PM), OFF = 24-hour format.")]
    public bool use12HourFormat = true;

    [Header("UI")]
    public TMP_Text clockText;
    public TMP_Text dayText;

    private float gameMinutes;
    private int day;

    void Start()
    {
        gameMinutes = (startingHour * 60) + startingMinute;
        day = startingDay;

        UpdateUI();
    }

    void Update()
    {
        // Convert real-time seconds into game minutes
        float gameMinutesPerSecond = 60f / realSecondsPerGameHour;

        gameMinutes += gameMinutesPerSecond * Time.deltaTime;

        // Midnight = new day
        if (gameMinutes >= 1440f)
        {
            gameMinutes -= 1440f;
            day++;

            Debug.Log("New Day: " + day);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int hours = Mathf.FloorToInt(gameMinutes / 60f);
        int minutes = Mathf.FloorToInt(gameMinutes % 60f);

        if (clockText != null)
        {
            if (use12HourFormat)
            {
                // 12-hour format
                string period = hours >= 12 ? "PM" : "AM";

                int displayHour = hours % 12;

                if (displayHour == 0)
                    displayHour = 12;

                clockText.text = string.Format(
                    "{0}:{1:00} {2}",
                    displayHour,
                    minutes,
                    period
                );
            }
            else
            {
                // 24-hour format
                clockText.text = string.Format(
                    "{0:00}:{1:00}",
                    hours,
                    minutes
                );
            }
        }

        // Day display
        if (dayText != null)
        {
            dayText.text = "Day " + day;
        }
    }

    public int GetHour()
    {
        return Mathf.FloorToInt(gameMinutes / 60f);
    }

    public int GetMinute()
    {
        return Mathf.FloorToInt(gameMinutes % 60f);
    }

    public int GetDay()
    {
        return day;
    }
}