using System;
using System.IO;
using UnityEngine;

public static class SaveFileHandler
{
    // SAVE JSON DATA
    public static bool SaveToJson<T>(string filePath, T data)
    {
        string temporaryPath = filePath + ".tmp";

        try
        {
            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(temporaryPath, json);

            if (File.Exists(filePath))
            {
                // CREATE BACKUP BEFORE REPLACING SAVE
                bool backupCreated = SaveBackupHandler.CreateBackup(filePath);

                if (!backupCreated)
                {
                    Debug.LogError("Save cancelled because backup creation failed.");
                    return false;
                }

                File.Replace(temporaryPath, filePath, null);
            }
            else
            {
                File.Move(temporaryPath, filePath);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Save failed: " + exception.Message);

            return false;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Temporary file cleanup failed: " + exception.Message);
                }
            }
        }
    }

    // LOAD JSON DATA
    public static T LoadFromJson<T>(string filePath) where T : class
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("Save file not found: " + filePath);
                return null;
            }

            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("Save file is empty: " + filePath);
                return null;
            }

            T data = JsonUtility.FromJson<T>(json);

            if (data == null)
            {
                Debug.LogError("Failed to deserialize save file: " + filePath);
                return null;
            }

            return data;
        }
        catch (Exception exception)
        {
            Debug.LogError("Load failed: " + exception.Message);

            return null;
        }
    }

    // CHECK FILE
    public static bool SaveExists(string filePath)
    {
        return File.Exists(filePath);
    }

    // DELETE FILE
    public static bool DeleteSave(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return true;

            File.Delete(filePath);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Delete failed: " + exception.Message);

            return false;
        }
    }
}