﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameAbovePlayer : Bolt.EntityBehaviour<IPlayerControllerState>
{
    [SerializeField] TMP_Text nameText;

    [SerializeField] Button button;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject canvas;

    [HideInInspector]
    public bool nameSet = false;

    string localName;

    public override void Attached()
    {
        state.Name = localName;
        state.AddCallback("Name", SetNameCallback);
    }

    private void Start()
    {
        button.onClick.AddListener(() => { SetName(inputField.text); });
    }

    public void SetNameCallback()
    {
        localName = state.Name;
        nameText.text = state.Name;
    }

    public void SetName(string name)
    {
        state.Name = name.Length > 2 ? name : "Player";
        nameText.text = state.Name;
        nameSet = true;
        state.SetDynamic("Name", state.Name);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        canvas.SetActive(false);
    }

}
