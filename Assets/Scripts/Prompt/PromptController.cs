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
            if (prompt == null || prompt.promptObject == null)
                continue;

            promptDictionary[prompt.promptType] = prompt.promptObject;
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

        if (!promptDictionary.TryGetValue(type, out GameObject prompt))
            return;

        if (prompt == null)
        {
            promptDictionary.Remove(type);
            promptRequests.Remove(type);
            CheckEmptyPrompts();
            return;
        }

        if (!promptRequests.ContainsKey(type))
            promptRequests[type] = new HashSet<Object>();

        promptRequests[type].Add(requester);

        UpdatePrompt(type);
    }

    // RELEASE PROMPT
    public void ReleasePrompt(PromptType type, Object requester)
    {
        if (requester == null || !promptRequests.ContainsKey(type))
            return;

        promptRequests[type].Remove(requester);

        UpdatePrompt(type);
    }

    // UPDATE PROMPT
    private void UpdatePrompt(PromptType type)
    {
        if (!promptDictionary.TryGetValue(type, out GameObject prompt))
        {
            promptRequests.Remove(type);
            CheckEmptyPrompts();
            return;
        }

        if (prompt == null)
        {
            promptDictionary.Remove(type);
            promptRequests.Remove(type);
            CheckEmptyPrompts();
            return;
        }

        // CLEAN DESTROYED REQUESTERS
        if (promptRequests.TryGetValue(type, out HashSet<Object> requesters))
        {
            requesters.RemoveWhere(requester => requester == null);
        }

        bool shouldShow =
            promptRequests.ContainsKey(type) &&
            promptRequests[type].Count > 0;

        prompt.SetActive(shouldShow);

        if (shouldShow)
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