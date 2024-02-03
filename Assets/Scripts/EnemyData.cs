using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New enemy", menuName = "Enemy")]
public class EnemyData : ScriptableObject
{
    public int enemyhealth;
    public int enemyattack;
    public int vision;
    public int range;
    public enemyClass imEnemy;
    public enum enemyClass
    {
        melee,
        ranged
    }
}
