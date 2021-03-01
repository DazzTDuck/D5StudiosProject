using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateGameTimer : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;

    GameInfo gameInfo;

    private void Update()
    {
        if(!gameInfo) { gameInfo = FindObjectOfType<GameInfo>(); }

        if (gameInfo) 
        {
            TimeSpan time = TimeSpan.FromSeconds(gameInfo.GetGameTimeLeft());
            timerText.text = time.ToString(@"m\:ss");
        }       
    }
}
