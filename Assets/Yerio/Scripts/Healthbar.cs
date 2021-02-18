using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Healthbar : MonoBehaviour
{
    [SerializeField] Canvas infoPlayerCanvas;
    [SerializeField] Image healthbarImageAboveHead;
    [SerializeField] Image healthbarImage;
    [SerializeField] TMP_Text healthText;

    public void UpdateHealthbar(float healthPercentage, int maxHealth, int currentHealth)
    {
        healthText.text = $"{currentHealth} | {maxHealth}";
        var fillAmount = Mathf.Lerp(healthbarImage.fillAmount, healthPercentage / 100, 20 * BoltNetwork.FrameDeltaTime);
        healthbarImage.fillAmount = fillAmount;
        healthbarImageAboveHead.fillAmount = fillAmount;
        //healthbarImage.fillAmount = healthPercentage / 100;
    }

    public void CanvasLookAt(Transform transform)
    {
        infoPlayerCanvas.transform.LookAt(transform);
    }
}
