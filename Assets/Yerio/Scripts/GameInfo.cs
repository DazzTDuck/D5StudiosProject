using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class GameInfo : Bolt.EntityBehaviour<IGameInfoState>
{
    float startingTime = 120;

    float timer;
    int team1Kills;
    int team2Kills;

    [SerializeField] PlayerController player;
    [SerializeField] bool isHost;

    public override void Attached()
    {
        StartCoroutine(StartDelay());
        StartCoroutine(DeleteIfNoPlayer());
    }

    public void SetPlayer(PlayerController playerController)
    {
        if (!player)
            player = playerController;
    }

    public float GetGameTimeLeft()
    {
        return state.GameTimeLeft;
    }

    public override void SimulateOwner()
    {
        if (state.GameTimeLeft > 0 && state.GameStarted && entity.IsOwner)
        {
            state.GameTimeLeft -= Time.deltaTime;
        }

        if(state.GameTimeLeft <= 0 && entity.IsOwner && state.GameStarted)
        {
            var request = RestartRequest.Create();
            request.Send();
        }
    }

    public void AddTeam1Kill()
    {
        if(entity.IsOwner)
            state.Team1Kills++;
    }
    public void AddTeam2Kill()
    {
        if (entity.IsOwner)
            state.Team2Kills++;
    }

    public void SetGameStarted()
    {
        if(!state.GameStarted) { state.GameStarted = true; }
    }

    void KillsCallback()
    {
        team1Kills =  state.Team1Kills;
        team2Kills =  state.Team2Kills;
    }

    void TimerCallback()
    {
        timer = state.GameTimeLeft;
    }

    IEnumerator StartDelay()
    {
        yield return new WaitUntil(() => player);

        yield return new WaitForSeconds(2f);

        isHost = player.GetIfHost();

        if (isHost)
        {
            if (entity.IsOwner)
            {
                state.GameTimeLeft = startingTime;
                timer = startingTime;
                state.Team1Kills = team1Kills;
                state.Team2Kills = team2Kills;
                state.AddCallback("GameTimeLeft", TimerCallback);
                state.AddCallback("Team1Kills", KillsCallback);
                state.AddCallback("Team2Kills", KillsCallback);
            }           
        }
        else
        {
            BoltNetwork.Destroy(gameObject);
        }
    }

    IEnumerator DeleteIfNoPlayer()
    {
        yield return new WaitForSeconds(20f);

        if (!player) { BoltNetwork.Destroy(gameObject); } else { StopCoroutine(nameof(DeleteIfNoPlayer)); }
    }
}
