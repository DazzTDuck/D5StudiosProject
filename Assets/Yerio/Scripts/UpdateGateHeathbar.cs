using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateGateHeathbar : MonoBehaviour
{
    [SerializeField] Image healthbar;

    GateHealth gateHealth;

    // Update is called once per frame
    void Update()
    {
        if (!gateHealth)
        {
            gateHealth = FindObjectOfType<GateHealth>();
        }

        if (gateHealth)
        {
            healthbar.fillAmount = Mathf.Lerp(healthbar.fillAmount, gateHealth.GetCurrentHealthPercentage() / 100, 20 * BoltNetwork.FrameDeltaTime);
        }
    }
}
