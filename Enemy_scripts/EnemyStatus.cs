using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    public Dictionary<Status, List<StatusEffect>> statusEffects;
    private List<StatusProfile> statusProfiles;
    private EnemyController s_enemyController;
    public Renderer[] renderersIgnore;
    public Material m_stun;
    public Material m_slow;
    public float stunCutoff = 0.6f;
    public float slowCutoff = 0.6f;

    private Renderer[] renderers;

    private bool wasStun;
    private bool wasSlow;

    void Awake()
    {
        statusEffects = new();
        foreach (Status status in Enum.GetValues(typeof(Status)))
        {
            statusEffects.Add(status, new List<StatusEffect>());
        }
        statusProfiles = new List<StatusProfile>
        {
            new StatusProfile(Status.Slow, true, 3, false),
            new StatusProfile(Status.Stun, false, 1, false),
        };
        s_enemyController = GetComponent<EnemyController>();
        renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!renderersIgnore.Contains(renderer))
            {
                Material[] ms = new Material[renderer.materials.Length + 2];
                for (int i = 2; i < ms.Length; i++)
                {
                    ms[i] = renderer.materials[i - 2];
                }
                ms[0] = m_stun;
                ms[1] = m_slow;
                renderer.materials = ms;
                renderer.materials[0].SetFloat("_cutoff", 1f);
                renderer.materials[1].SetFloat("_cutoff", 1f);
            }
        }
    }

    void Start()
    {
        /*
        //test
        AddStatus(Status.Slow, 5f, 0.5f);
        AddStatus(Status.Slow, 10f, 0.5f);
        AddStatus(Status.Slow, 10f, 0.5f);
        AddStatus(Status.Slow, 3f, 0.1f);
        */
    }

    void LateUpdate()
    {
        if (s_enemyController != null && !s_enemyController.isDead)
        {
            foreach (Status status in Enum.GetValues(typeof(Status)))
            {
                //string s = status.ToString();
                if (statusEffects[status] != null)
                {
                    for (int i = statusEffects[status].Count - 1; i >= 0; i--)
                    {
                        //s += " " + statusEffects[status][i].timeLeft;
                        statusEffects[status][i].timeLeft -= Time.deltaTime;
                        if (statusEffects[status][i].timeLeft <= 0)
                        {
                            statusEffects[status].RemoveAt(i);
                        }
                    }
                }
                //Debug.Log(s);
            }
            if (statusEffects[Status.Stun].Count > 0)
            {
                if (!wasStun)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        if (!renderersIgnore.Contains(renderer))
                        {
                            renderer.materials[0].SetFloat("_cutoff", stunCutoff);
                        }
                    }
                    wasStun = true;
                }
            }
            else
            {
                if (wasStun)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        if (!renderersIgnore.Contains(renderer))
                        {
                            renderer.materials[0].SetFloat("_cutoff", 1.0f);
                        }
                    }
                    wasStun = false;
                }
            }
            if (statusEffects[Status.Slow].Count > 0)
            {
                if (!wasSlow)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        if (!renderersIgnore.Contains(renderer))
                        {
                            renderer.materials[1].SetFloat("_cutoff", slowCutoff);
                        }
                    }
                    wasSlow = true;
                }
            }
            else
            {
                if (wasSlow)
                {
                    foreach (Renderer renderer in renderers)
                    {
                        if (!renderersIgnore.Contains(renderer))
                        {
                            renderer.materials[1].SetFloat("_cutoff", 1.0f);
                        }
                    }
                    wasSlow = false;
                }
            }
        }
    }

    [Description("return [0:not added, 1:added, 2:replaced]")]
    public int AddStatus(Status status, float duration, GameObject go, float param = float.NaN)
    {
        StatusEffect newStatus = new StatusEffect(duration, param, go);
        StatusProfile p = statusProfiles.Find(x => x.statusID == status);
        for (int i = 0; i < statusProfiles.Count; i++)
        {
            if (statusProfiles[i].statusID == status)
            {
                p = statusProfiles[i];
                break;
            }
        }
        if (p.replaceable)
        {
            int n = statusEffects[status].Count;
            int id = -1;
            if (!p.stacksSingle)
            {
                for (int i = 0; i < n; i++)
                {
                    if (statusEffects[status][i].go == go)
                    {
                        id = i;
                        break;
                    }
                }
                //Debug.Log(id);
                if (id > -1)
                {
                    statusEffects[status][id] = newStatus;
                    return 2;
                }
                else
                {
                    if (n < p.stacks)
                    {
                        statusEffects[status].Add(newStatus);
                        return 1;
                    }
                }
            }
        }
        else
        {
            int n = statusEffects[status].Count;
            if (!p.stacksSingle)
            {
                for (int i = 0; i < n; i++)
                {
                    if (statusEffects[status][i].go == go)
                    {
                        return 0;
                    }
                }
                if (n < p.stacks)
                {
                    statusEffects[status].Add(newStatus);
                    return 1;
                }
            }
        }
        return 0;
    }

    public class StatusProfile
    {
        public Status statusID;
        public bool replaceable;
        public int stacks;
        public bool stacksSingle;
        public StatusProfile(Status statusID, bool replaceable, int stacks, bool stacksSingle)
        {
            this.statusID = statusID;
            this.replaceable = replaceable;
            this.stacks = stacks;
            this.stacksSingle = stacksSingle;
        }
    }

    [Serializable]
    public class StatusEffect
    {
        public float param;
        public float duration;
        public float timeLeft;
        public GameObject go;

        public StatusEffect(float duration, float param, GameObject go)
        {
            this.duration = duration;
            timeLeft = duration;
            this.param = param;
            this.go = go;
        }
    }

    [Serializable]
    public enum Status
    {
        Slow,
        Stun,
        Burn
    }
}
