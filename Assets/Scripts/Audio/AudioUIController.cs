using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioUIController : MonoBehaviour
{
    // Apunta a los sliders del menú de pausa
    [Header("Sliders")]
    [SerializeField] Slider generalSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    // Variables para las VCAs del FMOD configuradas
    private FMOD.Studio.VCA generalVCA;
    private FMOD.Studio.VCA musicVCA;
    private FMOD.Studio.VCA sfxVCA;

    private void Start()
    {
        // Obtenemos las VCAs correspondientes
        generalVCA = FMODUnity.RuntimeManager.GetVCA("vca:/General");
        musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/Music");
        sfxVCA = FMODUnity.RuntimeManager.GetVCA("vca:/SFX");

        // Cargar valores guardados o usar los actuales de FMOD
        generalSlider.value = PlayerPrefs.GetFloat("GeneralVolume", GetVCAValue(generalVCA));
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", GetVCAValue(musicVCA));
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", GetVCAValue(sfxVCA));

        // Aplicar a FMOD
        generalVCA.setVolume(generalSlider.value);
        musicVCA.setVolume(musicSlider.value);
        sfxVCA.setVolume(sfxSlider.value);

        // Definimos los sliders para que ejecuten las funciones cuando cambian de valor
        generalSlider.onValueChanged.AddListener(OnGeneralAudioChanged);
        musicSlider.onValueChanged.AddListener(OnMusicAudioChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXAudioChanged);
    }

    private float GetVCAValue(FMOD.Studio.VCA vca)
    {
        vca.getVolume(out float volume);
        return volume;
    }

    // Se llama desde un slider, cambia el volumen de la VCA general según el valor introducido en el slider.
    public void OnGeneralAudioChanged(float value)
    {
        generalVCA.setVolume(value);
        PlayerPrefs.SetFloat("GeneralVolume", value);
    }

    // Se llama desde un slider, cambia el volumen de la VCA de música según el valor introducido en el slider.
    public void OnMusicAudioChanged(float value)
    {
        musicVCA.setVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    // Se llama desde un slider, cambia el volumen de la VCA de SFX según el valor introducido en el slider.
    public void OnSFXAudioChanged(float value)
    {
        sfxVCA.setVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}