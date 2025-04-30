using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinePlacer : BuildScript
{
    public GameObject pf_mines;
    public AnimationCurve ac_throwForce;
    public float cooldown = 1f;
    public AnimationCurve ac_throwAngle;
    public int maxMineCount = 3;
    public float stopRadius = 3f;
    public Vector3 offset = new Vector3(0, 0.25f, 0);
    private HashSet<GameObject> mineRecord = new HashSet<GameObject>();

    private bool IsOnCooldown = false;

    public AudioSettings a_throw;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOnCooldown && mineRecord.Count < maxMineCount && Physics.OverlapSphere(transform.position, stopRadius, LayermaskReference.enemy).Length == 0)
        {
            StartCoroutine(Throw());
        }
    }

    IEnumerator Throw()
    {
        IsOnCooldown = true;
        GameObject go = Instantiate(pf_mines, transform.position + offset, Quaternion.Euler(0f, UnityEngine.Random.value * 360f, 0f));
        go.GetComponent<Rigidbody>().AddForce((transform.up + go.transform.forward * Mathf.Tan(ac_throwAngle.Evaluate(UnityEngine.Random.value) * Mathf.Deg2Rad)) * ac_throwForce.Evaluate(UnityEngine.Random.value), ForceMode.Impulse);
        mineRecord.Add(go);
        go.GetComponent<Mine>().s_minePlacer = this;

        GameObject a = Instantiate(GameManager.instance.pf_audio, transform.position, Quaternion.identity);
        Helper.AudioInit(a.GetComponent<AudioSource>(), a_throw);
        Destroy(a, 2f);

        yield return new WaitForSeconds(cooldown);
        IsOnCooldown = false;
    }

    public void RemoveRecord(GameObject go)
    {
        mineRecord.Remove(go);
    }
}
