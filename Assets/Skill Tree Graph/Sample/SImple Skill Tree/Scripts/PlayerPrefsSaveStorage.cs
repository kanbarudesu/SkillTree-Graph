using SkillTreeGraph.Core;
using UnityEngine;

public class PlayerPrefsSaveStorage : ISkillTreeSaveStorage
{
    private readonly string _saveKey;

    public PlayerPrefsSaveStorage(string saveKey = "SkillTree_SaveData")
    {
        _saveKey = saveKey;
    }

    public void Save(SkillTreeSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(_saveKey, json);
        PlayerPrefs.Save();
    }

    public SkillTreeSaveData Load()
    {
        if (!PlayerPrefs.HasKey(_saveKey)) return null;

        string json = PlayerPrefs.GetString(_saveKey);
        return JsonUtility.FromJson<SkillTreeSaveData>(json);
    }
}