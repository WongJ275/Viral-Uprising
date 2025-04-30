using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    public float damage = 1f;
    public float atkRate = 2f;
    public float range = 5f;

    private float atkTimer = 0f;

    private EnemyStatus s_status;
    private EnemyController s_controller;

    public GameObject pf_explodeEffect;

    public AudioSettings a_shoot;

    void Awake()
    {
        s_status = GetComponentInParent<EnemyStatus>();
        s_controller = GetComponentInParent<EnemyController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (s_controller != null && !s_controller.isDead)
        {
            atkTimer = Mathf.Clamp(atkTimer - Time.deltaTime * ComputeSpeedMultiplier(), 0, atkRate);
            if (atkTimer <= 0f)
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, range, LayermaskReference.attackables);
                if (cols.Length > 0)
                {
                    atkTimer = atkRate;
                    foreach (Collider col in cols)
                    {
                        if (col.transform.root.CompareTag("Player"))
                        {
                            //Debug.Log("Bullet Hit Player: " + damage);
                            col.transform.root.GetComponent<PlayerHealth>().TakeDamage((int)damage);
                        }
                        else if (col.transform.root.CompareTag("BreakableBuilding"))
                        {
                            //Debug.Log("Bullet Hit Building: " + damage);
                            col.transform.root.GetComponent<BuildingHealth>().TakeDamage((int)damage);
                        }
                        else if (col.transform.root.CompareTag("Base"))
                        {
                            //Debug.Log("Bullet Hit Base: " + damage);
                            col.transform.root.GetComponent<BaseController>().ChangeHealth((int)-damage);
                        }
                    }
                    GameObject go = Instantiate(pf_explodeEffect, transform.position, transform.rotation);
                    Destroy(go, 10f);

                    GameObject a = Instantiate(GameManager.instance.pf_audio, transform);
                    Helper.AudioInit(a.GetComponent<AudioSource>(), a_shoot);
                    Destroy(a, 2f);
                }
            }
        }
    }

    private float ComputeSpeedMultiplier()
    {
        return (s_status.statusEffects[EnemyStatus.Status.Stun].Count > 0 && s_status.statusEffects[EnemyStatus.Status.Stun][0].timeLeft > s_status.statusEffects[EnemyStatus.Status.Stun][0].duration - s_status.statusEffects[EnemyStatus.Status.Stun][0].param) ? 0f : 1f;
    }
}
