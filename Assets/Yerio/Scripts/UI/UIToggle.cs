using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
/// <summary>
/// UnityEvent callback for when a toggle is toggled.
/// </summary>
public class ToggleEvent : UnityEvent<bool>
{ }

[ExecuteInEditMode]
public class UIToggle : MonoBehaviour
{
    [SerializeField] GameObject onText;
    [SerializeField] GameObject offText;

    [Space, SerializeField] Button arrowL;
    [SerializeField] Button arrowR;

    [Space, SerializeField] bool isOn = true;

    [SerializeField] ToggleEvent onValueChanged;

    // Start is called before the first frame update
    void Awake()
    {
        arrowL.onClick.AddListener(() => { isOn = false; onValueChanged.Invoke(isOn); });
        arrowR.onClick.AddListener(() => { isOn = true; onValueChanged.Invoke(isOn); });
    }

    // Update is called once per frame
    void Update()
    {
        if(isOn) { onText.SetActive(true); offText.SetActive(false); } else { onText.SetActive(false); offText.SetActive(true); }
    }

    public bool GetIfOn() { return isOn; }
    public void SetValue(bool value) { isOn = value; }
}
