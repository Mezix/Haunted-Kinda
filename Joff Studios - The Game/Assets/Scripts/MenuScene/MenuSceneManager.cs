﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSceneManager : MonoBehaviour
{
    public void StartGame()
    {
        Loader.Load(Loader.Scene.LevelScene);
    }

    public void LaunchSettings()
    {
        print("settings");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
