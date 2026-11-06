using System;
using System.IO;
using UnityEngine;

public static class SaveBackupHandler
{
    private const int MAX_BACKUPS = 2;

    // CREATE BACKUP
    public static bool CreateBackup(string filePath)
    {
        string temporaryPath = null;

        try
        {
            if (!File.Exists(filePath))
                return false;

            string backupDirectory = Path.Combine(
                Path.GetDirectoryName(filePath),
                "Backups"
            );

            Directory.CreateDirectory(backupDirectory);

            string fileName = Path.GetFileName(filePath);

            string backup1 = Path.Combine(
                backupDirectory, fileName + ".backup1"
            );

            string backup2 = Path.Combine(
                backupDirectory, fileName + ".backup2"
            );

            temporaryPath = Path.Combine(
                backupDirectory, fileName + ".backup.tmp"
            );

            // COPY CURRENT SAVE TO TEMPORARY BACKUP
            File.Copy(filePath, temporaryPath, true);

            // REMOVE OLDEST BACKUP
            if (File.Exists(backup2))
            {
                File.Delete(backup2);
            }

            // MOVE BACKUP 1 TO BACKUP 2
            if (File.Exists(backup1))
            {
                File.Move(backup1, backup2);
            }

            // PROMOTE TEMPORARY BACKUP TO BACKUP 1
            File.Move(temporaryPath, backup1);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Backup creation failed: " + exception.Message);

            return false;
        }
        finally
        {
            if (temporaryPath != null && File.Exists(temporaryPath))
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Backup cleanup failed: " + exception.Message);
                }
            }
        }
    }

    // RESTORE LATEST VALID BACKUP
    public static bool RestoreLatestBackup<T>(
        string filePath,
        Func<T, bool> validator
    ) where T : class
    {
        string restorePath = null;

        try
        {
            string backupDirectory = Path.Combine(
                Path.GetDirectoryName(filePath),
                "Backups"
            );

            string fileName = Path.GetFileName(filePath);

            string backup1 = Path.Combine(
                backupDirectory, fileName + ".backup1"
            );

            string backup2 = Path.Combine(
                backupDirectory, fileName + ".backup2"
            );

            string[] backups = { backup1, backup2 };

            string validBackup = null;

            // FIND LATEST VALID BACKUP
            foreach (string backup in backups)
            {
                if (!File.Exists(backup))
                    continue;

                string json = File.ReadAllText(backup);

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                T data = JsonUtility.FromJson<T>(json);

                if (data != null && validator != null && validator(data))
                {
                    validBackup = backup;
                    break;
                }
            }

            if (validBackup == null)
            {
                Debug.LogError("No valid backup found.");
                return false;
            }

            // RESTORE THROUGH TEMPORARY FILE
            string directory = Path.GetDirectoryName(filePath);

            Directory.CreateDirectory(directory);

            restorePath = filePath + ".restore.tmp";

            File.Copy(validBackup, restorePath, true);

            if (File.Exists(filePath))
            {
                File.Replace(restorePath, filePath, null);
            }
            else
            {
                File.Move(restorePath, filePath);
            }

            Debug.Log("Backup restored successfully.");

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Backup restoration failed: " + exception.Message);

            return false;
        }
        finally
        {
            if (restorePath != null && File.Exists(restorePath))
            {
                try
                {
                    File.Delete(restorePath);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Restore cleanup failed: " + exception.Message);
                }
            }
        }
    }
}