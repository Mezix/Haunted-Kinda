using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MenuSceneSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public bool settingsRaised;
    public static float volume;
    public Slider volumeSlider;
    public Text DifficultyText;
    public static int _difficulty = 2;
    public Toggle tutorial;

    
    public void SetVolume(float Volume)
    {
        audioMixer.SetFloat("MasterVolume", Volume);
        volume = Volume;
        volumeSlider.value = Volume;
        DifficultyText.text = "= Medium";
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
    }

    private void Awake()
    {
        SetVolume(volume);
        MenuSceneManager.playTutorial = tutorial.isOn;
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
