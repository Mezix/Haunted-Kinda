using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MenuSceneSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public bool settingsRaised;
    public static float masterVolume = 1;
    public static float musicVolume = 1;
    public static float SFXVolume = 1;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider SFXVolumeSlider;
    public Text DifficultyText;
    public Slider DifficultySlider;
    public static int _difficulty;
    public Toggle tutorial;

    public static bool holdingSlider;
   
    private void Start()
    {
        _difficulty = 2;
        SetDifficulty(_difficulty);
    }
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            holdingSlider = false;
        }
    }
    public void SetMasterVolume(float Volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Volume) * 20);
        masterVolume = Volume;
        masterVolumeSlider.value = Volume;
        holdingSlider = true;
    }
    public void SetMusicVolume(float Volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Volume) * 20);
        musicVolume = Volume;
    }
    public void SetSFXVolume(float Volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Volume) * 20);
        SFXVolume = Volume;
    }
    public void SetDifficulty(float Difficulty)
    {
        _difficulty = Mathf.RoundToInt(Difficulty);
        if(_difficulty == 0)
        {
            DifficultyText.text = "= Peaceful";
        }
        else if (_difficulty == 1)
        {
            DifficultyText.text = "= Easy";
        }
        else if (_difficulty == 2)
        {
            DifficultyText.text = "= Medium";
        }
        else if (_difficulty == 3)
        {
            DifficultyText.text = "= Hard";
        }
        holdingSlider = true;
        DifficultySlider.value = _difficulty;
    }

    private void Awake()
    {
        masterVolume = 1;
        musicVolume = 1;
        SFXVolume = 1;
        SetMasterVolume(masterVolume);
        MenuSceneManager.playTutorial = tutorial.isOn;
        holdingSlider = false;
    }
    public void Raise()
    {
        StartCoroutine(RaiseGrave());
    }
    public void Lower()
    {
        StartCoroutine(LowerGrave());
    }
    private IEnumerator RaiseGrave()
    {
        StopCoroutine(LowerGrave());

        settingsRaised = false;
        while (transform.position.y <= 0.43f)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0.45f, transform.position.z), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        settingsRaised = true;
        transform.position = new Vector3(transform.position.x, 0.45f, transform.position.z);
    }
    private IEnumerator LowerGrave()
    {
        StopCoroutine(RaiseGrave());
        settingsRaised = true;
        while (transform.position.y >= -0.68f)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, -0.7f, transform.position.z), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        settingsRaised = false;
        transform.position = new Vector3(transform.position.x, -0.7f, transform.position.z);
    }
}
