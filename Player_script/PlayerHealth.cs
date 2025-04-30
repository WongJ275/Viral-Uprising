using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerHealth : MonoBehaviour
{

    public int maxHealth, currentHealth, shieldHealth = 0;
    public bool isDead;
    public ItemsPurchaseManager itemsPurchaseManager;

    public PostProcessVolume volume;

    private float flashAmount = 0f;

    public float maxFlashAmount = 0.6f;
    public float flashMultiplier = 5f;

    public float hpFlash = 0.5f;

    public float flashFade = 1f;

    private Vignette vignette;

    public AudioSource getHitSound;

    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGetSettings(out vignette);
    }

    // Update is called once per frame
    void Update()
    {
        if (itemsPurchaseManager.hasIncreasedMaxHealth)
        {
            maxHealth = 120;
            currentHealth += 20;
            itemsPurchaseManager.hasIncreasedMaxHealth = false;
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }

        if (vignette != null)
        {
            vignette.intensity.Override(Mathf.Clamp(flashAmount + (1f - (float)currentHealth / (float)maxHealth) * hpFlash * maxFlashAmount, 0f, maxFlashAmount));
        }
        flashAmount = Mathf.Clamp(flashAmount - Time.deltaTime * flashFade, 0f, maxFlashAmount);
    }

    void Die()
    {
        isDead = true;
        GameManager.instance.Lose();
        //
    }

    public void TakeDamage(int damage)
    {
        if (damage > 0) {
            flashAmount = Mathf.Clamp(flashAmount + (float)damage / (float)maxHealth, 0.3f, maxFlashAmount);
        }
        
        if (shieldHealth > 0)
        {
            shieldHealth -= damage;
            if (shieldHealth < 0)
            {
                currentHealth += shieldHealth;
                GameManager.healthLoss += shieldHealth;
                shieldHealth = 0;
            }
        }
        else
        {
            currentHealth -= damage;
            GameManager.healthLoss += damage;

            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
        }

        if (damage > 0)
        {
            getHitSound.Play();
        }
    }

}
