using UnityEngine;
using UnityEngine.UI;

public class DurabilityBehavior : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;

    private Item item;

    private void Awake()
    {
        item = GetComponentInParent<Item>();

        if (fillImage != null)
            fillImage.gameObject.SetActive(false);
    }

    public void UpdateBar()
    {
        if (item == null)
            item = GetComponentInParent<Item>();

        if (item == null || fillImage == null)
            return;

        bool shouldShow =
            item.hasDurability &&
            item.CurrentDurability < item.maxDurability &&
            item.CurrentDurability > 0;

        fillImage.gameObject.SetActive(shouldShow);

        if (shouldShow)
        {
            fillImage.fillAmount =
                item.GetDurabilityPercentage();
        }
    }

    public void HideBar()
    {
        if (fillImage != null)
            fillImage.gameObject.SetActive(false);
    }
}