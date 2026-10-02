using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CampfireLightController : MonoBehaviour
{
    private Light2D campfireLight;
    private CampfireController campfireController;
    private SpriteRenderer litVisual;

    [SerializeField]
    private GameObject unlitVisual;

    private void Awake()
    {
        campfireLight = GetComponent<Light2D>();
        litVisual = GetComponent<SpriteRenderer>();

        campfireController =
            FindFirstObjectByType<CampfireController>();

        if (campfireLight == null)
        {
            Debug.LogError(
                "CampfireLightController could not find Light2D."
            );
        }

        if (litVisual == null)
        {
            Debug.LogError(
                "CampfireLightController could not find SpriteRenderer."
            );
        }

        if (campfireController == null)
        {
            Debug.LogError(
                "CampfireLightController could not find CampfireController."
            );
        }

        if (campfireController != null)
        {
            SetLight(campfireController.currentWood > 0);
        }
    }

    private void Update()
    {
        if (campfireController == null)
            return;

        SetLight(campfireController.currentWood > 0);
    }

    public void SetLight(bool isLit)
    {
        if (campfireLight != null)
            campfireLight.enabled = isLit;

        if (litVisual != null)
            litVisual.enabled = isLit;

        if (unlitVisual != null)
            unlitVisual.SetActive(!isLit);
    }
}