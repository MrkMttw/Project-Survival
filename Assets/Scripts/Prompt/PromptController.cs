using System.Collections.Generic;
using UnityEngine;

public enum PromptType
{
    E,
    F,
    G,
    ShiftG,
    B,
    Tab,
    R
}

[System.Serializable]
public class PromptEntry
{
    public PromptType promptType;
    public GameObject promptObject;
}

public class PromptController : MonoBehaviour
{
    public static PromptController Instance;

    [Header("Prompt UI")]
    public GameObject promptContainer;

    [Header("Prompt Entries")]
    public PromptEntry[] prompts;

    private Dictionary<PromptType, GameObject> promptDictionary =
        new Dictionary<PromptType, GameObject>();

    private Dictionary<PromptType, HashSet<Object>> promptRequests =
        new Dictionary<PromptType, HashSet<Object>>();

    private void Awake()
    {
        // SINGLETON
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // REGISTER PROMPTS
        foreach (PromptEntry prompt in prompts)
        {
            if (prompt == null)
                continue;

            // ALLOW INVISIBLE PROMPTS
            promptDictionary[prompt.promptType] =
                prompt.promptObject;

            if (prompt.promptObject != null)
                prompt.promptObject.SetActive(false);
        }

        if (promptContainer != null)
            promptContainer.SetActive(false);
    }

    // REQUEST PROMPT
    public void RequestPrompt(PromptType type, Object requester)
    {
        if (requester == null)
            return;

        if (!promptDictionary.ContainsKey(type))
            return;

        if (!promptRequests.ContainsKey(type))
            promptRequests[type] = new HashSet<Object>();

        promptRequests[type].Add(requester);

        UpdatePrompt(type);
    }

    // RELEASE PROMPT
    public void ReleasePrompt(PromptType type, Object requester)
    {
        if (requester == null ||
            !promptRequests.ContainsKey(type))
            return;

        promptRequests[type].Remove(requester);

        UpdatePrompt(type);
    }

    // CHECK IF PROMPT IS ACTIVE
    public bool IsPromptActive(PromptType type)
    {
        if (!promptRequests.TryGetValue(
            type, out HashSet<Object> requesters))
            return false;

        requesters.RemoveWhere(requester => requester == null);

        return requesters.Count > 0;
    }

    // UPDATE PROMPT
    private void UpdatePrompt(PromptType type)
    {
        if (!promptDictionary.TryGetValue(
            type, out GameObject prompt))
        {
            promptRequests.Remove(type);
            CheckEmptyPrompts();
            return;
        }

        // CLEAN DESTROYED REQUESTERS
        if (promptRequests.TryGetValue(
            type, out HashSet<Object> requesters))
        {
            requesters.RemoveWhere(requester => requester == null);
        }

        bool shouldShow = IsPromptActive(type);

        // UPDATE VISUAL PROMPT IF AVAILABLE
        if (prompt != null)
            prompt.SetActive(shouldShow);

        // INVISIBLE PROMPTS DON'T OPEN THE PROMPT CONTAINER
        if (shouldShow && prompt != null)
        {
            if (promptContainer != null)
                promptContainer.SetActive(true);
        }
        else
        {
            CheckEmptyPrompts();
        }
    }

    // HIDE ALL PROMPTS
    public void HideAllPrompts()
    {
        promptRequests.Clear();

        foreach (GameObject prompt in promptDictionary.Values)
        {
            if (prompt != null)
                prompt.SetActive(false);
        }

        if (promptContainer != null)
            promptContainer.SetActive(false);
    }

    // CHECK ACTIVE PROMPTS
    private void CheckEmptyPrompts()
    {
        foreach (GameObject prompt in promptDictionary.Values)
        {
            if (prompt != null && prompt.activeSelf)
                return;
        }

        if (promptContainer != null)
            promptContainer.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}