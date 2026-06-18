using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string GetPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }

    public static void Save(int slot, SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }

    public static SaveData Load(int slot)
    {
        string path = GetPath(slot);

        if (!File.Exists(path))
            return new SaveData(); // isEmpty = true par défaut

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool SaveExists(int slot)
    {
        return File.Exists(GetPath(slot));
    }

    public static void DeleteSave(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }
}