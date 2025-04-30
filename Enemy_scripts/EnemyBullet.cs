using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 1f;
    public float speed = 10f;
    public Transform t_source;
    public GameObject pf_hitEffect;
    public AudioSettings a_hit;

    public void Init(Transform t)
    {
        t_source = t;
    }

    public void Init(Transform t, float _damage, float _speed)
    {
        damage = _damage;
        speed = _speed;
        t_source = t;
    }
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.root == t_source) return;
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
            col.transform.root.GetComponent<BaseController>().ChangeHealth((int)(-damage));
        }
        GameObject go = Instantiate(pf_hitEffect, transform.position, transform.rotation);
        GameObject a = Instantiate(GameManager.instance.pf_audio, transform.position, Quaternion.identity);
        Helper.AudioInit(a.GetComponent<AudioSource>(), a_hit);
        Destroy(a, 10f);
        Destroy(go, 2f);
        Destroy(gameObject);
    }
}
