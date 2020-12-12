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

    public GameObject dirtPrefab;
    public GameObject dirt;
    public Transform dirtPos;
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
        masterVolume = 0.5f;
        musicVolume = 1;
        SFXVolume = 1;
        SetMasterVolume(masterVolume);
        MenuSceneManager.playTutorial = tutorial.isOn;
        holdingSlider = false;
    }
    public void Raise()
    {
        StopAllCoroutines();
        StartCoroutine(RaiseGrave());
    }
    public void Lower()
    {
        StopAllCoroutines();
        StartCoroutine(LowerGrave());
    }
    private IEnumerator RaiseGrave()
    {
        Destroy(dirt);
        dirt = Instantiate(dirtPrefab, dirtPos);
        settingsRaised = true;
        while (transform.position.y <= 0.43f)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0.45f, transform.position.z), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        transform.position = new Vector3(transform.position.x, 0.45f, transform.position.z);
        
    }
    private IEnumerator LowerGrave()
    {
        Destroy(dirt);
        dirt = Instantiate(dirtPrefab, dirtPos);
        settingsRaised = false;
        while (transform.position.y >= -1.1f)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, -1.15f, transform.position.z), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        transform.position = new Vector3(transform.position.x, -1.15f, transform.position.z);
        
    }
}
