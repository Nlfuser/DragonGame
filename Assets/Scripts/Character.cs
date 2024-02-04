using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] public int _maxhealth;
    [SerializeField] public int _health;
    [SerializeField] public int _attack;
    public Vector2 Pos => transform.position;
    public bool Takedmg(int dmg){
        _health -= dmg;
        if (_health <= 0)
        {
            Destroy(gameObject);
            return true;
        }
        else { return false; }
    }
}
