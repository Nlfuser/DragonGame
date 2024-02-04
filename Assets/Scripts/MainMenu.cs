using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuUI;
    [SerializeField] private GameObject MainSceneUI;
    [SerializeField] private GameManager GameManager;
    public void PlayGame(){
        MainMenuUI.SetActive(false);
        MainSceneUI.SetActive(true);
        GameManager.InitGameManager();
    }
}
