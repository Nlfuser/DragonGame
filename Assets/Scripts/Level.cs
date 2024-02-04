using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New level", menuName = "Level")]
public class Level : ScriptableObject
{
    public int enemiesRanged;
    public int enemiesMelee;
    public int xtiles;
    public int ytiles;
}
