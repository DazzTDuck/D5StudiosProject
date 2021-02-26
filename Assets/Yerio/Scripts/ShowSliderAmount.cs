using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class ShowSliderAmount : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    public void ShowValue(float amount)
    {
        text.text = amount.ToString("0.0");
    }
}
