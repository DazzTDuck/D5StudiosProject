using System.Collections;
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

    private void Start()
    {
        button.onClick.AddListener(() => { SetName(inputField.text); });
    }

    public void SetName(string name)
    {
        state.Name = name.Length > 2 ? name : "Player";
        nameText.text = state.Name;
        nameSet = true;
        canvas.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

}
