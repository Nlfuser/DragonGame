using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopMenu : MonoBehaviour
{
    [SerializeField] private GameObject GameOverMenuUI;
    [SerializeField] private GameObject MainSceneUI;
    [SerializeField] private GameManager GameManager;

    public void BoughtAttack(){
        print("BoughtAttack");
        CloseShop();
    }

    public void BoughtMaxHealth(){
        print("BoughtMaxHealth");
        CloseShop();
    }

    public void BoughtHealthRefill(){
        print("BoughtHealthRefill");        
        CloseShop();
    }

    public void BoughtFlight(){
        print("BoughtFlight");        
        CloseShop();
    }

    public void CloseShop(){
        print("CloseShop");
    }
}
