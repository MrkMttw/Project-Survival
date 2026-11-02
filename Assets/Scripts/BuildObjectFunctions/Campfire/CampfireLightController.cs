using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CampfireLightController : MonoBehaviour
{
    private Light2D campfireLight;
    private CampfireController campfireController;
    private SpriteRenderer litVisual;
    private Animator campfireAnimator;

    [SerializeField]
    private GameObject unlitVisual;

    private void Awake()
    {
        campfireLight = GetComponent<Light2D>();
        litVisual = GetComponent<SpriteRenderer>();
        campfireAnimator = GetComponent<Animator>();

        campfireController =
            FindFirstObjectByType<CampfireController>();

        if (campfireLight == null)
            Debug.LogError("Campfire Light2D missing.");

        if (litVisual == null)
            Debug.LogError("Campfire SpriteRenderer missing.");

        if (campfireAnimator == null)
            Debug.LogError("Campfire Animator missing.");

        if (campfireController == null)
            Debug.LogError("CampfireController missing.");

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

        if (campfireAnimator != null)
            campfireAnimator.enabled = isLit;

        if (unlitVisual != null)
            unlitVisual.SetActive(!isLit);
    }
}