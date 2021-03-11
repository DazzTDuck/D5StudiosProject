using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RespawnHandler : MonoBehaviour
{
    [SerializeField] float respawnTime;
    [SerializeField] Timer respawnTimer;
    [Space]
    [SerializeField] TMP_Text respawnText;

    BoltEntity entityToRespawn;

    public void StartRespawnTimer(GameObject canvasToDisable)
    {
        respawnTimer.SetTimer(respawnTime, () => { SendDestroyRequest(); canvasToDisable.SetActive(false);});
    }

    public void SetEntity(BoltEntity entity) { entityToRespawn = entity; }

    public void SendDestroyRequest()
    {
        var request = DestroyRequest.Create();
        request.Entity = entityToRespawn;
        request.KillTrigger = false;
        request.IsPlayer = true;
        request.Send();
    }

    private void Update()
    {
        if (respawnTimer.IsTimerActive())
            respawnText.text = $"Respawning in {(int)respawnTimer.GetTimerValue()}..";
    }
}
