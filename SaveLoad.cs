using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SaveLoad : MonoBehaviour
{
    public static string currentSaveSlot = "1";
    public static SaveData currentSaveData;
    private static string dataPath;

    public static bool initiated = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (!Application.isEditor)
        {
            dataPath = Application.persistentDataPath;
        }
        else
        {
            dataPath = Application.dataPath;
        }
        if (!initiated)
        {
            SetCurrentSaveSlot(currentSaveSlot);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void Initiate()
    {
        if (PlayerPrefs.HasKey("currentSaveSlot"))
        {
            SetCurrentSaveSlot(PlayerPrefs.GetString("currentSaveSlot"));
        }
        SetCurrentSaveSlot(currentSaveSlot);
        initiated = true;
    }

    public static void SetCurrentSaveSlot(string slot)
    {
        PlayerPrefs.SetString("currentSaveSlot", slot);
        currentSaveSlot = slot;
        currentSaveData = Load(slot);
    }

    public static void SaveCurrent()
    {
        Save(currentSaveData, currentSaveSlot);
    }

    public static void Save(SaveData s, String saveSlot)
    {
        string json = JsonUtility.ToJson(s);
        Debug.Log(json);
        File.WriteAllText(dataPath + "/save_" + saveSlot + ".txt", json);
    }

    public static SaveData Load(String saveSlot)
    {
        if (!File.Exists(dataPath + "/save_" + saveSlot + ".txt"))
        {
            Save(new SaveData(), saveSlot);
            return Load(saveSlot);
        }
        string json = File.ReadAllText(dataPath + "/save_" + saveSlot + ".txt");
        Debug.Log(json);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void SaveSettings(GameSettings gs)
    {
        string json = JsonUtility.ToJson(gs);
        Debug.Log(json);
        File.WriteAllText(dataPath + "/settings.txt", json);
    }

    public static GameSettings LoadSettings()
    {
        if (!File.Exists(dataPath + "/settings.txt"))
        {
            SaveSettings(new GameSettings());
            return LoadSettings();
        }
        string json = File.ReadAllText(dataPath + "/settings.txt");
        Debug.Log(json);
        return JsonUtility.FromJson<GameSettings>(json);
    }
}

[Serializable]
public class SaveData
{
    public int points;
    public List<int> levelsPassed;
    public List<int> weaponsBought;
    public List<int> weaponsEquipped;
    public List<int> skillsBought;
    public List<int> skillsEquipped;
    public List<int> buildingsBought;

    public SaveData(int _points, List<int> _levelsPassed, List<int> _weaponsBought, List<int> _weaponsEquipped, List<int> _skillsBought, List<int> _skillEquipped, List<int> _buildingsBought)
    {
        this.points = _points;
        this.levelsPassed = _levelsPassed.Select(x => x).ToList();
        this.weaponsBought = _weaponsBought.Select(x => x).ToList();
        this.weaponsEquipped = _weaponsEquipped.Select(x => x).ToList();
        this.skillsBought = _skillsBought.Select(x => x).ToList();
        this.skillsEquipped = _skillEquipped.Select(x => x).ToList();
        this.buildingsBought = _buildingsBought.Select(x => x).ToList();
    }

    public SaveData()
    {
        this.points = 0;
        this.levelsPassed = new List<int>();
        this.weaponsBought = new List<int>() { 0, 1 };
        this.weaponsEquipped = new List<int>() { 0, 1 };
        this.skillsBought = new List<int>() { 0 };
        this.skillsEquipped = new List<int>() { 0 };
        this.buildingsBought = new List<int>() { 0, 1, 2 };
    }
}

[Serializable]
public class GameSettings
{
    public int maxFrameRate = 120;
    public int mouseSens = 40;
    public int volume = 100;
    public int musicVolume = 100;
    public int soundEffectVolume = 100;
}
