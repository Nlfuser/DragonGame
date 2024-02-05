using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    [SerializeField] public bool canFly;

    public float coinCount;

    public void rogerUIUpdate()
    {
        GameManager._Instance.UIHealthUpdate();
        GameManager._Instance.UICoinUpdate();
        GameManager._Instance.UIAttackUpdate();
    }   
    public void GainGold(int amount){
        coinCount+=amount;
    }
}
