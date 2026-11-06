using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelectionController : MonoBehaviour
{
    [Header("UI References")]
    public Transform content;
    public GameObject worldEntryPrefab;
    public Button continueButton;
    public Button deleteButton;

    [Header("Delete Confirmation UI")]
    public GameObject deleteConfirmationPanel;

    private WorldSaveData selectedWorld;
    private WorldEntryUI selectedEntry;

    private void OnEnable()
    {
        RefreshWorldList();
    }

    private void Start()
    {
        continueButton.interactable = false;
        deleteButton.interactable = false;

        deleteConfirmationPanel.SetActive(false);
    }

    public void RefreshWorldList()
    {
        // CLEAR EXISTING ENTRIES
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        selectedWorld = null;
        selectedEntry = null;

        continueButton.interactable = false;
        deleteButton.interactable = false;

        // GET REGISTERED WORLDS
        List<WorldSaveData> worlds = WorldSaveManager.GetAllWorlds();

        // CREATE WORLD ENTRIES
        foreach (WorldSaveData world in worlds)
        {
            GameObject entryObject = Instantiate(worldEntryPrefab, content);

            WorldEntryUI entryUI = entryObject.GetComponent<WorldEntryUI>();

            entryUI.Setup(world, SelectWorld);
        }
    }

    private void SelectWorld(WorldSaveData world)
    {
        // DESELECT PREVIOUS ENTRY
        if (selectedEntry != null)
        {
            selectedEntry.SetSelected(false);
        }

        // UPDATE SELECTED WORLD
        selectedWorld = world;
        selectedEntry = GetSelectedEntry(world);

        // HIGHLIGHT NEW ENTRY
        if (selectedEntry != null)
        {
            selectedEntry.SetSelected(true);
        }

        continueButton.interactable = selectedWorld != null;
        deleteButton.interactable = selectedWorld != null;
    }

    private WorldEntryUI GetSelectedEntry(WorldSaveData world)
    {
        foreach (Transform child in content)
        {
            WorldEntryUI entry = child.GetComponent<WorldEntryUI>();

            if (entry != null && entry.GetWorldData() == world)
            {
                return entry;
            }
        }

        return null;
    }

    // OPEN DELETE CONFIRMATION
    public void OnClickDelete()
    {
        if (selectedWorld == null)
        {
            Debug.LogWarning("NO WORLD SELECTED!");
            return;
        }

        deleteConfirmationPanel.SetActive(true);
    }

    // CONFIRM WORLD DELETION
    public void OnConfirmDelete()
    {
        if (selectedWorld == null)
            return;

        string worldID = selectedWorld.worldID;

        if (WorldSaveManager.DeleteWorld(worldID))
        {
            Debug.Log("WORLD DELETED SUCCESSFULLY!");

            deleteConfirmationPanel.SetActive(false);

            RefreshWorldList();
        }
        else
        {
            Debug.LogError("WORLD DELETION FAILED!");
        }
    }

    // CANCEL WORLD DELETION
    public void OnCancelDelete()
    {
        deleteConfirmationPanel.SetActive(false);
    }

    // CONTINUE WORLD
    public void OnClickContinue()
    {
        Debug.Log("CONTINUE BUTTON CLICKED!");

        if (selectedWorld == null)
        {
            Debug.LogWarning("NO WORLD SELECTED!");
            return;
        }

        // STORE SELECTED WORLD
        SelectedWorldData.world = selectedWorld;

        Debug.Log("SELECTED WORLD: " + selectedWorld.worldName);
        Debug.Log("WORLD SEED: " + selectedWorld.worldSeed);
        Debug.Log("WORLD HANDOFF SUCCESSFUL!");

        // LOAD GAME SCENE
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}