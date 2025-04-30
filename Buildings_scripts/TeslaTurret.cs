using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeslaTurret : BuildScript
{
    public float damage = 1f;
    public float cooldown = 0.5f;
    public float range = 5f;
    public int chainTargets = 3;
    private bool onCooldown = false;
    private Turret s_turret;
    public GameObject pf_trail;
    public GameObject pf_lightning;

    public Transform t_gunpoint;

    public int numChains = 3;

    public AudioSettings a_hit;

    void Awake()
    {
        s_turret = transform.root.GetComponent<Turret>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!dead && !onCooldown && s_turret.targetInSight)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        Debug.Log("Attack");
        onCooldown = true;

        HashSet<Transform> hitRecord = new HashSet<Transform>();
        RaycastHit hit;
        if (!Physics.Raycast(t_gunpoint.position, t_gunpoint.forward, out hit, Mathf.Infinity, LayerMask.GetMask("Wall", "Enemy", "Building", "EnemyIgnore")))
        {
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
            yield break;
        }

        GameObject a = Instantiate(GameManager.instance.pf_audio, transform.position, Quaternion.identity);
        Helper.AudioInit(a.GetComponent<AudioSource>(), a_hit);
        Destroy(a, 2f);

        /*GameObject go = Instantiate(pf_trail, transform.position, Quaternion.identity);
        go.GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
        go.GetComponent<LineRenderer>().SetPosition(1, hit.point - transform.position);
        Destroy(go, 0.05f);*/

        /*GameObject go = Instantiate(pf_lightning, transform.position, Quaternion.LookRotation(hit.point - transform.position));
        LineRenderer _lr = go.GetComponent<LineRenderer>();
        float _dist = (hit.point - transform.position).magnitude;
        Vector3[] _positions = new Vector3[(int)(_dist * 10) + 2];
        _positions[0] = Vector3.zero;
        _positions[_positions.Length - 1] = _dist * Vector3.forward;
        for (int j = 1; j < _positions.Length - 1; j++)
        {
            _positions[j] = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), j * 0.1f);
        }
        _lr.positionCount = _positions.Length;
        _lr.SetPositions(_positions);
        Destroy(go, 0.1f);*/

        if (hit.transform == null)
        {
            yield return new WaitForSeconds(cooldown);
            onCooldown = false;
            yield break;
        }

        LightningEffect(numChains, t_gunpoint.position, hit.transform.root.GetComponent<TargetOffset>().GetRealOffset(), 0.1f, 0.2f, 0.2f);

        if (hit.transform != null && hit.transform.CompareTag("Enemy"))
        {
            hit.transform.root.GetComponent<EnemyController>().ChangeHealth((int)(-damage), transform.root);

            GameObject aaa = Instantiate(GameManager.instance.pf_audio, hit.transform.position, Quaternion.identity);
            Helper.AudioInit(aaa.GetComponent<AudioSource>(), a_hit);
            Destroy(aaa, 2f);

            List<Transform> targets = new List<Transform>();
            foreach (Collider collider in Physics.OverlapSphere(hit.transform.position, range, LayermaskReference.enemy))
            {
                if (collider.transform.root == hit.transform.root || targets.Contains(collider.transform.root) || hitRecord.Contains(collider.transform.root)) continue;
                targets.Add(collider.transform.root);
            }
            targets.Sort((x, y) => Vector3.Distance(hit.transform.position, x.GetComponent<TargetOffset>().GetRealOffset()).CompareTo(Vector3.Distance(hit.transform.position, y.GetComponent<TargetOffset>().GetRealOffset())));
            int count = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null) continue;
                if (count < targets.Count)
                {
                    RaycastHit hit2;
                    if (Helper.RaycastIgnore(hit.transform.position, targets[i].GetComponent<TargetOffset>().GetRealOffset() - hit.transform.position, out hit2, range, LayermaskReference.buildingDetect, transform.root, hit.transform.root)
                        && hit2.transform.root == targets[i])
                    {
                        hitRecord.Add(targets[i]);
                        LightningEffect(numChains, hit.transform.root.GetComponent<TargetOffset>().GetRealOffset(), targets[i].GetComponent<TargetOffset>().GetRealOffset(), 0.1f, 0.2f, 0.2f);
                        /*for (int n = 0; n < numChains; n++)
                        {
                            GameObject go2 = Instantiate(pf_lightning, hit.transform.position, Quaternion.LookRotation(hitColliders[i].transform.position - hit.transform.position));
                            LineRenderer lr = go2.GetComponent<LineRenderer>();
                            float dist = (hitColliders[i].transform.position - hit.transform.position).magnitude;
                            Vector3[] positions = new Vector3[(int)(dist * 10) + 2];
                            positions[0] = Vector3.zero;
                            positions[positions.Length - 1] = dist * Vector3.forward;
                            for (int j = 1; j < positions.Length - 1; j++)
                            {
                                positions[j] = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), j * 0.1f);
                            }
                            lr.positionCount = positions.Length;
                            lr.SetPositions(positions);
                            Destroy(go2, 0.1f);
                        }*/
                        targets[i].GetComponent<EnemyController>().ChangeHealth((int)(-damage), transform.root);

                        GameObject aa = Instantiate(GameManager.instance.pf_audio, targets[i].transform.position, Quaternion.identity);
                        Helper.AudioInit(aa.GetComponent<AudioSource>(), a_hit);
                        Destroy(aa, 2f);

                        count++;
                    }
                }
            }
        }

        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    private void LightningEffect(int num, Vector3 from, Vector3 to, float f, float duration, float radius)
    {
        for (int i=0; i < num; i++)
        {
            GameObject go = Instantiate(pf_lightning, from, Quaternion.LookRotation(to - from));
            LineRenderer lr = go.GetComponent<LineRenderer>();
            float dist = (to - from).magnitude;
            Vector3[] positions = new Vector3[(int)(dist / f) + 2];
            positions[0] = Vector3.zero;
            positions[positions.Length - 1] = dist * Vector3.forward;
            for (int j = 1; j < positions.Length - 1; j++)
            {
                positions[j] = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), j * f);
            }
            lr.positionCount = positions.Length;
            lr.SetPositions(positions);
            StartCoroutine(LightningEffectChange(duration / 3f, f, lr, radius));
            Destroy(go, duration);
        }
    }

    private IEnumerator LightningEffectChange(float rate, float f, LineRenderer lr, float radius)
    {
        yield return new WaitForSeconds(rate);
        while (lr != null)
        {
            Vector3[] positions = new Vector3[lr.positionCount];
            for (int j = 1; j < positions.Length - 1; j++)
            {
                positions[j] = new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), j * f);
            }
            lr.SetPositions(positions);
            yield return new WaitForSeconds(rate);
        }
    }
}
