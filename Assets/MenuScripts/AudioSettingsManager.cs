using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [SerializeField] public Slider VolumeSFXSlider;
    [SerializeField] public Slider VolumeGeneralSlider;

    float savedVolumeGeneral;
    float savedVolumeSFX;

    private const string volumeKeySFX = "SFXVolume";
    private const string volumeKeyGeneral = "GeneralVolume";

    private AudioSource AudioSource;

    public static AudioSettingsManager Instance { get; private set; }


    private void Start()
    {
        GetVolume();
        SetVolume();

        VolumeGeneralSlider.onValueChanged.AddListener(ChangeGeneralVolume);
        VolumeSFXSlider.onValueChanged.AddListener(ChangeSFXVolume);
    }

    private void ChangeSFXVolume(float value)
    {
        savedVolumeSFX = value;

        PlayerPrefs.SetFloat(volumeKeySFX, savedVolumeSFX);
        PlayerPrefs.Save();
    }

    private void ChangeGeneralVolume(float value)
    {
        savedVolumeGeneral = value;

        AudioListener.volume = savedVolumeGeneral;

        PlayerPrefs.SetFloat(volumeKeyGeneral, savedVolumeGeneral);
        PlayerPrefs.Save();
    }
        
    private void SetVolume()
    {
        AudioListener.volume = savedVolumeGeneral;

        VolumeGeneralSlider.value = savedVolumeGeneral;
        VolumeSFXSlider.value = savedVolumeSFX;
    }

    private void GetVolume()
    {

        savedVolumeGeneral = PlayerPrefs.GetFloat(volumeKeyGeneral, 1f);
        savedVolumeSFX = PlayerPrefs.GetFloat(volumeKeySFX, 1f);

    }



}
