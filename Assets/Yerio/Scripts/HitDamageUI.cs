using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bolt;
using TMPro;

public class HitDamageUI : MonoBehaviour
{
    [SerializeField] RectTransform canvas;
    [SerializeField] GameObject hitTextPrefab;
    [SerializeField] BoltEntity entityOwner;

    public void ShowDamageDone(float damage)
    {
        var text = Instantiate(hitTextPrefab, canvas);
        text.GetComponent<TMP_Text>().text = $"-{damage}";
        Destroy(text, 0.45f);
    }

    public void SendDamage(float damage, bool showDamage, float overallDamage = 0)
    {
        if (showDamage && overallDamage > 0)
        {
            ShowDamageDone(overallDamage);
            return;
        }

        if (showDamage)
        {
            ShowDamageDone(damage);
        }
    }
}
