using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateKillCounter : MonoBehaviour
{
    [SerializeField] TMP_Text team1Counter;
    [SerializeField] TMP_Text team2Counter;

    GameInfo gameInfo;

    private void Update()
    {
        if (!gameInfo) { gameInfo = FindObjectOfType<GameInfo>(); }

        if (gameInfo)
        {
            team1Counter.text = gameInfo.state.Team1Kills.ToString();
            team2Counter.text = gameInfo.state.Team2Kills.ToString();
        }
    }
}
