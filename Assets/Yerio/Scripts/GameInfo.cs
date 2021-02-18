using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class GameInfo : Bolt.EntityBehaviour<IGameInfoState>
{
    float startingTime = 300;

    [SerializeField] float timer;
    int team1Kills;
    int team2Kills;

    PlayerController player;
    NetworkCallbacks networkCallbacks;
    bool isHost;

    public override void Attached()
    {
        networkCallbacks = FindObjectOfType<NetworkCallbacks>();
        StartCoroutine(StartDelay());
    }

    public void SetPlayer(PlayerController playerController)
    {
        player = playerController;
    }

    public float GetGameTimeLeft()
    {
        return timer;
    }

    void Update()
    {
        if (state.GameTimeLeft > 0 && state.GameStarted)
        {
            state.GameTimeLeft -= Time.deltaTime;
        }
    }

    public void SetGameStarted()
    {
        if(!state.GameStarted) { state.GameStarted = true; }
    }

    void TimerCallback()
    {
        timer = state.GameTimeLeft;
    }

    IEnumerator StartDelay()
    {
        yield return new WaitUntil(() => player);

        isHost = player.GetIfHost();

        if (isHost)
        {
            if (entity.IsOwner)
            {
                state.GameTimeLeft = startingTime;
                timer = startingTime;
                state.AddCallback("GameTimeLeft", TimerCallback);
                state.Team1Kills = team1Kills;
                state.Team2Kills = team2Kills;
            }           
        }
        else
        {
            BoltNetwork.Destroy(gameObject);
        }
    }
}
