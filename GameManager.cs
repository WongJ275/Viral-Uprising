using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using static MainMenuManager;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static int shotFired = 0;
    public static int shotHit = 0;
    public static int headshotHit = 0;
    public static int killed = 0;
    public static int healthLoss = 0;
    public static int deathCount = 0;
    public static bool gameWon = false;

    [Header("EndGame")]
    public GameObject go_endCanvas;
    public Transform t_endTitle;
    public Transform t_items;

    public GameObject go_winEffect;
    public GameObject go_loseEffect;
    public GameObject go_Camera;

    public GameObject[] go_disable;

    public GameObject pf_audio;

    private bool ended = false;

    public AudioMixer amg_main;

    [Header("settings")]
    public GameObject go_settingsCanvas;
    private GameSettings gameSettings;

    public SettingsSlot[] settingsSlots;

    private bool settingsLoaded = false;
    public bool showingSettingsMenu = false;
    public GameObject[] go_settingsPanels;


    void Awake()
    {
        instance = this;
        Cursor.visible = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameSettings = SaveLoad.LoadSettings();
        SetSettingsDisplay();
    }

    private float VolumeToDB(int vol)
    {
        return vol==0 ? -1000f : Mathf.Log10((float)vol / 100f) * 20f;
    }

    public void Win()
    {
        if (ended) return;
        ended = true;
        Debug.Log("You win!");
        t_endTitle.GetChild(0).gameObject.SetActive(true);
        t_endTitle.GetChild(1).gameObject.SetActive(false);
        go_winEffect.SetActive(true);
        gameWon = true;
        Summary();
    }

    public void Lose()
    {
        if (ended) return;
        ended = true;
        Debug.Log("You lose!");
        t_endTitle.GetChild(0).gameObject.SetActive(false);
        t_endTitle.GetChild(1).gameObject.SetActive(true);
        go_loseEffect.SetActive(true);
        Summary();
    }

    [Header("points")]
    public RankInfo wavePoint;
    public RankInfo timePoint;
    public RankInfo baseHealthPoint;
    public RankInfo weaponAccuracyPoint;
    public RankInfo headshotPercentagePoint;
    public RankInfo playerHealthLossPoint;
    public RankInfo deathPoint;
    public RankInfo enemyKilledPoint;

    public RankInfo totalPoint;

    private void Summary()
    {
        CloseSettingsMenu();
        for (int i = 0; i < go_disable.Length; i++)
        {
            go_disable[i].SetActive(false);
        }
        go_Camera.SetActive(true);
        Camera.main.transform.root.gameObject.GetComponent<AudioListener>().enabled = false;
        GameObject.FindWithTag("Player").SetActive(false);
        StartCoroutine(EndGameEffect());
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        /*GameObject.FindWithTag("Player").GetComponent<FirstPersonController>().enabled = false;
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().enabled = false;
        GameObject.FindWithTag("Player").GetComponent<StarterAssetsInputs>().cursorLocked = false;
        GameObject.FindWithTag("Player").GetComponent<StarterAssetsInputs>().cursorInputForLook = false;
        GameObject.FindWithTag("Player").GetComponentInChildren<GunSway>().enabled = false;*/
        GameObject.Find("PlayerSkillManager").GetComponent<PlayerSkillManager>().enabled = false;
        /*foreach (var v in GameObject.FindWithTag("Player").GetComponentsInChildren<PlayerGunAttack>())
        {
            v.enabled = false;
        }*/
        //Time.timeScale = 0;
        go_endCanvas.SetActive(true);

        float waveCleared = SpawningManager.instance.currentWave;
        float waveValue = waveCleared / SpawningManager.instance.waveInfos.Count;
        int wavePointGained = (int)wavePoint.ac_point.Evaluate(waveValue);
        Debug.Log("Wave Cleared: " + waveCleared + "/" + SpawningManager.instance.waveInfos.Count + ComputeRank(wavePoint, waveValue) + wavePointGained);
        t_items.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = waveCleared + "/" + SpawningManager.instance.waveInfos.Count;
        t_items.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(wavePoint, waveValue);

        float time = Time.timeSinceLevelLoad;
        float minTime = SpawningManager.instance.waveInfos.Sum(x => (x.prepareTime + x.spawnInfos.Aggregate((y, z) => y = z).spawnTime));
        float timeValue = time - minTime;
        int timePointGained = (waveCleared >= SpawningManager.instance.waveInfos.Count) ? (int)timePoint.ac_point.Evaluate(timeValue) : 0;
        Debug.Log("Time: " + time + ((waveCleared >= SpawningManager.instance.waveInfos.Count) ? ComputeRank(timePoint, timeValue) + timePointGained : ""));
        t_items.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = ((int)(time / 60) > 0 ? (int)(time / 60) + "m" : "") + (int)(time % 60) + "s";
        t_items.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = ((waveCleared >= SpawningManager.instance.waveInfos.Count) ? ComputeRank(timePoint, timeValue) : "F");

        float baseRemainingHealthValue = BaseController.instance.currentHealth / BaseController.instance.maxHealth;
        int basePointGained = (int)baseHealthPoint.ac_point.Evaluate(baseRemainingHealthValue);
        Debug.Log("Base Remaining Health: " + baseRemainingHealthValue + ComputeRank(baseHealthPoint, baseRemainingHealthValue) + basePointGained);
        t_items.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = Math.Clamp((int)(baseRemainingHealthValue * 100f), 0, 100) + "%";
        t_items.GetChild(2).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(baseHealthPoint, baseRemainingHealthValue);

        float weaponAccuracyValue = (float)shotHit / (float)(shotFired == 0 ? 1 : shotFired);
        int weaponAccuracyPointGained = (int)weaponAccuracyPoint.ac_point.Evaluate(weaponAccuracyValue);
        Debug.Log("Weapon Accuracy: " + weaponAccuracyValue + ComputeRank(weaponAccuracyPoint, weaponAccuracyValue) + weaponAccuracyPointGained);
        t_items.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = Math.Clamp((int)(weaponAccuracyValue * 100f), 0, 100) + "%";
        t_items.GetChild(3).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(weaponAccuracyPoint, weaponAccuracyValue);

        float totalEnemy = SpawningManager.instance.waveInfos.Sum(x => x.spawnInfos.Count);
        float enemyKilledValue = killed / totalEnemy;
        int enemyKilledPointGained = (int)enemyKilledPoint.ac_point.Evaluate(enemyKilledValue);
        Debug.Log("Enemy Killed: " + killed + ComputeRank(enemyKilledPoint, enemyKilledValue) + enemyKilledPointGained);
        t_items.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text = killed + "/" + totalEnemy;
        t_items.GetChild(4).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(enemyKilledPoint, enemyKilledValue);

        float headshotPercentageValue = headshotHit / (shotHit == 0f ? 1f : shotHit);
        int headshotPercentagePointGained = (int)headshotPercentagePoint.ac_point.Evaluate(headshotPercentageValue);
        Debug.Log("Headshot Percentage: " + headshotPercentageValue + ComputeRank(headshotPercentagePoint, headshotPercentageValue) + headshotPercentagePointGained);
        t_items.GetChild(5).GetChild(1).GetComponent<TextMeshProUGUI>().text = Math.Clamp((int)(headshotPercentageValue * 100f), 0, 100) + "%";
        t_items.GetChild(5).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(headshotPercentagePoint, headshotPercentageValue);

        float playerHealthLossValue = healthLoss;
        int playerHealthLossPointGained = (int)playerHealthLossPoint.ac_point.Evaluate(playerHealthLossValue);
        Debug.Log("Player Health Loss: " + healthLoss + ComputeRank(playerHealthLossPoint, playerHealthLossValue) + playerHealthLossPointGained);
        t_items.GetChild(6).GetChild(1).GetComponent<TextMeshProUGUI>().text = "" + healthLoss;
        t_items.GetChild(6).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(playerHealthLossPoint, playerHealthLossValue);

        /*int deathValue = deathCount;
        int deathPointGained = (int)deathPoint.ac_point.Evaluate(deathValue);
        Debug.Log("Death: " + deathValue + ComputeRank(deathPoint, deathValue) + deathPointGained);
        t_items.GetChild(7).GetChild(1).GetComponent<TextMeshProUGUI>().text = "" + deathCount;
        t_items.GetChild(7).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(deathPoint, deathValue);*/

        int totalScore = wavePointGained + timePointGained + basePointGained + weaponAccuracyPointGained + enemyKilledPointGained + headshotPercentagePointGained + playerHealthLossPointGained /*+ deathPointGained*/;
        Debug.Log("Total Score: " + totalScore + ComputeRank(totalPoint, totalScore));
        SaveLoad.currentSaveData.points += totalScore;
        t_items.GetChild(9).GetChild(1).GetComponent<TextMeshProUGUI>().text = "" + totalScore;
        t_items.GetChild(9).GetChild(2).GetComponent<TextMeshProUGUI>().text = ComputeRank(totalPoint, totalScore);

        int currentLevel = PlayerPrefs.HasKey("missionID") ? PlayerPrefs.GetInt("missionID") : 1;
        if (SaveLoad.currentSaveData.levelsPassed.IndexOf(currentLevel) == -1) { SaveLoad.currentSaveData.levelsPassed.Add(currentLevel); }
        
        SaveLoad.SaveCurrent();
    }

    private IEnumerator EndGameEffect()
    {
        RectTransform rt_block = go_endCanvas.transform.GetChild(0).GetComponent<RectTransform>();
        float t = 1;
        float speed = 1.5f;
        float delay = 3f;
        float y = 1500;
        yield return new WaitForSeconds(delay);
        while (t > 0)
        {
            t -= Time.deltaTime * speed;
            rt_block.anchoredPosition = new Vector2(0, y * t);
            yield return null;
        }
        rt_block.anchoredPosition = new Vector2(0, 0);
    }

    private List<string> Ranks = new List<string> { "F", "E", "D", "C", "B", "A", "S" };

    private string ComputeRank(RankInfo rankInfo, float value)
    {
        if (rankInfo.isAscending)
        {
            for (int i = 0; i < rankInfo.value.Count; i++)
            {
                if (value <= rankInfo.value[i])
                {
                    return Ranks[i];
                }
            }
        }
        else
        {
            for (int i = rankInfo.value.Count - 1; i >= 0; i--)
            {
                if (value >= rankInfo.value[i])
                {
                    return Ranks[i];
                }
            }
        }
        return Ranks.Last();
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

    public void ChangeSettingsValue()
    {
        if (!settingsLoaded) return;
        for (int i = 0; i < settingsSlots.Length; i++)
        {
            int p = settingsSlots[i].place;
            settingsSlots[i].text.text = p == 0 ? ((int)(settingsSlots[i].slider.value)).ToString("D") : (p == 1 ? (settingsSlots[i].slider.value / 10f).ToString("N1") : (settingsSlots[i].slider.value / 100f).ToString("N2"));
        }
        gameSettings.maxFrameRate = (int)settingsSlots[0].slider.value;
        gameSettings.mouseSens = (int)settingsSlots[1].slider.value;
        gameSettings.volume = (int)settingsSlots[2].slider.value;
        gameSettings.musicVolume = (int)settingsSlots[3].slider.value;
        gameSettings.soundEffectVolume = (int)settingsSlots[4].slider.value;

        GameObject.FindWithTag("Player").GetComponent<FirstPersonController>().RotationSpeed = gameSettings.mouseSens / 10f / Mathf.PI;
        amg_main.SetFloat("MasterVolume", VolumeToDB(gameSettings.volume));
        amg_main.SetFloat("MusicVolume", VolumeToDB(gameSettings.musicVolume));
        amg_main.SetFloat("SfxVolume", VolumeToDB(gameSettings.soundEffectVolume));
        Application.targetFrameRate = gameSettings.maxFrameRate;

        SaveLoad.SaveSettings(gameSettings);
    }

    public void ShowSettingsMenuPanel(int id)
    {
        for (int i = 0; i < go_settingsPanels.Length; i++)
        {
            go_settingsPanels[i].SetActive(i == id);
        }
    }

    public void CloseSettingsMenu()
    {
        showingSettingsMenu = false;
        go_settingsCanvas.SetActive(showingSettingsMenu);
        Cursor.lockState = showingSettingsMenu || ended ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = showingSettingsMenu;
        //Time.timeScale = 1f;
    }

    private KeyCode key_mainMenu = KeyCode.P;
    private float t_mainMenu = 5f;
    private float counterMainMenu = 0f;

    private KeyCode key_restart = KeyCode.O;
    private float t_restart = 5f;
    private float counterRestart = 0f;

    void Update()
    {
        if (Input.GetKey(key_mainMenu))
        {
            counterMainMenu += Time.deltaTime;
            if (counterMainMenu > t_mainMenu)
            {
                Cursor.lockState = CursorLockMode.None;
                SceneManager.LoadScene(0);
            }
        }
        else { counterMainMenu = 0f; }

        if (Input.GetKey(key_restart))
        {
            counterRestart += Time.deltaTime;
            if (counterRestart > t_restart)
            {
                SceneManager.LoadScene(1);
            }
        }
        else { counterRestart = 0f; }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            showingSettingsMenu = !(showingSettingsMenu || ended);
            go_settingsCanvas.SetActive(showingSettingsMenu);
            Cursor.lockState = showingSettingsMenu ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = showingSettingsMenu;
            //Time.timeScale = showingSettingsMenu ? 0f : 1f;
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Replay()
    {
        SceneManager.LoadSceneAsync(1);
        Cursor.lockState = CursorLockMode.Locked;
    }
}

[Serializable]
public class RankInfo
{    
    public AnimationCurve ac_point;
    public List<float> value;
    public bool isAscending;
}
