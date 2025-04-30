using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayermaskReference : MonoBehaviour
{
    public static LayerMask playerAtkDetect = LayerMask.GetMask("Wall", "Enemy", "Building", "EnemyIgnore", "Base");
    public static LayerMask buildingDetect = LayerMask.GetMask("Wall", "Enemy", "Building", "EnemyIgnore", "Base");
    public static LayerMask enemy = LayerMask.GetMask("Enemy");
    public static LayerMask obstacles = LayerMask.GetMask("Building", "Player");
    public static LayerMask blocked = LayerMask.GetMask("Building", "Player", "Wall", "Base", "PlayerIgnore");
    public static LayerMask breakables = LayerMask.GetMask("Building", "Zone");
    public static LayerMask attackables = LayerMask.GetMask("Building", "Player", "Base", "PlayerIgnore");
    public static LayerMask sightTargetWithoutShield = LayerMask.GetMask("Building", "Base", "Wall", "Enemy");
    public static LayerMask sightTarget = LayerMask.GetMask("Building", "Base", "Wall", "Enemy", "PlayerIgnore");
}
