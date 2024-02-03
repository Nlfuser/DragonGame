
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using UnityEditor;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : Character
{
    public EnemyData myData;
    private int vision;
    private int range;
    private bool attackCharge;
    private float arrowDuration = 0.07f;
    // Start is called before the first frame update
    void Start()
    {
        _health = myData.enemyhealth;
        _attack = myData.enemyattack;
        vision = myData.vision;
        range = myData.range;
        attackCharge = false;
    }
    // Update is called once per frame
    public void Behave(Player player)
    {
#pragma warning disable CS0642
        if (EvadeLava(player.transform.position)) ;
        else if (Vector2.Distance(player.transform.position, transform.position) < vision)
        {
            switch (myData.imEnemy)
            {
                case EnemyData.enemyClass.melee:
                    Simplepursuit(player.transform.position);
                    break;
                case EnemyData.enemyClass.ranged:
                    RangedPursuit(player.transform.position);
                    break;
            }
        }
        else if (myData.imEnemy.Equals(EnemyData.enemyClass.melee))
        {
            //gofothemonay
        }
    }
    bool EvadeLava(Vector2 playerPos)
    {
        foreach (Vector2 surround in GameManager._Instance.cardinals)
        {
            Vector2 target = (Vector2)transform.position - surround;
            Vector2 escape = (Vector2)transform.position + surround;
            if (GameManager._Instance.GetLavaAtPosition(target) != null)
            {
                var possibleNode = GameManager._Instance.GetNodeAtPosition((Vector2)transform.position + surround);
                if (possibleNode != null)
                {
                    if (escape == playerPos)
                    {
                        RangedPursuit(playerPos);
                    }
                    else if (GameManager._Instance.GetEnemyAtPosition(escape) == null)
                    {
                        transform.position = escape;
                    }
                }
                attackCharge = false;
                return true;
            }
            /*if(myData.imEnemy.Equals(EnemyData.enemyClass.ranged) & GameManager._Instance.GetLavaPoolAtPosition(target) != null)
            {

            }*/
        }
        return false;
    }
    // float duration = 0.07f;
    IEnumerator Arrow(float duration, Vector2 targetPos)
    {
        float t = 0;
        GameObject arrowClone = Instantiate(GameManager._Instance.ArrowPrefab, transform.position, transform.rotation);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            arrowClone.transform.position = Vector3.Lerp(transform.position, targetPos, t);
            yield return null;
        }
        Player player = GameManager._Instance.player;
        if (arrowClone.transform.position == player.transform.position)
        {
            GameManager._Instance.PlayerHurt(_attack);
            Destroy(arrowClone);
        }
    }
    void RangedPursuit(Vector2 playerPos)
    {
        Vector2 pursuitShort = (Vector2)transform.position - (Vector2)playerPos;
        if (pursuitShort.magnitude < range)
        {
            if (!attackCharge)
            {
                attackCharge = true;
            }
            else
            {
                StartCoroutine(Arrow(arrowDuration, playerPos));
                attackCharge = false;
            }
        }
        else
        {
            Simplepursuit(playerPos);
        }
    }
    void Simplepursuit(Vector2 player)
    {
        Vector2 possibleLocation = (Vector2)transform.position - SimplePursuitPosition(player, transform.position);
        var possibleNode = GameManager._Instance.GetNodeAtPosition(possibleLocation);
        if (possibleNode != null)
        {
            if (possibleLocation == player)
            {
                GameManager._Instance.Fight(this);
            }
            if (GameManager._Instance.GetEnemyAtPosition(possibleLocation) == null && GameManager._Instance.GetLavaAtPosition(possibleLocation) == null)
            {
                transform.position = possibleLocation;
            }
        }
    }
    Vector2 SimplePursuitPosition(Vector2 playerPos, Vector2 currentEnemy)
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
        if (GameManager._Instance.GetEnemyAtPosition(route) != null)
        {
            if (route.Equals(Vector2.zero))
            {
                int aroundRand = Random.Range(0, 2) == 0 ? -1 : 1;
                route = route == xpursuit ? ypursuit + new Vector2(0, aroundRand) : xpursuit + new Vector2(aroundRand, 0);
            }
            else
            {
                route = route == xpursuit ? ypursuit : xpursuit;
            }
        }
        return route;
    }
}

