using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Healthbar : MonoBehaviour
{
    [SerializeField] Image healthbarImage;
    [SerializeField] TMP_Text healthText;

    public void UpdateHealthbar(float healthPercentage, int maxHealth, int currentHealth)
    {
        healthText.text = $"{currentHealth} | {maxHealth}";
        healthbarImage.fillAmount = healthPercentage / 100;
    }
}
