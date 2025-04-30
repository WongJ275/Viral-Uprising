using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using System.Linq;

public class UIScript : MonoBehaviour
{
    [Header("References")]
    public BaseController baseController;
    public WeaponSwitching weaponSwitching;
    public ItemsPurchaseManager itemsPurchaseManager;
    public PlayerSkillManager playerSkillManager;
    public StarterAssetsInputs playerInput;
    public BuildingSelector buildingSelector;

    [Header("Ammo UI")]
    public TextMeshProUGUI currentAmmo;
    public TextMeshProUGUI totalAmmo;

    [Header("Gun UI")]
    public RawImage rifle;
    public RawImage shotgun;
    public RawImage cube;
    public RawImage pistol;
    public RawImage rifle2, shotgun2, pistol2;
    private RawImage weapon1, weapon2;
    private Vector3 weapon1Scale, weapon2Scale, weapon1SelectedScale, weapon2SelectedScale;

    [Header("Health UI")]
    public PlayerHealth playerHealth;
    public Image healthBar;
    public Image shieldBar;
    public TextMeshProUGUI healthText;
    public RawImage shieldIcon;

    [Header("Perk UI")]
    public GameObject perk;
    public int[] perkBought = new int[5];
    public int perkCount = 0;
    public int maxShieldHealth;

    [Header("Station Health UI")]
    public Image stationHealthBar;
    public float hideBannerTime;
    public Image banner;

    [Header("Skill UI")]
    public Image skillCD;
    public Image skill;

    [Header("Crosshair UI")]
    public Image center;
    public Image top;
    public Image bottom;
    public Image left;
    public Image right;
    public float expandSpeed;
    public float resetPosition;
    public Image hitMarker;
    public float shootExpand;

    [Header("Coins UI")]
    public TextMeshProUGUI coinsText;

    [Header("Enemy UI")]
    public TextMeshProUGUI enemyCount;
    public TextMeshProUGUI waveCount;
    public TextMeshProUGUI enemyLine;
    public TextMeshProUGUI completedWave;
    public TextMeshProUGUI waveLine;
    public Image waveBar;
    public SpawningManager spawningManager;
    private int totalEnemiesInWave = -1;
    public TextMeshProUGUI waveTime;
    public Image waveBanner;
    private bool notFirstWave = false;

    [Header("SkillTime UI")]
    public TextMeshProUGUI skillTime;
    public Image skillTimeBar;
    public float skillTimeRemaining;

    [Header("Building UI")]
    public Image panel;
    public TextMeshProUGUI buildingCost;
    public TextMeshProUGUI buildingName;
    public GameObject buildingStorer;
    public Dictionary<int, string> buildingNameDict = new Dictionary<int, string> {
        {0, "Barrier"},
        {1, "Gun Turret"},
        {2, "Cryo Discharger"},
        {3, "Mine Factory"},
        {4, "Spike Trap"},
        {5, "EMP Trap"},
        {6, "Shield"},
        {7, "Tesla Turret"}
    };
    public Dictionary<int, int> buildingCostDict = new Dictionary<int, int> {
        {0, 10},
        {1, 30},
        {2, 10},
        {3, 30},
        {4, 20},
        {5, 20},
        {6, 20},
        {7, 40}
    };
    private bool buildingPanelActive = false;

    



    void Awake() {
        for (int i = 0; i < perkBought.Length; i++)
        {
            perk.transform.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < 4; i++) {
            hitMarker.transform.GetChild(i).gameObject.SetActive(false);
            skill.transform.GetChild(i).gameObject.SetActive(false);
        }
        shieldIcon.gameObject.SetActive(false);
        skillTimeRemaining = playerSkillManager.skillDuration;
        panel.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateAmmo();
        InitiateGun();
        InitiateSkill();
        Invoke("HideBanner", hideBannerTime);
        Invoke("InitialWaveTime", 5f);

    }

    // Update is called once per frame
    void Update()
    {
        UpdateAmmo();
        UpdateGun();
        UpdateHealth();
        UpdatePerks();
        UpdateStationHealth();
        UpdateSkill();
        UpdateCrosshair();
        UpdateCoins();
        UpdateWave();
        UpdateWaveTime();
        UpdateSkillTime();
        UpdateBuilding();
    }

    public void UpdateAmmo()
    {
        if (weaponSwitching.selectedWeapon == 2)
        {
            currentAmmo.text = "∞";
            totalAmmo.text = "∞";

            currentAmmo.transform.localPosition = new Vector3(22, 7, 0);
            return;
        }
        else if (weaponSwitching.armsAnimationType.animationType == 1)
        {
            currentAmmo.text = (weaponSwitching.playerGunAttack.currentAmmo / 8 ).ToString();
            totalAmmo.text = (weaponSwitching.playerGunAttack.ammoCount / 8 ).ToString();
        }
        else {
            currentAmmo.text = weaponSwitching.playerGunAttack.currentAmmo.ToString();
            totalAmmo.text = weaponSwitching.playerGunAttack.ammoCount.ToString();
        }

        if (weaponSwitching.playerGunAttack.currentAmmo >= 10 && weaponSwitching.armsAnimationType.animationType != 1)
        {
            currentAmmo.transform.localPosition = new Vector3(-5, 5, 0);
        }
        else
        {
            currentAmmo.transform.localPosition = new Vector3(22, 5, 0);
        }

        if (playerSkillManager.usingInfiniteBulletSkill)
        {
            totalAmmo.text = "∞";
        }
    }

    public void InitiateGun() {
        List<int> gunList = SaveLoad.currentSaveData.weaponsEquipped;
        
        switch (gunList.Min()) {
            case 0: case 2:
                rifle.gameObject.SetActive(true);
                weapon1 = rifle;
                weapon1Scale = new Vector3(0.15f, 0.12f, 0);
                weapon1SelectedScale = new Vector3(0.185f, 0.155f, 0);
                break;
            case 3: case 4:
                shotgun.gameObject.SetActive(true);
                weapon1 = shotgun;
                weapon1Scale = new Vector3(0.14f, 0.14f, 0);
                weapon1SelectedScale = new Vector3(0.165f, 0.165f, 0);
                break;
            case 1: case 5:
                pistol.gameObject.SetActive(true);
                weapon1 = pistol;
                weapon1Scale = new Vector3(0.17f, 0.17f, 0);
                weapon1SelectedScale = new Vector3(0.19f, 0.19f, 0);
                break;
        }

        switch (gunList.Max()) {
            case 0: case 2:
                rifle2.gameObject.SetActive(true);
                weapon2 = rifle2;
                weapon2Scale = new Vector3(0.15f, 0.12f, 0);
                weapon2SelectedScale = new Vector3(0.185f, 0.155f, 0);
                break;
            case 3: case 4:
                shotgun2.gameObject.SetActive(true);
                weapon2 = shotgun2;
                weapon2Scale = new Vector3(0.14f, 0.14f, 0);
                weapon2SelectedScale = new Vector3(0.165f, 0.165f, 0);
                break;
            case 1: case 5:
                pistol2.gameObject.SetActive(true);
                weapon2 = pistol2;
                weapon2Scale = new Vector3(0.17f, 0.17f, 0);
                weapon2SelectedScale = new Vector3(0.19f, 0.19f, 0);
                break;
        }
    }

    public void UpdateGun()
    {
        weapon1.transform.localScale = Vector3.Lerp(weapon1.transform.localScale, weapon1Scale, Time.deltaTime * 5);
        weapon2.transform.localScale = Vector3.Lerp(weapon2.transform.localScale, weapon2Scale, Time.deltaTime * 5);
        cube.transform.localScale = Vector3.Lerp(cube.transform.localScale, new Vector3(0.12f, 0.12f, 0), Time.deltaTime * 5);

        Color changeColor = new Color(0.75f, 0.75f, 0.75f, 255);
        weapon1.color = changeColor;
        weapon2.color = changeColor;
        cube.color = changeColor;

        if (weaponSwitching.selectedWeapon == 0) {
            weapon1.transform.localScale = Vector3.Lerp(weapon1.transform.localScale, weapon1SelectedScale, Time.deltaTime * 5);
            weapon1.color = Color.white;
        }
        else if (weaponSwitching.selectedWeapon == 1) {
            weapon2.transform.localScale = Vector3.Lerp(weapon2.transform.localScale, weapon2SelectedScale, Time.deltaTime * 5);
            weapon2.color = Color.white;
        }
        else if (weaponSwitching.selectedWeapon == 2) {
            cube.transform.localScale = Vector3.Lerp(cube.transform.localScale, new Vector3(0.155f, 0.155f, 0), Time.deltaTime * 5);
            cube.color = Color.white;
        }
    }

    public void UpdateHealth()
    {
        healthBar.fillAmount = (float) playerHealth.currentHealth / playerHealth.maxHealth;
        healthText.text = (playerHealth.currentHealth + playerHealth.shieldHealth).ToString();
        shieldBar.fillAmount = (float)playerHealth.shieldHealth / maxShieldHealth;

        int totalHealth = playerHealth.currentHealth + playerHealth.shieldHealth;

        if (totalHealth <= 99 && totalHealth >= 10)
        {
            healthText.transform.localPosition = new Vector3(276, 58, 0);
        }
        else if (totalHealth < 10)
        {
            healthText.transform.localPosition = new Vector3(300, 58, 0);
        }
        else
        {
            healthText.transform.localPosition = new Vector3(255, 58, 0);
        }

        if (playerHealth.shieldHealth > 0)
        {
            shieldIcon.gameObject.SetActive(true);
            if (totalHealth <= 99 && totalHealth >= 10)
            {
                shieldIcon.transform.localPosition = new Vector3(202, 38, 0);
            }
            else if (totalHealth < 10)
            {
                shieldIcon.transform.localPosition = new Vector3(213, 38, 0);
            }
            else
            {
                shieldIcon.transform.localPosition = new Vector3(195, 38, 0);
            }
        }
        else
        {
            shieldIcon.gameObject.SetActive(false);
        }
    
    }

    public void UpdatePerks()
    {
        //Vector3 spawnPosition = new Vector3(-180, 46, 0);
        Vector3 spawnPosition = new Vector3(-150, 10, 0);

        for (int i = 0; i < perkBought.Length; i++)
        {
            spawnPosition.x = -150;
            if (perkBought[i] != 9)
            {
                perk.transform.GetChild(perkBought[i]).gameObject.SetActive(true);
                spawnPosition.x += i * 60;
                perk.transform.GetChild(perkBought[i]).transform.localPosition = spawnPosition;
                perkBought[i] = 9;
            }
        }

    }

    public void UpdateStationHealth()
    {
        stationHealthBar.fillAmount = (float) baseController.currentHealth / baseController.maxHealth;
    }


    public void UpdateSkill() {
        skillCD.fillAmount = (float) playerSkillManager.skillCDRemaining / playerSkillManager.skillCD;
    }

    public void InitiateSkill() {
        skill.transform.GetChild(playerSkillManager.chosenSkill).gameObject.SetActive(true);
    }

    public void UpdateCrosshair() {
        if (playerInput.move != Vector2.zero) {
            if (playerInput.sprint) {
                ChangeCrosshair(5f + shootExpand);
            }
            else {
                ChangeCrosshair(3f + shootExpand);
            }
        }
        else if (shootExpand > 0) {
            ChangeCrosshair(shootExpand);
        }
        else {
            ResetCrosshair();
        }
    }

    public void CallUpdateCrosshair() {
        shootExpand = 5f;
        Invoke("ResetShootExpand", 0.2f);
    }

    public void ResetShootExpand() {
        shootExpand = 0;
    }

    public void ChangeCrosshair(float expandSize) {
        top.rectTransform.localPosition = new Vector2 (0, Mathf.Lerp(top.rectTransform.localPosition.y, center.rectTransform.localPosition.y + resetPosition + expandSize, Time.deltaTime * expandSpeed));
        bottom.rectTransform.localPosition = new Vector2 (0, Mathf.Lerp(bottom.rectTransform.localPosition.y, center.rectTransform.localPosition.y - resetPosition - expandSize, Time.deltaTime * expandSpeed));
        left.rectTransform.localPosition = new Vector2 (Mathf.Lerp(left.rectTransform.localPosition.x, center.rectTransform.localPosition.x - resetPosition - expandSize, Time.deltaTime * expandSpeed), 0);
        right.rectTransform.localPosition = new Vector2 (Mathf.Lerp(right.rectTransform.localPosition.x, center.rectTransform.localPosition.x + resetPosition + expandSize, Time.deltaTime * expandSpeed), 0);
    }

    public void ResetCrosshair() {
        top.rectTransform.localPosition = new Vector2 (0, resetPosition);
        bottom.rectTransform.localPosition = new Vector2 (0, -resetPosition);
        left.rectTransform.localPosition = new Vector2 (-resetPosition, 0);
        right.rectTransform.localPosition = new Vector2 (resetPosition, 0);
    }

    public void CrosshairHit() {
        for (int i = 0; i < 4; i++) {
            hitMarker.transform.GetChild(i).gameObject.SetActive(true);
        }
        Invoke("DisableHitMarker", 0.2f);
    }

    public void CrosshairKill() {
        for (int i = 0; i < 4; i++) {
            hitMarker.transform.GetChild(i).GetComponent<Image>().color = Color.red;
        }
        Invoke("ResetHitMarkerColor", 0.2f);
    }

    public void DisableHitMarker() {
        for (int i = 0; i < 4; i++) {
            hitMarker.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void ResetHitMarkerColor() {
        for (int i = 0; i < 4; i++) {
            hitMarker.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }
    }

    public void UpdateCoins() {
        coinsText.text = itemsPurchaseManager.gameCoinsManager.totalCoins.ToString();
        if (itemsPurchaseManager.gameCoinsManager.totalCoins < 10)
        {
            coinsText.transform.localPosition = new Vector3(125, -7, 0);
        }
        else {
            coinsText.transform.localPosition = new Vector3(109, -7, 0);
        }
    }

    public void HideBanner() {
        banner.GetComponent<Animator>().SetBool("hide", true);
        Invoke("DisableBanner", 2f);
    }

    public void DisableBanner() {
        banner.gameObject.SetActive(false);
    }

    public void UpdateWave() {
        if (totalEnemiesInWave == -1 && spawningManager.currentWave != -1 && spawningManager.currentWave < spawningManager.waveInfos.Count) {
            totalEnemiesInWave = spawningManager.waveInfos[spawningManager.currentWave].spawnInfos.Count;
        }
        enemyCount.text = SpawningManager.remainingEnemies.ToString();
        waveCount.text = (spawningManager.currentWave + 1).ToString();
        waveBar.fillAmount = (float) SpawningManager.remainingEnemies / totalEnemiesInWave;

        if (SpawningManager.remainingEnemies == 0) {
            totalEnemiesInWave = -1;
        }

        if (spawningManager.preparing || GameManager.gameWon) {
            if (spawningManager.currentWave == 0) {
                completedWave.gameObject.SetActive(false);
                waveLine.gameObject.SetActive(false);
            }
            else {
                completedWave.gameObject.SetActive(true);
                waveCount.text = (spawningManager.currentWave).ToString();
            }
            enemyLine.gameObject.SetActive(false);
        }
        else {
            waveLine.gameObject.SetActive(true);
            completedWave.gameObject.SetActive(false);
            enemyLine.gameObject.SetActive(true);
            waveCount.text = (spawningManager.currentWave + 1).ToString();
        }
    }

    public void UpdateWaveTime() {
        float remainingTime = -spawningManager.timeToNextWave;
        if (remainingTime < 0) {
            waveTime.text = "00:00";
            HideWaveTime();
        }
        else {
            int minutes = (int) remainingTime / 60;
            int seconds = (int) remainingTime % 60;
            waveTime.text = minutes.ToString("00") + ":" + seconds.ToString("00");
            if (notFirstWave) {
                ShowWaveTime();
            }
        }
    }

    public void InitialWaveTime() {
        notFirstWave = true;
    }

    public void ShowWaveTime() {
        waveBanner.gameObject.SetActive(true);
    }

    public void HideWaveTime() {
        waveBanner.GetComponent<Animator>().SetBool("hideWaveBanner", true);
        Invoke("DisableWaveBanner", 2f);
    }

    public void DisableWaveBanner() {
        waveBanner.gameObject.SetActive(false);
    }

    public void UpdateSkillTime() {
        if (!playerSkillManager.usingSkill) {
            skillTimeRemaining = playerSkillManager.skillDuration;
            skillTimeBar.gameObject.SetActive(false);
            return;
        }
        else {
            skillTimeBar.gameObject.SetActive(true);
            skillTimeRemaining -= Time.deltaTime;
            int seconds = (int) skillTimeRemaining % 60;
            int milliseconds = (int) (skillTimeRemaining * 100) % 100;
            skillTime.text = seconds.ToString("00") + ":" + milliseconds.ToString("00");
        }

    }

    public void UpdateBuilding() {
        if (weaponSwitching.selectedWeapon == 2 && !buildingPanelActive) {
            panel.gameObject.SetActive(true);
            buildingPanelActive = true;
        }
        else if (weaponSwitching.selectedWeapon != 2 && buildingPanelActive) {
            panel.GetComponent<Animator>().SetBool("hidePanel", true);
            Invoke("DisablePanel", 1f);
            buildingPanelActive = false;
        }

        if (buildingPanelActive) {
            buildingCost.text = buildingCostDict[buildingSelector.selectedBuilding].ToString();
            buildingName.text = buildingNameDict[buildingSelector.selectedBuilding];

            buildingStorer.transform.GetChild(buildingSelector.selectedBuilding).gameObject.SetActive(true);
            if (buildingSelector.previousSelectedBuilding != -1) {
                buildingStorer.transform.GetChild(buildingSelector.previousSelectedBuilding).gameObject.SetActive(false);
            }
        }

        switch (buildingSelector.selectedBuilding) {
            case 0: case 6: 
                buildingName.transform.localPosition = new Vector3(15, -8, 0);
                break;
            case 2:
                buildingName.transform.localPosition = new Vector3(11, -8, 0);
                break;
            case 3: 
                buildingName.transform.localPosition = new Vector3(11.8f, -8, 0);
                break;
            case 4: case 5: case 7: case 1:
                buildingName.transform.localPosition = new Vector3(13.1f, -8, 0);
                break;
        }
    }

    public void DisablePanel() {
        panel.gameObject.SetActive(false);
    }

}
