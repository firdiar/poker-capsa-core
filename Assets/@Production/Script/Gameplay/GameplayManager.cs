using Gtion.Plugin.DI;
using Pker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class GameplayManager : MonoBehaviour, IInjectCallback
{
    [GInject]
    PokerGameManager pokerManager;

    public bool IsDependencyReady { get;set; }


    public void OnDependencyReady()
    {
        pokerManager.OnPlayerWin.AddListener(OnPlayerWin);
        pokerManager.StartGame();
    }

    private async void OnPlayerWin(int arg0)
    {
        Debug.Log("Player Win! : "+arg0);
        await Task.Delay(3000);

        
        pokerManager.NextGame();
    }
}
