using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FadeText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Animator fadeAnimator;

    bool isHovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    private void Update()
    {
        if (isHovering) 
        {
            fadeAnimator.ResetTrigger("FadeOut");
            fadeAnimator.SetTrigger("FadeIn"); 
        } 
        else 
        {
            fadeAnimator.ResetTrigger("FadeIn");
            fadeAnimator.SetTrigger("FadeOut"); 
        }
    }
}
