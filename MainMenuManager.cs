using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;
    private SaveData saveData;

    public TextMeshProUGUI ui_Point;

    public RectTransform[] rt_panels;

    private GameSettings gameSettings;

    public SettingsSlot[] settingsSlots;

    private bool settingsLoaded = false;

    [Header("Equipment")]
    public Transform[] t_categoryDisplays;

    [Header("Equipment - Weapon")]
    public Transform t_weaponRow;
    private Transform[] t_weaponRows;
    public int[] weaponPrice;
    public int maxWeaponCount;
    public TextMeshProUGUI[] equippedWeaponNames;
    public Image[] equippedWeaponImages;

    [Header("Equipment - Skills")]
    public Transform t_skillRow;
    private Transform[] t_skillRows;
    public int[] skillPrice;
    public int maxSkillCount;
    public TextMeshProUGUI[] equippedSkillNames;
    public Image[] equippedSkillImages;

    [Header("Equipment - Building")]
    public Transform t_buildingRow;
    private Transform[] t_buildingRows;
    public int[] buildingPrice;

    
    private int c_name = 1;
    private int c_image = 2;
    private int c_purchaseBtn = 3;
    private int c_equipBtn = 4;

    public AudioMixer amg_main;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SaveLoad.Initiate();
        
        gameSettings = SaveLoad.LoadSettings();
        SetSettingsDisplay();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        t_weaponRows = new Transform[t_weaponRow.childCount];
        for (int i = 0; i < t_weaponRow.childCount; i++){ t_weaponRows[i] = t_weaponRow.GetChild(i); }
        t_skillRows = new Transform[t_skillRow.childCount];
        for (int i = 0; i < t_skillRow.childCount; i++) { t_skillRows[i] = t_skillRow.GetChild(i); }
        t_buildingRows = new Transform[t_buildingRow.childCount];
        for (int i = 0; i < t_buildingRow.childCount; i++) { t_buildingRows[i] = t_buildingRow.GetChild(i); }

        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    public void Setup()
    {
        saveData = SaveLoad.currentSaveData;
        //Helper.WriteLog(saveData.points + "");
        ui_Point.text = "<sprite=0>" + saveData.points;
        InitWeaponDisplay();
        InitSkillDisplay();
        InitBuildingDisplay();
    }

    public void ResetDispaly()
    {
        for (int i = 0; i < rt_panels.Length; i++)
        {
            rt_panels[i].gameObject.SetActive(false);
        }
    }

    public void ShowPanel(int i)
    {
        ResetDispaly();
        StartCoroutine(Enlarge(rt_panels[i], 5f, 0.05f, 1f));
    }

    public void ChangeSettingsValue()
    {
        if (!settingsLoaded) return;
        for (int i = 0; i < settingsSlots.Length; i++)
        {
            int p = settingsSlots[i].place;
            settingsSlots[i].text.text = p==0 ? ((int)(settingsSlots[i].slider.value)).ToString("D") : (p==1 ? (settingsSlots[i].slider.value / 10f).ToString("N1") : (settingsSlots[i].slider.value / 100f).ToString("N2"));
        }
        gameSettings.maxFrameRate = (int)settingsSlots[0].slider.value;
        gameSettings.mouseSens = (int)settingsSlots[1].slider.value;
        gameSettings.volume = (int)settingsSlots[2].slider.value;
        gameSettings.musicVolume = (int)settingsSlots[3].slider.value;
        gameSettings.soundEffectVolume = (int)settingsSlots[4].slider.value;

        amg_main.SetFloat("MasterVolume", VolumeToDB(gameSettings.volume));
        amg_main.SetFloat("MusicVolume", VolumeToDB(gameSettings.musicVolume));
        amg_main.SetFloat("SfxVolume", VolumeToDB(gameSettings.soundEffectVolume));

        Application.targetFrameRate = gameSettings.maxFrameRate;

        SaveLoad.SaveSettings(gameSettings);
    }

    private float VolumeToDB(int vol)
    {
        return vol == 0 ? -1000f : Mathf.Log10((float)vol / 100f) * 20f;
    }

    public void SetSettingsDisplay()
    {
        settingsSlots[0].slider.value = gameSettings.maxFrameRate;
        settingsSlots[1].slider.value = gameSettings.mouseSens;
        settingsSlots[2].slider.value = gameSettings.volume;
        settingsSlots[3].slider.value = gameSettings.musicVolume;
        settingsSlots[4].slider.value = gameSettings.soundEffectVolume;
        settingsLoaded = true;
        ChangeSettingsValue();

    }

    public void InitWeaponDisplay()
    {
        for (int i = 0; i < t_weaponRows.Length; i++)
        {
            if (saveData.weaponsBought.Contains(i))
            {
                t_weaponRows[i].GetChild(c_purchaseBtn).gameObject.SetActive(false);
                t_weaponRows[i].GetChild(c_equipBtn).gameObject.SetActive(true);
                
                t_weaponRows[i].GetChild(c_equipBtn).GetComponentInChildren<TextMeshProUGUI>().text = saveData.weaponsEquipped.Contains(i) ? "Unequip" : "Equip";
                t_weaponRows[i].GetChild(c_equipBtn).gameObject.SetActive(saveData.weaponsEquipped.Count < maxWeaponCount || saveData.weaponsEquipped.Contains(i));
            }
            else
            {
                t_weaponRows[i].GetChild(c_purchaseBtn).gameObject.SetActive(true);
                t_weaponRows[i].GetChild(c_purchaseBtn).GetComponentInChildren<TextMeshProUGUI>().text = "Buy: " + weaponPrice[i] + "<sprite=0>";
               t_weaponRows[i].GetChild(c_equipBtn).gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < maxWeaponCount; i++)
        {
            if (i < saveData.weaponsEquipped.Count)
            {
                equippedWeaponNames[i].text = t_weaponRows[saveData.weaponsEquipped[i]].GetChild(c_name).GetComponent<TextMeshProUGUI>().text;
                equippedWeaponImages[i].sprite = t_weaponRows[saveData.weaponsEquipped[i]].GetChild(c_image).GetComponent<Image>().sprite;
                equippedWeaponImages[i].rectTransform.sizeDelta = t_weaponRows[saveData.weaponsEquipped[i]].GetChild(c_image).GetComponent<Image>().rectTransform.sizeDelta / t_weaponRows[saveData.weaponsEquipped[i]].GetChild(c_image).GetComponent<Image>().rectTransform.sizeDelta.y * 60f;
                equippedWeaponImages[i].color = Color.white;
            }
            else
            {
                equippedWeaponNames[i].text = "";
                equippedWeaponImages[i].sprite = null;
                equippedWeaponImages[i].color = Color.clear;
            }
        }
    }

    public void PurchaseWeapon(int i)
    {
        if (saveData.weaponsBought.Contains(i)) return;
        SaveLoad.currentSaveData.points -= weaponPrice[i];
        SaveLoad.currentSaveData.weaponsBought.Add(i);
        SaveLoad.SaveCurrent();
        Setup();
        InitWeaponDisplay();
    }

    public void EquipWeapon(int i)
    {
        if (saveData.weaponsEquipped.Count < maxWeaponCount && !saveData.weaponsEquipped.Contains(i))
        {
            SaveLoad.currentSaveData.weaponsEquipped.Add(i);
        }
        else
        {
            SaveLoad.currentSaveData.weaponsEquipped.Remove(i);
        }
        SaveLoad.SaveCurrent();
        Setup();
        InitWeaponDisplay();
    }

    public void InitSkillDisplay()
    {
        for (int i = 0; i < t_skillRows.Length; i++)
        {
            if (saveData.skillsBought.Contains(i))
            {
                t_skillRows[i].GetChild(c_purchaseBtn).gameObject.SetActive(false);
                //t_skillRows[i].GetChild(c_equipBtn).gameObject.SetActive(true);
                t_skillRows[i].GetChild(c_equipBtn).gameObject.SetActive(!saveData.skillsEquipped.Contains(i));

                //t_skillRows[i].GetChild(c_equipBtn).GetComponentInChildren<TextMeshProUGUI>().text = saveData.skillsEquipped.Contains(i) ? "Unequip" : "Equip";
                t_skillRows[i].GetChild(c_equipBtn).GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
                //t_skillRows[i].GetChild(c_equipBtn).gameObject.SetActive(saveData.skillsEquipped.Count < maxSkillCount || saveData.skillsEquipped.Contains(i));
            }
            else
            {
                t_skillRows[i].GetChild(c_purchaseBtn).gameObject.SetActive(true);
                t_skillRows[i].GetChild(c_purchaseBtn).GetComponentInChildren<TextMeshProUGUI>().text = "Buy: " + skillPrice[i] + "<sprite=0>";
                t_skillRows[i].GetChild(c_equipBtn).gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < maxSkillCount; i++)
        {
            if (i < saveData.skillsEquipped.Count)
            {
                equippedSkillNames[i].text = t_skillRows[saveData.skillsEquipped[i]].GetChild(c_name).GetComponent<TextMeshProUGUI>().text;
                equippedSkillImages[i].sprite = t_skillRows[saveData.skillsEquipped[i]].GetChild(c_image).GetComponent<Image>().sprite;
                equippedSkillImages[i].color = Color.white;
            }
            else
            {
                equippedSkillNames[i].text = "";
                equippedSkillImages[i].sprite = null;
                equippedSkillImages[i].color = Color.clear;
            }
        }
    }

    public void PurchaseSkill(int i)
    {
        if (saveData.skillsBought.Contains(i)) return;
        SaveLoad.currentSaveData.points -= skillPrice[i];
        SaveLoad.currentSaveData.skillsBought.Add(i);
        SaveLoad.SaveCurrent();
        Setup();
        InitSkillDisplay();
    }

    public void EquipSkill(int i)
    {
        /*if (saveData.skillsEquipped.Count < maxSkillCount && !saveData.skillsEquipped.Contains(i))
        {
            SaveLoad.currentSaveData.skillsEquipped.Add(i);
        }
        else
        {
            SaveLoad.currentSaveData.skillsEquipped.Remove(i);
        }*/
        if (!saveData.skillsEquipped.Contains(i))
        {
            SaveLoad.currentSaveData.skillsEquipped = new List<int>{i};
        }
        SaveLoad.SaveCurrent();
        Setup();
        InitSkillDisplay();
    }

    public void InitBuildingDisplay()
    {
        for (int i = 0; i < t_buildingRows.Length; i++)
        {
            if (saveData.buildingsBought.Contains(i))
            {
                t_buildingRows[i].GetChild(c_purchaseBtn).gameObject.SetActive(false);
            }
            else
            {
                t_buildingRows[i].GetChild(c_purchaseBtn).gameObject.SetActive(true);
                t_buildingRows[i].GetChild(c_purchaseBtn).GetComponentInChildren<TextMeshProUGUI>().text = "Buy: " + buildingPrice[i] + "<sprite=0>";
            }
        }
    }

    public void PurchaseBuilding(int i)
    {
        if (saveData.buildingsBought.Contains(i)) return;
        SaveLoad.currentSaveData.points -= buildingPrice[i];
        SaveLoad.currentSaveData.buildingsBought.Add(i);
        SaveLoad.SaveCurrent();
        Setup();
        InitBuildingDisplay();
    }

    public void SetCategoryDisplay(int id)
    {
        for (int i = 0; i < t_categoryDisplays.Length; i++)
        {
            t_categoryDisplays[i].gameObject.SetActive(id == i);
        }
    }

    /*public void ShowMissionSelection()
    {
        rt_missionSelection.gameObject.SetActive(true);
        StartCoroutine(Enlarge(rt_missionSelection, 5f, 0.05f, 1f));
    }*/

    IEnumerator Enlarge(RectTransform rt, float speed, float minScale, float maxScale)
    {
        rt.gameObject.SetActive(true);
        float t = minScale;
        while (t < maxScale)
        {
            t = Mathf.Clamp(t + speed * Time.deltaTime, minScale, maxScale);
            rt.localScale = new Vector3(t, t, 1f);
            yield return null;
        }
    }

    public void Quit()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (!Application.isEditor)
        {
            Application.Quit();
        }
        else
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
        }
    }

    [Serializable]
    public class SettingsSlot
    { 
        public Slider slider;
        public TextMeshProUGUI text;
        public int place;
    }
}
