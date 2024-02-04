using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private GameObject GameOverMenuUI;
    [SerializeField] private GameObject MainSceneUI;
    [SerializeField] private GameManager GameManager;

    public void Restart(){
        GameManager.DestroyLevel();
        GameManager.InitGameManager();
        GameOverMenuUI.SetActive(false);
    }
}
