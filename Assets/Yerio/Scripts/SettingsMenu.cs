using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public GameObject startMenu;
    public GameObject settingsMenu;

    public AudioMixer masterMixer;   

    public TMP_Dropdown resolutionDropdown;

    public Slider master;
    public Slider sfx;
    public Slider music;

    public Slider fov;
    public TMP_Text fovValueText;

    public Toggle fullscreenToggle;

    public TMP_Dropdown qualityDropdown;

    //values
    public static float masterVolume = 1f;
    public static float sfxVolume = 1f;
    public static float musicVolume = 1f;
    public static float fovValue = 65f;

    public static bool fullscreen = false;

    public Resolution[] resolutions;
    public static int currentResIndex;

    public static int qualityIndexSave = 2;


    private void Start()
    {
        GetResolutions();

        SetMasterVolume(masterVolume);
        master.value = masterVolume;

        SetSFXVolume(sfxVolume);
        sfx.value = sfxVolume;

        SetMusicVolume(musicVolume);
        music.value = musicVolume;

        SetQuality(qualityIndexSave);
        qualityDropdown.value = qualityIndexSave;

        SetResolution(currentResIndex);
        resolutionDropdown.value = currentResIndex;

        SetFullscreen(fullscreen);
        fullscreenToggle.isOn = fullscreen;

        UpdateFOVNumber(fovValue);
        fov.value = fovValue;
    }

        public void GetResolutions()
    {
        //get and set the resolutions for the dropdown
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            if (!options.Contains(option))
            {
                options.Add(option);               
            }       

            if (resolutions[i].width == Screen.currentResolution.width
                && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resIndex)
    {
        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        currentResIndex = resIndex;
    }

    //control the different volume sliders
    public void SetMasterVolume(float volume)
    {
        masterMixer.SetFloat("MasterValue", Mathf.Log10(volume) * 20);
        masterVolume = volume;
    }
    public void SetMusicVolume(float volume)
    {
        masterMixer.SetFloat("MusicValue", Mathf.Log10(volume) * 20);
        musicVolume = volume;
    }
    public void SetSFXVolume(float volume)
    {
        masterMixer.SetFloat("SFXValue", Mathf.Log10(volume) * 20);
        sfxVolume = volume;
    }

    //set the quality of the game
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        qualityIndexSave = qualityIndex;
    }

    //set the fullscreen of the game
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        fullscreen = isFullscreen;
    }

    public void UpdateFOVNumber(float value)
    {
        fovValueText.text = value.ToString();
        fovValue = value;
        Camera.main.GetComponent<Camera>().fieldOfView = value;
    }

    public void SettingsToMenuTrigger()
    {
        StartCoroutine("SettingsToMenu");
    }

    public void MenuToSettingsTrigger()
    {
        StartCoroutine("MenuToSettings");
    }

    public void LoadMainMenu()
    {
        StartCoroutine("LoadingSceneMenu");
    }

    IEnumerator LoadingSceneMenu()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(0);
    }
}
