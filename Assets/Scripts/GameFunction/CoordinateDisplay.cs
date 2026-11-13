using UnityEngine;
using TMPro;

public class CoordinateDisplay : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("PC UI")]
    [SerializeField] private TMP_Text coordinateText;

    [Header("Mobile UI")]
    [SerializeField] private TMP_Text mobileCoordinateText;

    void Update()
    {
        int x = Mathf.RoundToInt(player.position.x);
        int y = Mathf.RoundToInt(player.position.y);

        string coordinates = $"X: {x}  Y: {y}";

        // PC UI
        if (coordinateText != null)
            coordinateText.text = coordinates;

        // MOBILE UI
        if (mobileCoordinateText != null)
            mobileCoordinateText.text = coordinates;
    }
}