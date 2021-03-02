using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowSliderPerc : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text percText;

    public void ShowPercentage(float sliderValue)
    {
        //(value - minY)/(maxX - minY) * 100. calculate percentage
        float percent = (sliderValue - slider.minValue) / (slider.maxValue - slider.minValue) * 100;
        percText.text = percent.ToString("0") + "%";
    }
}
