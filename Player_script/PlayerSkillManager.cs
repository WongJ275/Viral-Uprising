using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Rendering.PostProcessing;

public class PlayerSkillManager : MonoBehaviour
{
    public int chosenSkill;
    public bool isCD = false;
    public int skillCD;
    public int skillDuration;
    public float skillCDRemaining;
    public int speedBuff;
    public bool usingInfiniteBulletSkill = false;
    public bool usingSkill = false;

    public AudioSource skillActivateSound;
    public AudioSource rampageSound;
    public AudioSource healSound;
    public AudioSource skillEndSound;

    public PlayerHealth playerHealth;
    public FirstPersonController firstPersonController;

    public PostProcessVolume volume;
    public Vignette vignette;
    private float flashAmount = 0f;
    public float maxFlashAmount = 0.6f;
    public float flashFade = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        if (!SaveLoad.initiated)
        {
            SaveLoad.Initiate();
        }
        chosenSkill = (SaveLoad.currentSaveData.skillsEquipped == null || SaveLoad.currentSaveData.skillsEquipped.Count == 0) ? 0 : SaveLoad.currentSaveData.skillsEquipped[0];
        // heal skill
        if (chosenSkill == 0)
        {
            skillCD = 30;
            skillDuration = 5;
        }
        // shield skill
        else if (chosenSkill == 1)
        {
            skillCD = 30;
            skillDuration = 10;
        }
        // speed skill
        else if (chosenSkill == 2)
        {
            skillCD = 20;
            skillDuration = 5;
        }
        // infinite bullet and faster reload skill
        else if (chosenSkill == 3)
        {
            skillCD = 60;
            skillDuration = 10;
        }

        volume.profile.TryGetSettings(out vignette);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && !isCD && !GameManager.instance.showingSettingsMenu)
        {
            isCD = true;

            if (chosenSkill == 0)
            {
                HealSkill();
            }
            else if (chosenSkill == 1)
            {
                ShieldSkill();
            }
            else if (chosenSkill == 2)
            {
                SpeedSkill();
            }
            else if (chosenSkill == 3)
            {
                InfiniteBulletSkill();
            }
            skillActivateSound.Play();
            usingSkill = true;
            //Invoke("resetCD", skillCD + skillDuration);
            resetCD();
        }

        if (rampageSound.isPlaying && !usingInfiniteBulletSkill)
        {
            rampageSound.Stop();
        }

        if (vignette != null)
        {
            vignette.intensity.Override(Mathf.Clamp(flashAmount, 0f, maxFlashAmount));
        }
        flashAmount = Mathf.Clamp(flashAmount - Time.deltaTime * flashFade, 0f, maxFlashAmount);

    }

    public void HealSkill()
    {
        InvokeRepeating("Heal", 0, 1);
        Invoke("StopHeal", skillDuration);
    }

    public void Heal()
    {
        playerHealth.currentHealth += 5;
        if (playerHealth.currentHealth > playerHealth.maxHealth)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
        }
        healSound.Play();

        flashAmount = 0.4f;
    }

    public void StopHeal()
    {
        CancelInvoke("Heal");
        usingSkill = false;
    }


    public void ShieldSkill()
    {
        playerHealth.shieldHealth = 30;
        Invoke("StopShield", skillDuration);
    }

    public void StopShield()
    {
        playerHealth.shieldHealth = 0;
        usingSkill = false;
        skillEndSound.Play();
    }


    public void SpeedSkill()
    {
        firstPersonController.MoveSpeed += speedBuff;
        firstPersonController.SprintSpeed += speedBuff;
        Invoke("StopSpeed", skillDuration);
    }

    public void StopSpeed()
    {
        firstPersonController.MoveSpeed -= speedBuff;
        firstPersonController.SprintSpeed -= speedBuff;
        usingSkill = false;
        skillEndSound.Play();
    }


    public void InfiniteBulletSkill()
    {
        usingInfiniteBulletSkill = true;
        rampageSound.Play();
    }


    public void resetCD()
    {
        //isCD = false;

        skillCDRemaining = skillCD;
        StartCoroutine(CoolDown());
    }


    IEnumerator CoolDown()
    {
        while (skillCDRemaining > 0)
        {
            skillCDRemaining -= Time.deltaTime;
            yield return null;
        }
        isCD = false;
    }
}
