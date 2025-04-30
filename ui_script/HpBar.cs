using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public GameObject pf_hpBar;

    public float fadeRate = 1.0f;
    public float fadeDelay = 0f;
    private float fadeCounter = 0f;

    public float midRate = 0.3f;
    public float midDelay = 1f;
    public float midCounter = 1f;
    private float midHp;

    private Dictionary<Image, Color> dic = new Dictionary<Image, Color>();
    public RectTransform rt_medium;
    public RectTransform rt_front;

    public int width = 10;
    public float offsetY = 1f;
    public Vector3 offset;

    public bool isEnemy = true;

    private GameObject go_hpBar;

    private EnemyController s_enemyController;
    private BuildingHealth s_buildingHealth;

    private bool isDead = false;

    public float oldHp;

    void Awake()
    {
        if (isEnemy)
        {
            s_enemyController = GetComponent<EnemyController>();
            midHp = s_enemyController.maxHealth;
            oldHp = s_enemyController.maxHealth;
        }
        else
        {
            s_buildingHealth = GetComponent<BuildingHealth>();
            midHp = s_buildingHealth.maxHealth;
            oldHp = s_buildingHealth.maxHealth;
        }
    }

    void Start()
    {
        go_hpBar = Instantiate(pf_hpBar, GameObject.Find("Hp Canvas").transform);
        rt_medium = go_hpBar.transform.GetChild(1).GetComponent<RectTransform>();
        rt_front = go_hpBar.transform.GetChild(2).GetComponent<RectTransform>();
        go_hpBar.GetComponent<RectTransform>().sizeDelta = new Vector2(width, go_hpBar.GetComponent<RectTransform>().sizeDelta.y);
        go_hpBar.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(width, go_hpBar.transform.GetChild(1).GetComponent<RectTransform>().sizeDelta.y);
        go_hpBar.transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(width, go_hpBar.transform.GetChild(2).GetComponent<RectTransform>().sizeDelta.y);

        foreach (var v in go_hpBar.GetComponentsInChildren<Image>())
        {
            dic[v] = v.color;
            Color c = new Color(dic[v].r, dic[v].g, dic[v].b, 0f);
            v.color = c;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        fadeCounter = Mathf.Clamp(fadeCounter - Time.deltaTime * fadeRate, 0f, 1f);
        midCounter -= midRate * Time.deltaTime;
        foreach(var v in dic.Keys)
        {
            Color c = new Color(dic[v].r, dic[v].g, dic[v].b, dic[v].a * fadeCounter);
            v.color = c;
        }
        if (fadeCounter >= 0f)
        {
            if (isEnemy)
            {
                rt_front.sizeDelta = new Vector2(width * s_enemyController.curHealth / s_enemyController.maxHealth, rt_front.sizeDelta.y);
                if (oldHp > s_enemyController.curHealth)
                {
                    midCounter = midDelay;
                }
                if (midCounter <= 0f)
                {
                    midHp = Mathf.Clamp(midHp - midRate * s_enemyController.maxHealth * Time.deltaTime, s_enemyController.curHealth, s_enemyController.maxHealth);
                }
                rt_medium.sizeDelta = new Vector2(width * midHp / s_enemyController.maxHealth, rt_medium.sizeDelta.y);

                oldHp = s_enemyController.curHealth;
            }
            else
            {
                rt_front.sizeDelta = new Vector2(width * s_buildingHealth.currentHealth / s_buildingHealth.maxHealth, rt_front.sizeDelta.y);
                if (oldHp > s_buildingHealth.currentHealth)
                {
                    midCounter = midDelay;
                }
                if (midCounter <= 0f)
                {
                    midHp = Mathf.Clamp(midHp - midRate * s_buildingHealth.maxHealth * Time.deltaTime, s_buildingHealth.currentHealth, s_buildingHealth.maxHealth);
                }
                rt_medium.sizeDelta = new Vector2(width * midHp / s_buildingHealth.maxHealth, rt_medium.sizeDelta.y);
                oldHp = s_buildingHealth.currentHealth;
            }
            
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position + transform.right * offset.x + transform.up * offset.y + transform.forward * offset.z);
            Debug.Log(screenPos);
            go_hpBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(screenPos.x /*/ Screen.width * 1920f*/, screenPos.y /*/ Screen.height * 1080f*/);
        }
        else
        {
            if (isEnemy)
            {
                midHp = s_enemyController.curHealth;
            }
            else
            {
                midHp = s_buildingHealth.currentHealth;
            }
        }
    }

    public void Show()
    {
        fadeCounter = 1.0f + fadeDelay + Time.deltaTime;
    }

    public void Dead()
    {
        isDead = true;
        Destroy(go_hpBar);
    }
}
