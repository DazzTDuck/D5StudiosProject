using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.PlaySound("ButtonClick");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.PlaySound("ButtonHover");
    }

}
