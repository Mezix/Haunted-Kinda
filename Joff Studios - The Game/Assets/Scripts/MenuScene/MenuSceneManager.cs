using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSceneManager : MonoBehaviour
{
    public UIGrave uigrave;
    public MenuSceneSettings settings;

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
}
