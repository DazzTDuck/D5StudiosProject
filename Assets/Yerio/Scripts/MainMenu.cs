using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using TMPro;


public class MainMenu : GlobalEventListener
{
    [SerializeField] Button hostButton;
    [SerializeField] GameObject findGamePanel;
    [SerializeField] TMP_Text currentSessionsText;
    [SerializeField] GameObject sessionButtonPrefab;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //host game
    public void StartServer()
    {
        BoltLauncher.StartServer();
    }

    public override void BoltStartDone()
    {
        base.BoltStartDone();

        BoltMatchmaking.CreateSession(sessionID: "Epic Game", sceneToLoad: "Yerio");
    }

    //join game button
    public void StartClient()
    {
        BoltLauncher.StartClient();

        //enable Panel & disable host button
        findGamePanel.SetActive(true);
        hostButton.interactable = false;
    }

    public void BackToMainMenu()
    {
        //disable Panel & enable host button
        BoltLauncher.Shutdown();

        findGamePanel.SetActive(false);
        hostButton.interactable = true;
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        base.SessionListUpdated(sessionList);

        currentSessionsText.text = $"{sessionList.Count} Games Found";

        foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value as UdpSession;

            if (photonSession.Source == UdpSessionSource.Photon)
            {
                var button = Instantiate(sessionButtonPrefab, findGamePanel.transform);
                button.GetComponent<Button>().onClick.AddListener(() => JoinSession(photonSession));
            }
        }
    }

    public void JoinSession(UdpSession session)
    {
        BoltMatchmaking.JoinSession(session);
    }
}
