using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class ItemsPurchaseManager : MonoBehaviour
{

    public Camera playerCam;
    public RaycastHit hit;
    public GameCoinsManager gameCoinsManager;
    public PurchasePanel purchasePanel;
    private PurchasePanel previousPanel;
    public FirstPersonController firstPersonController;
    public AudioSource purchaseSound;
    
    public bool hasFasterReload, hasLargerMagSize, hasLowerHpHigherDamage, hasFasterMovement, hasIncreasedMaxHealth;
    

    // Start is called before the first frame update
    void Start()
    {
        gameCoinsManager = transform.GetChild(0).GetComponent<GameCoinsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        ActivatePerk();
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit))
        {
            if (hit.collider.tag == "Purchase" && hit.distance < 15f)
            {
                if (purchasePanel != null) {
                    previousPanel = purchasePanel;
                }

                purchasePanel = hit.collider.GetComponent<PurchasePanel>();

                if (previousPanel != null && previousPanel != purchasePanel) {
                    previousPanel.panel.SetActive(false);
                }

                if (!purchasePanel.purchased) {
                    purchasePanel.panel.SetActive(true);
                }

                ShowMessage();

                
                if (Input.GetKeyDown(KeyCode.F) && !purchasePanel.purchased && !GameManager.instance.showingSettingsMenu) 
                {
                    PurchaseItemManager();
                }
            }
            else {
                if (purchasePanel != null) {
                    purchasePanel.panel.SetActive(false);
                }
            }
        }

        
    }


    public void PurchaseItemManager()
    {
        if (gameCoinsManager.totalCoins >= purchasePanel.itemCost)
        {
            gameCoinsManager.changeCoins(-purchasePanel.itemCost);
            purchasePanel.PurchaseItem();
        }
    }


    public void ActivatePerk()
    {
        if (hasFasterMovement)
        {
            hasFasterMovement = false;
            firstPersonController.MoveSpeed = 8;
            firstPersonController.SprintSpeed = 11;
        }
    }


    public void ShowMessage() {

        if (purchasePanel.weaponSwitching.selectedWeapon == 2 && purchasePanel.panelID == 1) {
            purchasePanel.isFull.SetActive(false);
            purchasePanel.notEnoughCoins.SetActive(false);
            purchasePanel.cannotBuy.SetActive(true);
        }
        else {
            if (purchasePanel.panelID == 1) {
                purchasePanel.cannotBuy.SetActive(false);
            }
            ShowMessage2();
        }
    }

    public void ShowMessage2() {
        if (gameCoinsManager.totalCoins < purchasePanel.itemCost) {
            purchasePanel.notEnoughCoins.SetActive(true);
            purchasePanel.notEnoughCoinsActive = true;
        }
        else {
            purchasePanel.notEnoughCoins.SetActive(false);
            purchasePanel.notEnoughCoinsActive = false;
        }

        if (!purchasePanel.notEnoughCoinsActive && purchasePanel.panelID == 2 && purchasePanel.playerHealth.currentHealth == purchasePanel.playerHealth.maxHealth) {
            purchasePanel.isFull.SetActive(true);
        }
        else if (purchasePanel.panelID == 2) {
            purchasePanel.isFull.SetActive(false);
        }

        if (!purchasePanel.notEnoughCoinsActive && purchasePanel.panelID == 1 && purchasePanel.weaponSwitching.playerGunAttack.ammoCount == purchasePanel.weaponSwitching.playerGunAttack.maxTotalAmmo) {
            purchasePanel.isFull.SetActive(true);
        }
        else if (purchasePanel.panelID == 1) {
            purchasePanel.isFull.SetActive(false);
        }

        
    }
}
