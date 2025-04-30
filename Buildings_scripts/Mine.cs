using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public float damage = 5f;
    public float detectRadius = 1f;
    public float damageRadius = 3f;
    public AnimationCurve ac_damageMultiplier;
    public MinePlacer s_minePlacer;
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

    void Awake()
    {
        lights = new List<Light>(GetComponentsInChildren<Light>());
        lightsIntensity = new List<float>();
        foreach (var light in lights)
        {
            lightsIntensity.Add(light.intensity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isExploding)
        {
            timer += Time.deltaTime;
            lightTimer += Time.deltaTime * lightFlashRate * ac_lightExplodeFlashMultiplier.Evaluate(timer / explodeDelay);
            if (timer >= explodeDelay)
            {
                Dictionary<Transform,float> hits = new Dictionary<Transform,float>();
                foreach (var col in Physics.OverlapSphere(transform.position, damageRadius, LayermaskReference.enemy))
                {
                    float dam = damage * ac_damageMultiplier.Evaluate(Vector3.Distance(col.ClosestPoint(transform.position), transform.position) / damageRadius);
                    if (!hits.ContainsKey(col.transform.root) || hits[col.transform.root] < dam)
                    {
                        hits[col.transform.root] = dam;
                    }
                }
                foreach (var k in hits.Keys)
                {
                    k.GetComponent<EnemyController>().ChangeHealth((int)(hits[k]));
                }
                //Debug.Log("explode");
                GameObject e = Instantiate(pf_explosionEffect, transform.position, Quaternion.identity);
                Destroy(e, 1f);
                s_minePlacer.RemoveRecord(gameObject);
                Destroy(gameObject);
            }
        }
        else
        {
            lightTimer += Time.deltaTime * lightFlashRate;
        }
        lightTimer %= 1f;
        for (int i = 0; i < lights.Count; i++)
        {
            lights[i].intensity = lightsIntensity[i] * ac_light.Evaluate(lightTimer);
        }

        if (!isExploding && Physics.OverlapSphere(transform.position, detectRadius, LayermaskReference.enemy).Length > 0)
        {
            isExploding = true;
        }
    }

    public void Init(MinePlacer minePlacer)
    {
        s_minePlacer = minePlacer;
    }
}
