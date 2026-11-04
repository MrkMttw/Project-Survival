using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GlobalLightingController : MonoBehaviour
{
    public static GlobalLightingController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Distance Settings")]
    [Min(0f)]
    [SerializeField] private float activationDistance = 25f;

    [Min(0f)]
    [SerializeField] private float deactivationDistance = 30f;

    [Header("Performance")]
    [Min(0.01f)]
    [SerializeField] private float checkInterval = 0.25f;

    [Header("Independent Light Prefab IDs")]
    [Tooltip("IDs of independent light prefabs allowed to register.")]
    [SerializeField] private string[] independentPrefabIDs;

    private readonly HashSet<WorldLightRegistration> registeredLights =
        new HashSet<WorldLightRegistration>();

    private float checkTimer;
    private float activationDistanceSqr;
    private float deactivationDistanceSqr;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "GlobalLightingController: Another instance already exists."
            );

            enabled = false;
            return;
        }

        Instance = this;
        UpdateDistanceThresholds();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;
        UpdateRegisteredLights();
    }

    // DISTANCE THRESHOLDS

    private void UpdateDistanceThresholds()
    {
        activationDistanceSqr =
            activationDistance * activationDistance;

        deactivationDistanceSqr =
            deactivationDistance * deactivationDistance;
    }

    // REGISTRATION

    public void RegisterLight(WorldLightRegistration registration)
    {
        if (registration == null)
            return;

        if (registration.IsIndependentPrefab &&
            !IsIndependentPrefabAllowed(registration))
        {
            Debug.LogWarning(
                $"LIGHT REJECTED: {registration.name}. " +
                $"ID: {registration.IndependentPrefabID}",
                registration
            );

            registration.SetWithinRange(false);
            return;
        }

        registeredLights.Add(registration);

        UpdateLightDistance(registration);
    }

    public void UnregisterLight(WorldLightRegistration registration)
    {
        if (registration == null)
            return;

        registeredLights.Remove(registration);
    }

    // CHECK INDEPENDENT PREFAB ID

    private bool IsIndependentPrefabAllowed(
        WorldLightRegistration registration)
    {
        string prefabID = registration.IndependentPrefabID;

        if (string.IsNullOrWhiteSpace(prefabID) ||
            independentPrefabIDs == null)
        {
            return false;
        }

        foreach (string allowedID in independentPrefabIDs)
        {
            if (prefabID == allowedID)
                return true;
        }

        return false;
    }

    // DISTANCE CHECKING

    private void UpdateRegisteredLights()
    {
        if (player == null)
            return;

        foreach (WorldLightRegistration registration in registeredLights)
        {
            if (registration == null)
                continue;

            UpdateLightDistance(registration);
        }
    }

    private void UpdateLightDistance(
        WorldLightRegistration registration)
    {
        if (player == null || registration == null)
        {
            if (registration != null)
                registration.SetWithinRange(false);

            return;
        }

        Vector2 difference =
            (Vector2)registration.transform.position -
            (Vector2)player.position;

        float distanceSqr = difference.sqrMagnitude;

        bool currentlyActive = registration.IsWithinRange;

        if (currentlyActive)
        {
            if (distanceSqr >= deactivationDistanceSqr)
                registration.SetWithinRange(false);
        }
        else
        {
            if (distanceSqr <= activationDistanceSqr)
                registration.SetWithinRange(true);
        }
    }

    // REFRESH

    public void RefreshAllLights()
    {
        UpdateDistanceThresholds();

        foreach (WorldLightRegistration registration in registeredLights)
        {
            if (registration != null)
                UpdateLightDistance(registration);
        }
    }

    // PLAYER REFERENCE

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        RefreshAllLights();
    }
}