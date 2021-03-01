using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
/// <summary>
/// UnityEvent callback for when a toggle is toggled.
/// </summary>
public class SwitchEvent : UnityEvent<int>
{ }

public class UISwitch : MonoBehaviour
{
    [SerializeField] TMP_Text selectedText;

    [Space, SerializeField] List<string> options = new List<string>();
    [Space, SerializeField] SwitchEvent onValueChanged;

    int value;

    public void RefreshShownValue() { selectedText.text = options[value]; }
    public void ClearOptions() { options.Clear(); }
    public void AddOptions(List<string> options) { ClearOptions(); this.options.AddRange(options); }

    public void SetValue(int value) { this.value = value; onValueChanged.Invoke(this.value); }

    public void IncreaseValue() 
    { 
        if(value == options.Count - 1) { value = 0; } else { value++; }    

        onValueChanged.Invoke(value);
        RefreshShownValue();
    }
    public void DecreaseValue() 
    {
        if (value == 0) { value = options.Count - 1; } else { value--; }

        onValueChanged.Invoke(value);
        RefreshShownValue();
    }
}
