using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : BuildScript
{
    public int damage = 1;
    public float damageDistance = 2;
    public float delayTime = 0.5f;
    public float extendRate = 2f;
    public float stayTime = 1f;
    public float retractRate = 1f;
    public float cooldown = 3f;
    public float size = 0.9f;

    public float atkThreshold = 0.2f;

    private bool extended;
    private HashSet<GameObject> hitRecord;

    public AudioSettings a_shoot;

    void Awake()
    {
        hitRecord = new HashSet<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!extended && Physics.OverlapBox(transform.position + transform.up * damageDistance / 2f, new Vector3(size, damageDistance, size), transform.rotation, LayermaskReference.enemy).Length > 0)
        {
            Debug.Log("Start extending");
            StartCoroutine(Extend());
        }
        Helper.DrawCube(transform.position + transform.up * damageDistance / 2f, new Vector3(size, damageDistance, size), Color.blue, 0.05f);
    }

    IEnumerator Extend()
    {
        hitRecord.Clear();
        extended = true;
        float extendDistance = 0;
        yield return new WaitForSeconds(delayTime);

        GameObject a = Instantiate(GameManager.instance.pf_audio, transform.position, Quaternion.identity);
        Helper.AudioInit(a.GetComponent<AudioSource>(), a_shoot);
        Destroy(a, 2f);

        while (extendDistance < damageDistance)
        {
            //Debug.Log(extendDistance);
            extendDistance = Mathf.Clamp(extendDistance + Time.deltaTime * extendRate * damageDistance, 0f, damageDistance);
            Helper.DrawCube(transform.position + transform.up * extendDistance / 2f, new Vector3(size, extendDistance, size), Color.red, 0.01f);
            SetSpikeGraphic(extendDistance);
            if (extendDistance > damageDistance * atkThreshold)
            {
                foreach (Collider c in Physics.OverlapBox(transform.position + transform.up * extendDistance / 2f, new Vector3(size, extendDistance, size), transform.rotation, LayermaskReference.enemy))
                {
                    if (!hitRecord.Contains(c.transform.root.gameObject))
                    {
                        hitRecord.Add(c.transform.root.gameObject);
                        c.transform.root.GetComponent<EnemyController>().ChangeHealth(-damage);
                        Debug.Log("Hit" + c.transform.root.name + " - " + damage);
                    }
                }
            }
            yield return null;
        }
        yield return new WaitForSeconds(stayTime);
        while (extendDistance > 0)
        {
            extendDistance -= Time.deltaTime * retractRate * damageDistance;
            Helper.DrawCube(transform.position + transform.up * extendDistance / 2f, new Vector3(size, extendDistance, size), Color.red, 0.01f);
            SetSpikeGraphic(extendDistance);
            yield return null;
        }
        SetSpikeGraphic(0);
        yield return new WaitForSeconds(cooldown);
        extended = false;
    }

    private void SetSpikeGraphic(float distance)
    {
        float scale = transform.localScale.y * 2f;
        for (int i=0; i < 4; i++)
        {
            Transform child = transform.GetChild(i);
            child.localScale = new Vector3(child.localScale.x, Mathf.Clamp(distance / scale, 0.1f, 10f), child.localScale.z);
        }
        /*foreach (Transform child in transform)
        {
            //child.localPosition = new Vector3(child.localPosition.x, distance / scale / 2f, child.localPosition.z);
            child.localScale = new Vector3(child.localScale.x, Mathf.Clamp(distance / scale, 0.1f, 10f), child.localScale.z);
        }*/
    }
}
