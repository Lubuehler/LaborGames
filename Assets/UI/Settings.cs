using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    private void Start()
    {
        InitializeSettings();
    }


    public void OnScreenModeChanged(int fullScreenMode)
    {
        DataController.Instance.SetScreenMode(fullScreenMode);
    }

    public void OnResolutionChanged(int resolutionIndex)
    {
        DataController.Instance.SetResolution(resolutionIndex);
    }

    public void OnBackClick()
    {
        UIController.Instance.ShowUIElement(UIElement.Main);
    }

    private void OnDisable()
    {
        // Save settings
        PlayerPrefs.SetInt("Resolution", QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("ScreenMode", screenModeDropdown.value);
    }

    public void InitializeSettings()
    {
        // Load saved settings or initialize to default values
        resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", QualitySettings.GetQualityLevel());
        screenModeDropdown.value = PlayerPrefs.GetInt("ScreenMode", (int)Screen.fullScreenMode);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
    }
}
