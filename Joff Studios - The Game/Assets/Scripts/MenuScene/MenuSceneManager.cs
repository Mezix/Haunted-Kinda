using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;

public class MenuSceneManager : MonoBehaviour
{
    public UIGrave uigrave;
    public MenuSceneSettings settings;

    public static bool playTutorial;

    private void Awake()
    {
        Time.timeScale = 1;
    }
    public void StartGame()
    {
        Loader.Load(Loader.Scene.LevelScene);
    }

    public void LaunchSettings()
    {
        if(!settings.settingsRaised)
        {
            settings.Raise();
        }
        else
        {
            settings.Lower();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void SetTutorial(bool value)
    {
        playTutorial = value;
    }
}
