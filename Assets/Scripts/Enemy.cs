using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using UnityEditor;

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
            switch (myData.imEnemy)
            {
                case EnemyData.enemyClass.melee:
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
                    break;
                case EnemyData.enemyClass.ranged:
                    Vector2 pursuitShort = (Vector2)transform.position - (Vector2)player.transform.position;
                    if (pursuitShort.magnitude < vision)
                    {
                        Debug.Log(transform.gameObject.name+": I see player");
                    }
                    break;
            }
        }
        else if (myData.imEnemy.Equals(EnemyData.enemyClass.melee))
        {
            //gofothemonay
        }
    }
    Vector2 Simplepursuit(Vector2 playerPos, Vector2 currentEnemy)
    {
        Vector2 pursuitShort = currentEnemy - playerPos;
        Vector2 xpursuit = new Vector2(pursuitShort.x, 0).normalized;
        Vector2 ypursuit = new Vector2(0, pursuitShort.y).normalized;
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
        if(GameManager._Instance.GetEnemyAtPosition(route) != null)
        {
            if (route.Equals(Vector2.zero))
            {
                int aroundRand = Random.Range(0, 2) == 0? -1 : 1;                
                route = route == xpursuit ? ypursuit + new Vector2(0,aroundRand) : xpursuit + new Vector2(aroundRand, 0);
            }
            else
            {
                route = route == xpursuit? ypursuit: xpursuit;
            }
        }
        return route;
    }

    
}
