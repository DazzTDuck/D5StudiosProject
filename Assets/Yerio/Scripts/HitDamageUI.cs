using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bolt;
using TMPro;

public class HitDamageUI : GlobalEventListener
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

    public override void OnEvent(DamageRequest evnt)
    {
        if (evnt.EntityShooter == entityOwner && evnt.ShowDamage && evnt.OverallDamage > 0)
        {
            ShowDamageDone(evnt.OverallDamage);
            return;
        }

        if (evnt.EntityShooter == entityOwner && evnt.ShowDamage)
        {
            ShowDamageDone(evnt.Damage);
        }
    }
}
