using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Enemy : Character
{
    public EnemyData myData;
    private int vision;
    // Start is called before the first frame update
    void Start()
    {
        _health = myData.enemyhealth;
        _attack = myData.enemyattack;
        vision = myData.vision;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Behave(Player player)
    {
        if (Vector2.Distance(player.transform.position, transform.position) < vision)
        {
            Vector2 possibleLocation = (Vector2)transform.position - Simplepursuit(player.transform.position, transform.position);
            var possibleNode = GameManager._Instance.GetNodeAtPosition(possibleLocation);
            if (possibleNode != null)
            {
                if (possibleLocation == (Vector2)player.transform.position)
                {
                    GameManager._Instance.Fight(this);
                }
                if (GameManager._Instance.GetEnemyAtPosition(possibleLocation) == null && GameManager._Instance.GetLavaAtPosition(possibleLocation) == null)
                {
                    transform.position = possibleLocation;
                }
            }
        }
        else
        {
            //gofothemonay
        }
    }
    Vector2 Simplepursuit(Vector2 player, Vector2 currentEnemy)
    {
        Vector2 pursuitShort = currentEnemy - player;
        Vector2 xpursuit = new Vector2(pursuitShort.x, 0);
        Vector2 ypursuit = new Vector2(0, pursuitShort.y);
        Vector2 route = Vector2.zero;
        if (Math.Abs(pursuitShort.x) == Math.Abs(pursuitShort.y))
        {
            if (Random.Range(0, 2) == 0) route = xpursuit;
            else { route = ypursuit; }
        }
        else
        {
            route = Math.Abs(pursuitShort.x) > Math.Abs(pursuitShort.y) ? xpursuit : ypursuit;
        }
        return route.normalized;
    }

    
}
