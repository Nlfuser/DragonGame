using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopMenu : MonoBehaviour
{

    [SerializeField] private GameManager GameManager;
    [SerializeField] private GameObject ShopMenuUI;
    [SerializeField] private int AttackCost;
    [SerializeField] private int HealthCost;
    [SerializeField] private int RefillCost;
    [SerializeField] private int FlyCost;

    public void BoughtAttack(){
        if(GameManager.player.coinCount >= AttackCost){
        print("BoughtAttack");
            GameManager.PlayerCoinGain(-AttackCost);
            GameManager.player._attack += 1;
            GameManager.AttackText.SetText(string.Format("{0}", GameManager.player._attack));

            CloseShop();
        }

    }

    public void BoughtMaxHealth(){
        if(GameManager.player.coinCount >= HealthCost){
        print("BoughtMaxHealth");
            GameManager.PlayerCoinGain(-HealthCost);
            GameManager.player._maxhealth += 5;
            GameManager.player._health += 5;
            GameManager.HealthText.SetText(string.Format("{0}", GameManager.player._health) + " / " + string.Format("{0}", GameManager.player._maxhealth));

            CloseShop();
        }
    }

    public void BoughtHealthRefill(){
        if(GameManager.player.coinCount >= RefillCost){
        print("BoughtRefill");
            GameManager.PlayerCoinGain(-RefillCost);
            GameManager.player._health = GameManager.player._maxhealth;
            GameManager.HealthText.SetText(string.Format("{0}", GameManager.player._health) + " / " + string.Format("{0}", GameManager.player._maxhealth));
            CloseShop();
        }
    }

    public void BoughtFlight(){
        if(GameManager.player.coinCount >= FlyCost){
        print("BoughtFlight");
            GameManager.PlayerCoinGain(-FlyCost);
            GameManager.player.canFly = true;
            CloseShop();
        }
    }

    public void CloseShop(){

        GameManager.ChangeState(GameState.GenerateLevel);
        print("CloseShop");        
        ShopMenuUI.SetActive(false);

    }
}
