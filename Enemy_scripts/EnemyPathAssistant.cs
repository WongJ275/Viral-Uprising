using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathAssistant : MonoBehaviour
{
    public Transform enemy;
    public void Init(Transform enemy)
    {
        this.enemy = enemy;
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy == null) 
        { 
            Destroy(gameObject); 
            return;
        }
        transform.position = enemy.position;
    }
}
