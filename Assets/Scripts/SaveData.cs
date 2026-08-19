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
    /// This is used to restore the player's location upon loading the game.
    /// </summary>
    public Vector3 playerPosition;
    /// <summary>
    /// The boundary of the map when the game was saved.
    /// This is used to restore the map boundary upon loading the game.
    /// </summary>
    public string mapBoundary;
}
