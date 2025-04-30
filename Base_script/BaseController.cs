using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth = 100;
    public static BaseController instance;

    public GameObject warning;
    public bool stationUnderAttack = false;
    public float warningTime = 5f;
    public AudioSource warningSound;

    void Awake()
    {
        instance = this;
        currentHealth = maxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (stationUnderAttack) {
            if (!warning.activeSelf) {
                warning.SetActive(true);
            }
            warningTime -= Time.deltaTime;

            if (warningTime > 0 && !warningSound.isPlaying)
            {
                warningSound.Play();
            }
            
            if (warningTime <= 0)
            {
                warning.SetActive(false);
                warningTime = 5f;
                stationUnderAttack = false;
                warningSound.Stop();
            }
        }
    }

    public void ChangeHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        stationUnderAttack = true;
        warningTime = 5f;

        if (currentHealth <= 0)
        {
            //Destroy(gameObject);
            GameManager.instance.Lose();
            Debug.Log("Base Destroyed");
        }
    }


}
