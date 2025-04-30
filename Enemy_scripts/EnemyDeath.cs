using System.Collections;
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    private Animator m_animator;
    public string a_deathName;
    private Renderer[] renderers;
    //public float deathTime = 1.0f;
    public float lightSpeed = 2.0f;
    public float dissolveSpeed = 1.0f;
    public float maxCutoff = 1.0f;
    public float minCutoff = 0.1f;
    public Material dissolveMat;

    void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
    }
    
    public void Death(float multiplier = 1.0f)
    {
        lightSpeed *= multiplier;
        dissolveSpeed *= multiplier;
        if (m_animator != null)
        {
            m_animator.Play(a_deathName);
        }
        StartCoroutine(DeathCoroutine());
        StartCoroutine(VolumeDown());
    }

    public IEnumerator VolumeDown()
    {
        AudioSource a = GetComponent<AudioSource>();
        float v = a.volume;
        float time = 1f / lightSpeed + 1f / dissolveSpeed;
        if (a == null) yield break;
        float t = time;

        while (t > 0)
        {
            a.volume = v * t;
            t -= Time.deltaTime;
            yield return null;
        }
        a.volume = 0f;

    }

    public IEnumerator DeathCoroutine()
    {
        Debug.Log(renderers.Length);
        foreach (Renderer r in renderers)
        {
            Material[] ms = new Material[r.materials.Length + 1];
            for (int i = 1; i < ms.Length; i++)
            {
                ms[i] = r.materials[i-1];
            }
            ms[0] = dissolveMat;
            r.materials = ms;
        }
        float t = maxCutoff;
        while (t > minCutoff)
        {
            foreach (Renderer r in renderers)
            {
                r.materials[0].SetFloat("_cutoff", t);
            }
            t -= lightSpeed * Time.deltaTime;
            yield return null;
        }
        t = minCutoff;
        foreach (Renderer r in renderers)
        {
            r.materials = new Material[] { dissolveMat };
            r.materials[0].SetFloat("_cutoff", t);
        }
        yield return null;
        while (t < maxCutoff)
        {
            foreach (Renderer r in renderers)
            {
                r.materials[0].SetFloat("_cutoff", t);
            }
            t += dissolveSpeed * Time.deltaTime;
            yield return null;
        }
        t = maxCutoff;
        foreach (Renderer r in renderers)
        {
            r.materials[0].SetFloat("_cutoff", t);
        }
        yield return null;
        //yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
