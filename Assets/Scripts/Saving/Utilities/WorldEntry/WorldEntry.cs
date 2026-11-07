using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public Image border;
    public TextMeshProUGUI worldName;
    public TextMeshProUGUI created;
    public TextMeshProUGUI lastPlayed;
    public TextMeshProUGUI seed;
    public Button button;

    [Header("Selection Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private WorldSaveData worldData;
    private Action<WorldSaveData> onSelected;

    private void Awake()
    {
        button.onClick.AddListener(SelectWorld);
    }

    // SETUP WORLD ENTRY
    public void Setup(
        WorldSaveData data,
        Action<WorldSaveData> selectionCallback)
    {
        worldData = data;
        onSelected = selectionCallback;

        worldName.text = data.worldName;

        created.text =
            "Created: " + FormatDate(data.creationDate);

        lastPlayed.text =
            "Last Played: " + FormatDate(data.lastPlayedDate);

        seed.text = "Seed: " + data.worldSeed;
    }

    public WorldSaveData GetWorldData()
    {
        return worldData;
    }

    // FORMAT DATE
    private string FormatDate(string date)
    {
        DateTime parsedDate;

        if (DateTime.TryParse(
            date,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out parsedDate))
        {
            return parsedDate.ToLocalTime().ToString("MMM dd, yyyy");
        }

        return "Unknown";
    }

    // SELECT WORLD
    private void SelectWorld()
    {
        if (worldData == null)
            return;

        onSelected?.Invoke(worldData);
    }

    // UPDATE SELECTION VISUAL
    public void SetSelected(bool selected)
    {
        if (border != null)
        {
            border.color =
                selected ? selectedColor : normalColor;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(SelectWorld);
    }
}