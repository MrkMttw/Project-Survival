using UnityEngine;
using TMPro;

public class CoordinateDisplay : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text coordinateText;

    void Update()
    {
        int x = Mathf.RoundToInt(player.position.x);
        int y = Mathf.RoundToInt(player.position.y);

        coordinateText.text = $"X: {x}  Y: {y}";
    }
}