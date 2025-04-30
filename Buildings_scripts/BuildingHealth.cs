using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHealth : BuildScript
{
    public int maxHealth = 10;
    public int currentHealth;

    public GameObject pf_deadEffect;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        BreakableCollection.t_breakables.Add(transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        foreach (var v in GetComponentsInChildren<BuildScript>())
        {
            v.dead = true;
        }
        BreakableCollection.t_breakables.Remove(transform);
        if (GetComponent<HpBar>() != null)
        {
            GetComponent<HpBar>().Dead();
        }
        if (this.GetComponent<BuildProperty>() != null && this.GetComponent<BuildProperty>().s_platform != null)
        {
            this.GetComponent<BuildProperty>().s_platform.occupied = false;
        }
        GameObject e = Instantiate(pf_deadEffect, transform.position + GetComponent<TargetOffset>().Offset, Quaternion.identity);
        Destroy(e, 3f);
        Destroy(gameObject);
    }
}
