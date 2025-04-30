using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PurchasePanel : MonoBehaviour
{

    public int itemCost, panelID;
    public bool purchased;
    public ItemsPurchaseManager itemsPurchaseManager;
    public WeaponSwitching weaponSwitching;
    public PlayerHealth playerHealth;
    public GameObject purchasePanel;
    public UIScript uiScript;

    public GameObject panel;
    public GameObject notEnoughCoins;
    public GameObject isFull;
    public GameObject cannotBuy;

    public bool notEnoughCoinsActive = false;


    public PostProcessVolume volume;
    private Vignette vignette;
    private float flashAmount = 0f;
    public float maxFlashAmount = 0.6f;
    public float flashFade = 1f;


    public void Start() {
        if (panelID == 2) {
            volume.profile.TryGetSettings(out vignette);
        }
    }

    public void Update() {
        if (vignette != null)
        {
            vignette.intensity.Override(Mathf.Clamp(flashAmount, 0f, maxFlashAmount));
        }
        flashAmount = Mathf.Clamp(flashAmount - Time.deltaTime * flashFade, 0f, maxFlashAmount);
    }

    public void PurchaseItem()
    {
        if (panelID == 1)
        {
            // buy ammo
            if (weaponSwitching.selectedWeapon != 2) {
                if (weaponSwitching.playerGunAttack.ammoCount < weaponSwitching.playerGunAttack.maxTotalAmmo)
                {
                    weaponSwitching.playerGunAttack.ammoCount += weaponSwitching.playerGunAttack.maxAmmo * 2;
                    if (weaponSwitching.playerGunAttack.ammoCount > weaponSwitching.playerGunAttack.maxTotalAmmo)
                    {
                        weaponSwitching.playerGunAttack.ammoCount = weaponSwitching.playerGunAttack.maxTotalAmmo;
                    }

                    itemsPurchaseManager.purchaseSound.Play();
                }
                else
                {
                    itemsPurchaseManager.gameCoinsManager.changeCoins(itemCost);
                }
            }
        }
        else if (panelID == 2)
        {
            // buy health

            if (playerHealth.currentHealth < playerHealth.maxHealth) {
                playerHealth.TakeDamage(-10);

                flashAmount = 0.4f;

                if (playerHealth.currentHealth > playerHealth.maxHealth)
                {
                    playerHealth.currentHealth = playerHealth.maxHealth;
                }

                itemsPurchaseManager.purchaseSound.Play();
            }
            else {
                itemsPurchaseManager.gameCoinsManager.changeCoins(itemCost);
            }
        }
        else if (panelID == 3)
        {
            // buy faster reload
            Purchased();
            itemsPurchaseManager.hasFasterReload = true;
        }
        else if (panelID == 4)
        {
            // buy faster movement
            Purchased();
            itemsPurchaseManager.hasFasterMovement = true;
        }
        else if (panelID == 5)
        {
            // buy larger mag size
            Purchased();
            itemsPurchaseManager.hasLargerMagSize = true;
        }
        else if (panelID == 6)
        {
            // buy increased max health
            Purchased();
            itemsPurchaseManager.hasIncreasedMaxHealth = true;
        }
        else if (panelID == 7)
        {
            // buy lower hp high damage
            Purchased();
            itemsPurchaseManager.hasLowerHpHigherDamage = true;
        }

        if (purchased) {
            purchasePanel.GetComponent<Animator>().SetBool("itemBought", true);
            itemsPurchaseManager.purchaseSound.Play();
            Invoke("InactivePanel", 2f);
        }
        
    }

    void InactivePanel() {
        purchasePanel.SetActive(false);
    }

    public void Purchased() {
        purchased = true;
        panel.SetActive(false);
        uiScript.perkBought[uiScript.perkCount] = panelID - 3;
        uiScript.perkCount++;
    }


}
