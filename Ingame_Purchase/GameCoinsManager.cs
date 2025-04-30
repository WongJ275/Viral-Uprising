using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCoinsManager : MonoBehaviour
{
    public static GameCoinsManager instance;
    public int totalCoins = 0;

    void Awake()
    {
        instance = this;
    }

    public void changeCoins(int coins)
    {
        totalCoins += coins;
    }
}
