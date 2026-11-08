using UnityEngine;
using System;

public class CampfireController : MonoBehaviour
{
    public event Action<bool> FuelStateChanged;

    [Header("Healing")]
    [Range(0f, 100f)]
    public float baseHealPercentPerSecond = 2f;

    [HideInInspector]
    public float healPercentPerSecond = 2f;

    [Header("Campfire Level")]
    public int progressionWood = 0;

    [SerializeField]
    private int campfireLevel = 0;

    [Header("Wood")]
    public int currentWood = 0;
    public int maxWood = 100;
    public GameObject woodItemPrefab;

    public float fuelDecayDelay = 10f;
    public float fuelDecayInterval = 5f;
    public float fuelDecayAmount = 1f;

    private float fuelDecayTimer = 0f;
    private float fuelDecayDelayTimer = 0f;

    private HealthController playerHealth;
    private bool playerInRange = false;

    private CampfireUI campfireUI;
    private bool hasFuel;

    // PERSISTENCE
    private BuildingObject buildingObject;
    private WorldGenerator worldGenerator;
    private BuildingSaveData savedData;

    public int CampfireLevel
    {
        get { return campfireLevel; }
    }

    private void Awake()
    {
        UpdateHealingRate();
        hasFuel = currentWood > 0;
    }

    private void Start()
    {
        Debug.Log("CAMPFIRE START RAN: " + name, this);
    }

    private void Update()
    {
        UpdateFuel();

        // KEEP THE IN-MEMORY SAVE DATA UPDATED
        SaveCampfireStateToMemory();

        if (!playerInRange || playerHealth == null)
            return;

        if (currentWood <= 0)
            return;

        float healAmount =
            playerHealth.maxHealth *
            (healPercentPerSecond / 100f) *
            Time.deltaTime;

        playerHealth.Heal(healAmount);
    }

    // PERSISTENCE

    public void ConnectToBuildingSaveData(BuildingObject building)
    {
        if (building == null)
        {
            Debug.LogError("CAMPFIRE CONNECTION FAILED: BuildingObject is NULL!");
            return;
        }

        buildingObject = building;

        worldGenerator = FindFirstObjectByType<WorldGenerator>();

        if (worldGenerator == null)
        {
            Debug.LogError("CAMPFIRE CONNECTION FAILED: WorldGenerator not found!");
            return;
        }

        if (!worldGenerator.TryGetBuildingSaveData(
            buildingObject,
            out savedData))
        {
            Debug.LogError(
                "CAMPFIRE CONNECTION FAILED: Building record not found! ID: " +
                buildingObject.buildingID
            );

            return;
        }

        Debug.Log(
            "CAMPFIRE CONNECTION SUCCESSFUL | ID: " +
            buildingObject.buildingID,
            this
        );

        if (savedData.hasCampfireState)
        {
            currentWood = savedData.campfireCurrentWood;
            campfireLevel = savedData.campfireLevel;
            fuelDecayTimer = savedData.campfireFuelDecayTimer;
            fuelDecayDelayTimer = savedData.campfireFuelDecayDelayTimer;

            UpdateHealingRate();

            hasFuel = currentWood > 0;

            FuelStateChanged?.Invoke(hasFuel);

            Debug.Log("CAMPFIRE STATE RESTORED!");
        }

        SaveCampfireStateToMemory();
        UpdateCampfireUI();
    }

    private void SaveCampfireStateToMemory()
    {
        if (savedData == null)
            return;

        savedData.hasCampfireState = true;
        savedData.campfireCurrentWood = currentWood;
        savedData.campfireLevel = campfireLevel;
        savedData.campfireFuelDecayTimer = fuelDecayTimer;
        savedData.campfireFuelDecayDelayTimer = fuelDecayDelayTimer;
    }

    // HEALING

    private void UpdateHealingRate()
    {
        healPercentPerSecond =
            baseHealPercentPerSecond + (campfireLevel * 0.2f);
    }

    // FUEL

    private void UpdateFuel()
    {
        if (currentWood <= 0)
        {
            currentWood = 0;
            fuelDecayTimer = 0f;

            NotifyFuelStateChanged();

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

            NotifyFuelStateChanged();
            UpdateCampfireUI();
        }
    }

    private void NotifyFuelStateChanged()
    {
        bool newFuelState = currentWood > 0;

        if (hasFuel == newFuelState)
            return;

        hasFuel = newFuelState;

        FuelStateChanged?.Invoke(hasFuel);
    }

    // ADD WOOD

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
                    playerHealth.UpdateMaxHealth(campfireLevel);
            }
        }

        fuelDecayDelayTimer = 0f;
        fuelDecayTimer = 0f;

        NotifyFuelStateChanged();
        UpdateCampfireUI();

        SaveCampfireStateToMemory();
    }

    // WOOD VALIDATION

    public bool CanFeedWood(Item item)
    {
        if (!playerInRange)
            return false;

        if (item == null || woodItemPrefab == null)
            return false;

        Item woodItem = woodItemPrefab.GetComponent<Item>();

        if (woodItem == null)
            return false;

        return item.ID == woodItem.ID;
    }

    // UI

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

    // PLAYER DETECTION

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