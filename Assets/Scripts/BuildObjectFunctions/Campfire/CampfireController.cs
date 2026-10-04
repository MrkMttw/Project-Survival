using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("Healing")]
    [Range(0f, 100f)]
    public float baseHealPercentPerSecond = 2f;

    [HideInInspector]
    public float healPercentPerSecond = 2f;

    [Header("Campfire Level")]
    public int progressionWood = 0;

    [Header("Campfire Level")]
    [SerializeField]
    private int campfireLevel = 0;

    [Header("Wood")]
    public int currentWood = 0;
    public int maxWood = 100;
    public int woodItemID;

    public float fuelDecayDelay = 10f;
    public float fuelDecayInterval = 5f;
    public float fuelDecayAmount = 1f;

    private float fuelDecayTimer = 0f;
    private float fuelDecayDelayTimer = 0f;

    private HealthController playerHealth;
    private bool playerInRange = false;

    private CampfireUI campfireUI;

    public int CampfireLevel
    {
        get { return campfireLevel; }
    }

    private void UpdateHealingRate()
    {
        healPercentPerSecond =
            baseHealPercentPerSecond + (campfireLevel * 0.2f);
    }

    private void Awake()
    {
        UpdateHealingRate();
    }

    private void Update()
    {
        UpdateFuel();

        UpdateCampfireUI();

        if (!playerInRange || playerHealth == null)
            return;

        if (currentWood <= 0)
            return;

        // Heal based on the player's MAX HP
        float healAmount =
            playerHealth.maxHealth *
            (healPercentPerSecond / 100f) *
            Time.deltaTime;

        playerHealth.Heal(healAmount);
    }

    private void UpdateFuel()
    {
        if (currentWood <= 0)
        {
            currentWood = 0;
            fuelDecayTimer = 0f;

            return;
        }

        if (fuelDecayDelayTimer < fuelDecayDelay)
        {
            fuelDecayDelayTimer += Time.deltaTime;
            return;
        }

        fuelDecayTimer += Time.deltaTime;

        if (fuelDecayTimer >= fuelDecayInterval)
        {
            currentWood -= (int)fuelDecayAmount;
            currentWood = Mathf.Max(0, currentWood);

            fuelDecayTimer = 0f;

            UpdateCampfireUI();
        }
    }

    public void AddWood(int amount)
    {
        if (amount <= 0)
            return;

        currentWood =
            Mathf.Min(
                currentWood + amount,
                maxWood
            );

        if (campfireLevel < 5)
        {
            int requiredWood = GetNextLevelRequirement();

            if (currentWood >= requiredWood)
            {
                currentWood -= requiredWood;
                currentWood += 2;

                campfireLevel++;

                UpdateHealingRate();

                CraftingController craftingController =
                    FindFirstObjectByType<CraftingController>();

                if (craftingController != null)
                    craftingController.RefreshRecipes();

                if (playerHealth != null)
                {
                    playerHealth.UpdateMaxHealth(campfireLevel);
                }
            }
        }

        fuelDecayDelayTimer = 0f;
        fuelDecayTimer = 0f;
        
        UpdateCampfireUI();
    }

    public bool CanFeedWood(Item item)
    {
        if (!playerInRange)
            return false;

        if (item == null)
            return false;

        return item.ID == woodItemID;
    }

    public void RegisterCampfireUI(CampfireUI ui)
    {
        campfireUI = ui;

        UpdateCampfireUI();
    }

    private void UpdateCampfireUI()
    {
        if (campfireUI == null)
            return;

        int requiredWood = GetNextLevelRequirement();

        int displayedWood =
            Mathf.Min(
                currentWood,
                requiredWood
            );

        campfireUI.UpdateUI(
            campfireLevel,
            displayedWood,
            requiredWood
        );
    }

    private int GetNextLevelRequirement()
    {
        switch (campfireLevel)
        {
            case 0:
                return 10;

            case 1:
                return 25;

            case 2:
                return 40;

            case 3:
                return 55;

            case 4:
                return 70;

            case 5:
                return 70;

            default:
                return 10;
        }
    }

    public void PlayerEntered(HealthController health)
    {
        if (health == null)
            return;

        playerHealth = health;
        playerInRange = true;

        Debug.Log("Player entered campfire healing range.");
    }

    public void PlayerExited(HealthController health)
    {
        if (health == null)
            return;

        if (playerHealth == health)
        {
            playerHealth = null;
            playerInRange = false;

            Debug.Log("Player left campfire healing range.");
        }
    }
}