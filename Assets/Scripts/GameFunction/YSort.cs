/* 
*TO MAKE THIS WORK PROPERLY*

 GO TO YOUR SPRITE SHEET
 KLYC SLICE
 SELECT THE PIVOT TO BOTTOM
 (DO THAT TO ALL YOUR ASSET IMAGE)
*/


using UnityEngine;

public class YSort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}