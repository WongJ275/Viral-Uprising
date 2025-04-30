using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class PlayerGunAttack : MonoBehaviour
{

    public Transform bulletSpawn;
    public RaycastHit hit;
    public Camera playerCam;
    private StarterAssetsInputs playerInput;
    public GameObject arms;
    public WeaponSwitching weaponSwitching;

    public UIScript uiScript;
    

    public float bulletSpeed, spread, damage;
    private float damageSave;
    public int maxAmmo, currentAmmo, bulletsPerBurst, burstCount, ammoCount, maxTotalAmmo;
    public float reloadTime, shootingDelay, burstDelay;    
    public bool isReloading, isShooting, canShoot, isAutoGun, isBursting;
    public bool canPlaySound = true;

    public float runSpread, walkSpread;


    [Header("Skill")]
    public PlayerSkillManager playerSkillManager;
    public float skillReloadBuff;
    private bool usingSkill = false;


    [Header("Animation")]
    public GameObject impactEffect;
    public GameObject muzzleFlash;
    public GameObject impactEffectEnemy;
    public GameObject bulletTrail;
    public ArmsAnimationType armsAnimationType;
    public float trailTime;
    public AudioSource reloadSound;
    public AudioSource shootSound;
    public AudioSource hitSound;


    [Header("Perks")]
    public bool hasFasterReload;
    private bool fasterReloadActivated;
    public bool hasLargerMagSize;
    public bool hasLowerHpHigherDamage;
    [SerializeField]
    private float fasterReloadBuff, lowerHpHigherDamageBuff;
    [SerializeField]
    private int largerMagSizeBuff;
    public ItemsPurchaseManager itemsPurchaseManager;
    public PlayerHealth playerHealth;



    void Start()
    {
        currentAmmo = maxAmmo;
        canShoot = true;
        playerInput = transform.root.GetComponent<StarterAssetsInputs>();
        weaponSwitching = transform.parent.GetComponent<WeaponSwitching>();
        reloadSound = weaponSwitching.reloadSound;
        hitSound = weaponSwitching.hitSound;
        damageSave = damage;
    }

    void Update()
    {
        CheckInput();
        ActivatePerk();
    }

    void Shoot() {
        canShoot = false;

        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);

        if (playerInput.move != Vector2.zero) {
            if (playerInput.sprint) {
                xSpread *= runSpread;
                ySpread *= runSpread;
            }
            else {
                xSpread *= walkSpread;
                ySpread *= walkSpread;
            }
        }

        Vector3 bulletDirection = playerCam.transform.forward + playerCam.transform.right * xSpread + playerCam.transform.up * ySpread;


        currentAmmo--;
        burstCount--;
        if (bulletsPerBurst == 1)
        {
            Invoke("AllowShoot", shootingDelay);
        }
        

        
        GameObject muzzle = Instantiate(muzzleFlash, bulletSpawn.position, bulletSpawn.rotation);
        muzzle.transform.parent = bulletSpawn;
        Destroy(muzzle, 0.2f);


        if (Physics.Raycast(playerCam.transform.position, bulletDirection, out hit, float.PositiveInfinity, LayermaskReference.playerAtkDetect)) {
            //Debug.Log(hit.distance);
            //Debug.Log(hit.transform.gameObject.name);
            GameManager.shotFired++;
            if (hit.transform.CompareTag("Enemy")) {
                hit.transform.root.GetComponent<EnemyController>().ChangeHealth(-damage, transform.root, hit.collider);
                uiScript.CrosshairHit();
                hitSound.Play();
            }

            

            if (trailTime > 0 && hit.distance > 2f) {

                Vector3 rayDirection = (hit.point - bulletSpawn.position).normalized;
                GameObject trail = Instantiate(bulletTrail, bulletSpawn.position, bulletTrail.transform.rotation);
                
                if (this.isActiveAndEnabled)
                {
                    StartCoroutine(BulletTrail(trail, rayDirection, hit.point));
                }
            }

            
            if (!hit.transform.CompareTag("Enemy")) {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 5.0f);

                //Debug.DrawRay(hit.point, hit.normal, Color.red, 5.0f);

                //Debug.Log(Quaternion.LookRotation(hit.normal));
            }
            else if (hit.transform.CompareTag("Enemy")) {
                GameObject impact = Instantiate(impactEffectEnemy, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 1.0f);
            }
            
        }

        if (burstCount > 0) {
            isBursting = true;
            Invoke("Shoot", burstDelay);
        }
        else {
            if (bulletsPerBurst > 1)
            {
                Invoke("AllowShoot", shootingDelay);
            }
            isBursting = false;
        }

        ShootAnimation();

        if (armsAnimationType.gunType == 1) {
            shootSound.Play();
        }
        else if (canPlaySound) {
            shootSound.Play();
            canPlaySound = false;
        }
    }


    void ShootAnimation() {
        if (armsAnimationType.animationType == 0) {
            arms.GetComponent<Animator>().SetBool("isShooting0", true);
            Invoke("ResetShootAnimation", 0.1f);
        }
        else if (armsAnimationType.animationType == 1) {
            arms.GetComponent<Animator>().SetBool("isShooting1", true);
            Invoke("ResetShootAnimation", 0.3f);
        }
        else if (armsAnimationType.animationType == 3) {
            arms.GetComponent<Animator>().SetBool("isShooting2", true);
            Invoke("ResetShootAnimation", 0.1f);
        }
        
    }

    void ResetShootAnimation() {
        if (armsAnimationType.animationType == 0) {
            arms.GetComponent<Animator>().SetBool("isShooting0", false);
        }
        else if (armsAnimationType.animationType == 1) {
            arms.GetComponent<Animator>().SetBool("isShooting1", false);
        }
        else if (armsAnimationType.animationType == 3) {
            arms.GetComponent<Animator>().SetBool("isShooting2", false);
        }
    }

    void AllowShoot() {
        canShoot = true;
        canPlaySound = true;
    }

    void Reload() {
        isReloading = true;
        SetAnimation();
        arms.GetComponent<Animator>().SetFloat("reloadSpeed", 1.5f / reloadTime);
        reloadSound.pitch = 2.7f / reloadTime;
        reloadSound.Play();
        Invoke("Reloaded", reloadTime);
    }

    void Reloaded() {
        if (isReloading) {
            if (!playerSkillManager.usingInfiniteBulletSkill) {
                ammoCount = ammoCount - (maxAmmo - currentAmmo);
                if (ammoCount < 0) {
                    currentAmmo += maxAmmo - currentAmmo + ammoCount;
                    ammoCount = 0;
                }
                else {
                    currentAmmo = maxAmmo;
                }
            }
            else if (playerSkillManager.usingInfiniteBulletSkill) {
                    currentAmmo = maxAmmo;
            }
            ResetAnimation();
            isReloading = false;
        }
    }

    void SetAnimation() {
        
        if (armsAnimationType.animationType == 0) {
            arms.GetComponent<Animator>().SetBool("isShooting0", false);
            arms.GetComponent<Animator>().SetBool("isReloading0", true);
        }
        else if (armsAnimationType.animationType == 1) {
            arms.GetComponent<Animator>().SetBool("isShooting1", false);
            arms.GetComponent<Animator>().SetBool("isReloading1", true);
        }
        else if (armsAnimationType.animationType == 3) {
            arms.GetComponent<Animator>().SetBool("isShooting2", false);
            arms.GetComponent<Animator>().SetBool("isReloading2", true);
        }
    }

    void ResetAnimation() {
        if (armsAnimationType.animationType == 0) {
            arms.GetComponent<Animator>().SetBool("isReloading0", false);
        }
        else if (armsAnimationType.animationType == 1) {
            arms.GetComponent<Animator>().SetBool("isReloading1", false);
        }
        else if (armsAnimationType.animationType == 3) {
            arms.GetComponent<Animator>().SetBool("isReloading2", false);
        }
    }


    void CheckInput() {
        if (isAutoGun) {
            isShooting = Input.GetKey(KeyCode.Mouse0) && !GameManager.instance.showingSettingsMenu;
        }
        else {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0) && !GameManager.instance.showingSettingsMenu;
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && !isBursting && currentAmmo < maxAmmo && (ammoCount > 0 || playerSkillManager.usingInfiniteBulletSkill) && !arms.GetComponent<Animator>().GetBool("isPullingGun0") && !arms.GetComponent<Animator>().GetBool("isPullingGun1") && !arms.GetComponent<Animator>().GetBool("isPullingGun2") && !GameManager.instance.showingSettingsMenu) { 
            Reload();
        }

        if (currentAmmo == 0 && !isReloading && isShooting && (ammoCount > 0 || playerSkillManager.usingInfiniteBulletSkill) && !arms.GetComponent<Animator>().GetBool("isPullingGun0") && !arms.GetComponent<Animator>().GetBool("isPullingGun1") && !arms.GetComponent<Animator>().GetBool("isPullingGun2")) {
            Reload();
        }

        if (isShooting && canShoot && !isReloading && currentAmmo > 0 && !isBursting && !arms.GetComponent<Animator>().GetBool("isPullingGun0") && !arms.GetComponent<Animator>().GetBool("isPullingGun1") && !arms.GetComponent<Animator>().GetBool("isPullingGun2")) {
            burstCount = bulletsPerBurst;
            uiScript.CallUpdateCrosshair();
            Shoot();
        }

    }

    public void ActivatePerk() {

        if (!hasFasterReload) {
            hasFasterReload = itemsPurchaseManager.hasFasterReload;
        }

        if (!hasLargerMagSize) {
            hasLargerMagSize = itemsPurchaseManager.hasLargerMagSize;
        }

        if (!hasLowerHpHigherDamage) {
            hasLowerHpHigherDamage = itemsPurchaseManager.hasLowerHpHigherDamage;
        }

        if (hasLargerMagSize) {
            maxAmmo = largerMagSizeBuff;
        }

        if (hasLowerHpHigherDamage) {
            if (playerHealth.maxHealth == 100) {
                if (playerHealth.currentHealth < 40) {
                    damage = lowerHpHigherDamageBuff;
                }
                else {
                    damage = damageSave;
                }
            }
            else if (playerHealth.maxHealth == 120) {
                if (playerHealth.currentHealth < 48) {
                    damage = lowerHpHigherDamageBuff;
                }
                else {
                    damage = damageSave;
                }
            }
        }

        if (hasFasterReload && !fasterReloadActivated) {
            fasterReloadActivated = true;
            reloadTime -= fasterReloadBuff;
        }

        if (playerSkillManager.usingInfiniteBulletSkill && !usingSkill) {
            usingSkill = true;
            reloadTime -= skillReloadBuff;
            Invoke("ResetReloadTime", playerSkillManager.skillDuration);
        }
    }

    public void ResetReloadTime() {
        usingSkill = false;
        playerSkillManager.usingSkill = false;
        reloadTime += skillReloadBuff;
        playerSkillManager.usingInfiniteBulletSkill = false;
        
    }


    IEnumerator BulletTrail(GameObject trail, Vector3 direction, Vector3 hitPoint) {
        float time = 0;

        trail.transform.rotation = Quaternion.LookRotation(direction);
        trail.transform.position = bulletSpawn.position;
        trail.transform.parent = bulletSpawn;

        while (time < 0.1f) {
            trail.transform.position = Vector3.Lerp(trail.transform.position, hitPoint, time / trailTime);
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(trail);
    }
}

