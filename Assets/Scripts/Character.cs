using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] public int _health;
    [SerializeField] public int _attack;
    public Vector2 Pos => transform.position;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Takedmg(int dmg){
        _health -= dmg;
        if(_health <= 0) Destroy(gameObject);
    }
}
