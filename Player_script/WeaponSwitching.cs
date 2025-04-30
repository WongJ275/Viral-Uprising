using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{

    public int selectedWeapon = 0;

    public ArmsAnimationType armsAnimationType;
    public PlayerGunAttack playerGunAttack;
    public GameObject arms;
    public int animationNum;
    public AudioSource pullShotgun;
    public AudioSource pullPistol;
    public AudioSource pullCube;
    public AudioSource reloadSound;
    public AudioSource hitSound;

    private GameObject[] go_buildables;

    void Awake() {
        if (!SaveLoad.initiated)
        {
            SaveLoad.Initiate();
        }
        List<int> weaponsEquipped = SaveLoad.currentSaveData.weaponsEquipped;
        int minID = weaponsEquipped.Min();

        transform.GetChild(minID).gameObject.SetActive(true);
        armsAnimationType = transform.GetChild(minID).GetComponent<ArmsAnimationType>();
        playerGunAttack = transform.GetChild(minID).GetComponent<PlayerGunAttack>();

        InitialSelectWeapon();

        for (int i = transform.childCount - 2; i >= 0; i--)
        {
            if (!weaponsEquipped.Contains(i))
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    void Start()
    {
        go_buildables = GameObject.FindGameObjectsWithTag("Buildable");
    }

    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        if (playerGunAttack.isBursting == false && !arms.GetComponent<Animator>().GetBool("isPullingGun0") && !arms.GetComponent<Animator>().GetBool("isPullingGun1") && !arms.GetComponent<Animator>().GetBool("isPullingCube") && !arms.GetComponent<Animator>().GetBool("isPullingGun2") ){
            if (Input.GetAxis("Mouse ScrollWheel") > 0f && !GameManager.instance.showingSettingsMenu) {
                selectedWeapon = (selectedWeapon + 1) % transform.childCount;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f && !GameManager.instance.showingSettingsMenu) {
                selectedWeapon = (selectedWeapon - 1 + transform.childCount) % transform.childCount;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) && !GameManager.instance.showingSettingsMenu) {
                selectedWeapon = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && transform.childCount >= 2 && !GameManager.instance.showingSettingsMenu) {
                selectedWeapon = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && transform.childCount >= 3 && !GameManager.instance.showingSettingsMenu) {
                selectedWeapon = 2;
            }
            
            if (previousSelectedWeapon != selectedWeapon) {
                if (playerGunAttack.transform.GetChild(0).childCount != 0) {
                    for (int i = playerGunAttack.transform.GetChild(0).childCount - 1; i >= 0; i--) {
                        Destroy(playerGunAttack.transform.GetChild(0).GetChild(i).gameObject);
                    }
                }

                if (selectedWeapon != 2) {
                    playerGunAttack.isReloading = false;
                    CancelAnimation();
                    playerGunAttack = transform.GetChild(selectedWeapon).GetComponent<PlayerGunAttack>();
                    arms.GetComponent<Animator>().SetBool("isHoldingCube", false);
                }
                else if (selectedWeapon == 2) {
                    playerGunAttack.isReloading = false;
                    CancelAnimation();
                    if (previousSelectedWeapon == 0) {
                        playerGunAttack = transform.GetChild(1).GetComponent<PlayerGunAttack>();
                    }
                    else if (previousSelectedWeapon == 1) {
                        playerGunAttack = transform.GetChild(0).GetComponent<PlayerGunAttack>();
                    }
                }
                arms.GetComponent<Animator>().SetBool("isHoldingPistol", false);

                SelectWeapon();
            }

            if (previousSelectedWeapon == 2 && selectedWeapon != 2) {
                BuildingPlacer buildingPlacer = transform.GetChild(2).GetComponent<BuildingPlacer>();
                buildingPlacer.DestroyBuildingPreview();
            }
        }
    }

    void SelectWeapon() {

        armsAnimationType = transform.GetChild(selectedWeapon).GetComponent<ArmsAnimationType>();
        animationNum = armsAnimationType.animationType;


        
        if (animationNum == 0) {
            arms.GetComponent<Animator>().SetBool("isPullingGun0", true);
            pullShotgun.PlayDelayed(0.3f);
        }
        else if (animationNum == 1) {
            arms.GetComponent<Animator>().SetBool("isPullingGun1", true);
            pullShotgun.PlayDelayed(0.5f);
        }
        else if (animationNum == 2) {
            arms.GetComponent<Animator>().SetBool("isPullingCube", true);
            pullCube.Play();
        }
        else if (animationNum == 3) {
            arms.GetComponent<Animator>().SetBool("isPullingGun2", true);
            pullPistol.Play();
        }

        Invoke("EndPullAnimation", 1f);

        for (int i = 0; i < transform.childCount; i++) {
            if (i == selectedWeapon) {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            else {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (animationNum == transform.childCount - 1)
        {
            for (int i = 0; i < go_buildables.Length; i++)
            {
                go_buildables[i].transform.GetChild(0).gameObject.SetActive(true);
                go_buildables[i].transform.GetChild(1).gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < go_buildables.Length; i++)
            {
                go_buildables[i].transform.GetChild(0).gameObject.SetActive(false);
                go_buildables[i].transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public void InitialSelectWeapon() {
        animationNum = armsAnimationType.animationType;

        if (animationNum == 0) {
            arms.GetComponent<Animator>().SetBool("isPullingGun0", true);
        }
        else if (animationNum == 1) {
            arms.GetComponent<Animator>().SetBool("isPullingGun1", true);
        }
        else if (animationNum == 2) {
            arms.GetComponent<Animator>().SetBool("isPullingCube", true);
        }
        else if (animationNum == 3) {
            arms.GetComponent<Animator>().SetBool("isPullingGun2", true);
        }

        Invoke("EndPullAnimation", 1f);
    }


    void EndPullAnimation() {
        if (animationNum == 0) {
            arms.GetComponent<Animator>().SetBool("isPullingGun0", false);
        }
        else if (animationNum == 1) {
            arms.GetComponent<Animator>().SetBool("isPullingGun1", false);
        }
        else if (animationNum == 2) {
            arms.GetComponent<Animator>().SetBool("isPullingCube", false);
            arms.GetComponent<Animator>().SetBool("isHoldingCube", true);
        }
        else if (animationNum == 3) {
            arms.GetComponent<Animator>().SetBool("isPullingGun2", false);
            arms.GetComponent<Animator>().SetBool("isHoldingPistol", true);
        }
    }


    void CancelAnimation() {
        if (armsAnimationType.animationType == 0) {
            arms.GetComponent<Animator>().SetBool("isReloading0", false);
            arms.GetComponent<Animator>().SetBool("isShooting0", false);
        }
        else if (armsAnimationType.animationType == 1) {
            arms.GetComponent<Animator>().SetBool("isReloading1", false);
            arms.GetComponent<Animator>().SetBool("isShooting1", false);
        }
        else if (armsAnimationType.animationType == 3) {
            arms.GetComponent<Animator>().SetBool("isReloading2", false);
            arms.GetComponent<Animator>().SetBool("isShooting2", false);
        }

        armsAnimationType = transform.GetChild(selectedWeapon).GetComponent<ArmsAnimationType>();
        reloadSound.Stop();
    }
}
