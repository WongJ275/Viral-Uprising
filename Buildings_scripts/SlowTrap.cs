using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTrap : BuildScript
{
    //public float slowDuration = 2f;
    public float slowRate = 0.9f;
    //public float cooldown = 1f;
    public float slowRadius = 5f;
    //public float detectRadius = 4.5f;

    public GameObject go_effect;

    //private bool isOnCooldown;
    // Start is called before the first frame update
    void Start()
    {
        go_effect.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Collider c in Physics.OverlapSphere(transform.position, slowRadius, LayermaskReference.enemy))
        {
            c.transform.root.GetComponent<EnemyStatus>().AddStatus(EnemyStatus.Status.Slow, 0.1f, this.gameObject, slowRate);
        }
    }

    /*IEnumerator Slow()
    {
        isOnCooldown = true;
        foreach (Collider c in Physics.OverlapSphere(transform.position, slowRadius, LayerMask.GetMask("Enemy")))
        {
            c.transform.root.GetComponent<EnemyStatus>().AddStatus(EnemyStatus.Status.Slow, slowDuration, slowRate);
        }
        yield return new WaitForSeconds(slowDuration);
        isOnCooldown = false;
    }*/
}
