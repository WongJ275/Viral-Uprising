using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunTrap : BuildScript
{
    public float stunDuration = 1f;
    public float stunInvulDuration = 2f;
    public Vector3 detectSpace = Vector3.one;
    public float cooldown = 5f;

    public GameObject pf_effect;

    private bool isOnCooldown;

    public AudioSettings a_shoot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isOnCooldown)
        {
            int count = 0;
            foreach (Collider c in Physics.OverlapBox(transform.position + transform.up * detectSpace.y / 2f, detectSpace, transform.rotation, LayermaskReference.enemy))
            {
                count += c.transform.root.GetComponent<EnemyStatus>().AddStatus(EnemyStatus.Status.Stun, stunDuration + stunInvulDuration, this.gameObject, stunDuration);
            }
            if (count > 0)
            {
                Debug.Log("Stun " + count + " enemies");

                GameObject a = Instantiate(GameManager.instance.pf_audio, transform.position, Quaternion.identity);
                Helper.AudioInit(a.GetComponent<AudioSource>(), a_shoot);
                Destroy(a, 2f);

                GameObject e = Instantiate(pf_effect, transform.position, transform.rotation);
                Destroy(e, 3f);
                StartCoroutine(Cooldown());
            }
        }
    }

    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
