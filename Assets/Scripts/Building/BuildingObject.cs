using UnityEngine;

public class BuildingObject : MonoBehaviour
{
    [Header("Building Information")]
    public int itemID;
    public string buildingID;

    [Header("Directional Building")]
    public int directionalPresetIndex = -1;

    [Header("ON/OFF State")]
    public bool hasOnOffState = false;
    public bool isOn = true;

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer onStateRenderer;
    [SerializeField] private GameObject offStateObject;

    private void Start()
    {
        if (hasOnOffState)
            UpdateVisualState();
    }

    public void SetState(bool state)
    {
        if (!hasOnOffState)
            return;

        isOn = state;
        UpdateVisualState();
    }

    public void ToggleState()
    {
        if (!hasOnOffState)
            return;

        SetState(!isOn);
    }

    private void UpdateVisualState()
    {
        if (onStateRenderer != null)
        {
            Color color = onStateRenderer.color;
            color.a = isOn ? 1f : 0f;
            onStateRenderer.color = color;
        }

        if (offStateObject != null)
            offStateObject.SetActive(!isOn);
    }
}