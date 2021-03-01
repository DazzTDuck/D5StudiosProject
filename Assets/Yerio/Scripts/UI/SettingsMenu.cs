using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class SettingsMenu : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] AudioMixer masterMixer;

    [Header("Dropdowns")]
    [SerializeField] TMP_Dropdown resolutionSwitch;
    [SerializeField] TMP_Dropdown qualityDropdown;

    [Header("Sliders")]
    [SerializeField] Slider master;
    [SerializeField] Slider sfx;

    [Header("Sensitivity")]
    [SerializeField] PlayerCamera[] cams;
    [SerializeField] Slider sensitivity;
    [SerializeField] float sensDevider = 5f;

    [Header("Toggles")]
    [SerializeField] UIToggle fullscreenToggle;
    [SerializeField] UIToggle motionBlurToggle;
    [SerializeField] UIToggle bloomToggle;

    [Header("PostFX Profile")]
    [SerializeField] VolumeProfile postFx;

    MotionBlur motionBlur;
    Bloom bloom;

    Resolution[] resolutions;

    List<Resolution> resolutionsList = new List<Resolution>();

    Animator settingsAnimator;

    private void Start()
    {
        LoadSettings();
        //settingsAnimator = GetComponentInChildren<Animator>();
    }

    void LoadSettings()
    {
        if (resolutionSwitch)
            GetResolutions();

        if (postFx)
        {
            postFx.TryGet(out bloom);
            postFx.TryGet(out motionBlur);
        }

        if (masterMixer)
        {
            SetMasterVolume(PlayerPrefs.GetFloat("masterVolume", 1f));
            if (master)
                master.value = PlayerPrefs.GetFloat("masterVolume", 1f);

            SetSFXVolume(PlayerPrefs.GetFloat("sfxVolume", 1f));
            if (sfx)
                sfx.value = PlayerPrefs.GetFloat("sfxVolume", 1f);
        }       
        
        SetSensitivity(PlayerPrefs.GetFloat("sensitivityValue", 5f));
        if (sensitivity)
            sensitivity.value = PlayerPrefs.GetFloat("sensitivityValue", 5f);


        if (qualityDropdown)
        {
            SetQuality(PlayerPrefs.GetInt("qualityIndex", 2));
            qualityDropdown.value = PlayerPrefs.GetInt("qualityIndex", 2); ;
            qualityDropdown.RefreshShownValue();
        }

        if (resolutionSwitch)
        {
            SetResolution(PlayerPrefs.GetInt("resIndex", resolutions.Length));
            resolutionSwitch.value = PlayerPrefs.GetInt("resIndex", resolutions.Length);
            resolutionSwitch.RefreshShownValue();
        }
        if (fullscreenToggle)
        {
            SetFullscreen(PlayerPrefs.GetInt("fullscreenBool", 1) != 0);
            fullscreenToggle.SetValue(PlayerPrefs.GetInt("fullscreenBool", 1) != 0);
        }
        if (bloomToggle)
        {
            SetBloom(PlayerPrefs.GetInt("bloomBool", 1) != 0);
            bloomToggle.SetValue(PlayerPrefs.GetInt("bloomBool", 1) != 0);
        }
        if (motionBlurToggle)
        {
            SetMotionBlur(PlayerPrefs.GetInt("motionBlurBool", 1) != 0);
            motionBlurToggle.SetValue(PlayerPrefs.GetInt("motionBlurBool", 1) != 0);
        }
    }

    public void ResetAllSettings()
    {
        PlayerPrefs.DeleteKey("masterVolume");
        PlayerPrefs.DeleteKey("sfxVolume");
        PlayerPrefs.DeleteKey("sensitivityValue");
        PlayerPrefs.DeleteKey("resIndex");
        PlayerPrefs.DeleteKey("qualityIndex");
        PlayerPrefs.DeleteKey("motionBlurBool");
        PlayerPrefs.DeleteKey("bloomBool");
        PlayerPrefs.DeleteKey("fullscreenBool");

        LoadSettings();
    }

    public void GetResolutions()
    {
        //get and set the resolutions for the dropdown
        resolutions = Screen.resolutions;

        resolutionSwitch.ClearOptions();
        resolutionsList.Clear();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            if (!options.Contains(option))
            {
                options.Add(option);
                resolutionsList.Add(resolutions[i]);
               // Debug.Log(option + " " + i);
            }
        }

        PlayerPrefs.SetInt("resIndex", resolutions.Length); //the last in the list
        PlayerPrefs.Save();
        resolutionSwitch.AddOptions(options);
        resolutionSwitch.value = PlayerPrefs.GetInt("resIndex");
        resolutionSwitch.RefreshShownValue();
    }

    public void SettingsTrigger(string triggerName)
    {
        settingsAnimator.SetTrigger(triggerName);
    }

    public void SetResolution(int resIndex)
    {
        Resolution res = resolutionsList[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        //Screen.SetResolution(1920, 1080, Screen.fullScreen);

        //Debug.Log(resIndex + " " + res.width + " " + res.height);

        PlayerPrefs.SetInt("resIndex", resIndex);
        PlayerPrefs.Save();
    }

    //set the quality of the game
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);

        PlayerPrefs.SetInt("qualityIndex", qualityIndex);
        PlayerPrefs.Save();
    }

    //control the different volume sliders
    public void SetMasterVolume(float volume)
    {
        masterMixer.SetFloat("MasterValue", volume);

        PlayerPrefs.SetFloat("masterVolume", volume);
        PlayerPrefs.Save();
    }
    public void SetSensitivity(float value)
    {
        //set Sens On Camera
        foreach (var cam in cams)
        {
            cam.SetSensitivity(value / sensDevider);
        }

        PlayerPrefs.SetFloat("sensitivityValue", value);
        PlayerPrefs.Save();
    }
    public void SetSFXVolume(float volume)
    {
        masterMixer.SetFloat("SFXValue", volume);

        PlayerPrefs.SetFloat("sfxVolume", volume);
        PlayerPrefs.Save();
    }

    //set the fullscreen of the game
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("fullscreenBool", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void SetBloom(bool isBloom)
    {
        bloom.active = isBloom;

        PlayerPrefs.SetInt("bloomBool", isBloom ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void SetMotionBlur(bool isMB)
    {
        motionBlur.active = isMB;

        PlayerPrefs.SetInt("motionBlurBool", isMB ? 1 : 0);
        PlayerPrefs.Save();
    }
}
