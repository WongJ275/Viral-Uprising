using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplode : MonoBehaviour
{
    public float damage = 5f;
    public float detectRadius = 1f;
    public float damageRadius = 3f;
    public AnimationCurve ac_damageMultiplier;
    public float explodeDelay = 1f;
    public AnimationCurve ac_light;
    public float lightFlashRate = 0.5f;
    public AnimationCurve ac_lightExplodeFlashMultiplier;

    private List<Light> lights;
    private List<float> lightsIntensity;

    private bool isExploding = false;

    private float timer = 0f;
    private float lightTimer = 0f;

    public GameObject pf_explosionEffect;
    private EnemyStatus s_status;
    private EnemyController s_enemyController;

    void Awake()
    {
        lights = new List<Light>(GetComponentsInChildren<Light>());
        lightsIntensity = new List<float>();
        foreach (var light in lights)
        {
            lightsIntensity.Add(light.intensity);
        }
        s_enemyController = GetComponentInParent<EnemyController>();
        s_status = GetComponentInParent<EnemyStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        if (s_enemyController != null && !s_enemyController.isDead)
        {
            if (isExploding)
            {
                timer += Time.deltaTime * ComputeSpeedMultiplier();
                lightTimer += Time.deltaTime * lightFlashRate * ac_lightExplodeFlashMultiplier.Evaluate(timer / explodeDelay);
                if (timer >= explodeDelay)
                {
                    Dictionary<Transform, float> hits = new Dictionary<Transform, float>();
                    foreach (var col in Physics.OverlapSphere(transform.position, damageRadius, LayermaskReference.attackables))
                    {
                        float dam = damage * ac_damageMultiplier.Evaluate(Vector3.Distance(col.ClosestPoint(transform.position), transform.position) / damageRadius);
                        if (!hits.ContainsKey(col.transform.root) || hits[col.transform.root] < dam)
                        {
                            hits[col.transform.root] = dam;
                        }
                    }
                    foreach (var k in hits.Keys)
                    {
                        if (k.CompareTag("Player"))
                        {
                            //Debug.Log("Bullet Hit Player: " + damage);
                            k.GetComponent<PlayerHealth>().TakeDamage((int)hits[k]);
                        }
                        else if (k.CompareTag("BreakableBuilding"))
                        {
                            //Debug.Log("Bullet Hit Building: " + damage);
                            k.GetComponent<BuildingHealth>().TakeDamage((int)hits[k]);
                        }
                        else if (k.CompareTag("Base"))
                        {
                            //Debug.Log("Bullet Hit Base: " + damage);
                            k.GetComponent<BaseController>().ChangeHealth((int)-hits[k]);
                        }
                    }
                    /*foreach (var col in Physics.OverlapSphere(transform.position, damageRadius, LayermaskReference.attackables))
                    {
                        float dam = damage * ac_damageMultiplier.Evaluate(Vector3.Distance(col.ClosestPoint(transform.position), transform.position) / damageRadius);
                        if (col.transform.root.CompareTag("Player"))
                        {
                            Debug.Log("Bullet Hit Player: " + damage);
                            col.transform.root.GetComponent<PlayerHealth>().TakeDamage((int)dam);
                        }
                        else if (col.transform.root.CompareTag("BreakableBuilding"))
                        {
                            Debug.Log("Bullet Hit Building: " + damage);
                            col.transform.root.GetComponent<BuildingHealth>().TakeDamage((int)(dam));
                        }
                        else if (col.transform.root.CompareTag("Base"))
                        {
                            Debug.Log("Bullet Hit Base: " + damage);
                            col.transform.root.GetComponent<BaseController>().ChangeHealth((int)(-dam));
                        }
                    }*/
                    Debug.Log("explode");
                    GameObject e = Instantiate(pf_explosionEffect, transform.position, Quaternion.identity);
                    Destroy(e, 1f);
                    s_enemyController.Die(10f);
                }
            }
            else
            {
                lightTimer += Time.deltaTime * lightFlashRate * ComputeSpeedMultiplier();
            }
            lightTimer %= 1f;
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].intensity = lightsIntensity[i] * ac_light.Evaluate(lightTimer);
            }

            if (!isExploding)
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, detectRadius, LayermaskReference.attackables);
                foreach (var c in cols)
                {
                    if (c == null) continue;
                    if (c.transform.root == s_enemyController.t_atkTarget.root)
                    {
                        isExploding = true;
                    }
                }
            }
        }
        
    }

    private float ComputeSpeedMultiplier()
    {
        return (s_status.statusEffects[EnemyStatus.Status.Stun].Count > 0 && s_status.statusEffects[EnemyStatus.Status.Stun][0].timeLeft > s_status.statusEffects[EnemyStatus.Status.Stun][0].duration - s_status.statusEffects[EnemyStatus.Status.Stun][0].param) ? 0f : 1f;
    }
}
