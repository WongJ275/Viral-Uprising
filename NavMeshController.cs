using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshController : MonoBehaviour
{
    public float bakeRate = 0.1f;
    private NavMeshSurface NavMeshSurface;
    public float updateTime;

    void Awake()
    {
        NavMeshSurface = GetComponent<NavMeshSurface>();
    }

    void Start()
    {
        StartCoroutine(AutoBake(bakeRate));
    }

    private IEnumerator AutoBake(float rate)
    {
        while(true)
        {
            float t = 0f;
            AsyncOperation operation = NavMeshSurface.UpdateNavMesh(NavMeshSurface.navMeshData);
            yield return null;
            while (!operation.isDone)
            {
                //Debug.Log("baking");
                t += Time.deltaTime;
                yield return null;
            }
            //Debug.Log("finish");
            updateTime = t;
            yield return new WaitForSeconds(rate);
        }
    }
}
