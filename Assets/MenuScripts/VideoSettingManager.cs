using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class VideoSettingManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown ResolutionDropdown;
    [SerializeField] private Toggle FullscreenToggle;

    private Resolution[] resolutions;
    private List<Resolution> availableResolutions = new List<Resolution>();

    private const string ResolutionWidthKey = "ResolutionWidth";
    private const string ResolutionHeighKey = "ResolutionHeigh";
    private const string FullscreenKey = "Fullscreen";

    private void Start()
    {
        GetResolutions();
        LoadSettings();

        ResolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        FullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
    }

    private void ChangeFullscreen(bool isFullscreen)
    {
        ApplySettings(ResolutionDropdown.value, isFullscreen);
    }

    private void ChangeResolution(int index)
    {
        ApplySettings(index, FullscreenToggle.isOn);
        
    }

    //Metodo para recuperar los ajustes guardados
    private void LoadSettings()
    {
        int savedWidth = PlayerPrefs.GetInt(
            ResolutionWidthKey, Screen.width);

        int savedHeight = PlayerPrefs.GetInt(
            ResolutionHeighKey, Screen.height);

        bool savedFullscreen = PlayerPrefs.GetInt(
            FullscreenKey, 1) == 1;

        // Buscar la resolución guardada
        int resolutionIndex = availableResolutions.FindIndex(
            r => r.width == savedWidth && r.height == savedHeight
        );

        // Si no existe, utilizar la resolución actual
        if (resolutionIndex == -1)
        {
            resolutionIndex = availableResolutions.FindIndex(
                r => r.width == Screen.width && r.height == Screen.height
            );
        }

        if (resolutionIndex == -1)
            resolutionIndex = 0;

        ResolutionDropdown.SetValueWithoutNotify(resolutionIndex);
        FullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);

        ApplySettings(resolutionIndex, savedFullscreen);
    }

    //Metodo para obtener las resoluciones que permite el monitor
    private void GetResolutions()
    {
        resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();
        availableResolutions.Clear();

        List<string> options = new List<string>();
        HashSet<string> addedResolutions = new HashSet<string>();

        foreach (Resolution resolution in resolutions)
        {

            string resolutionName = resolution.width + " x " + resolution.height;

            if (addedResolutions.Add(resolutionName))
            {
                options.Add(resolutionName);
                availableResolutions.Add(resolution);
            }

            ResolutionDropdown.AddOptions(options);

        }
    }

    private void ApplySettings(int resolutionIndex, bool isFullscreen)
    {
        if (availableResolutions.Count == 0)
            return;

        Resolution resolution = availableResolutions[resolutionIndex];

        // Aplicar resolución y modo de pantalla
        FullScreenMode mode = isFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            mode
        );

        // Guardar ajustes
        PlayerPrefs.SetInt(ResolutionWidthKey, resolution.width);
        PlayerPrefs.SetInt(ResolutionHeighKey, resolution.height);
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);

        PlayerPrefs.Save();
    }
}
