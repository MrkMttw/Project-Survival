using UnityEngine;

/// <summary>
/// Serializable data structure for storing game save information.
/// Contains player state and other persistent game data.
/// </summary>
[System.Serializable]
public class SaveData
{
    /// <summary>
    /// The player's position in world space when the game was saved.
    /// </summary>
    public Vector3 playerPosition;

    //public string mapBoundary;
}
