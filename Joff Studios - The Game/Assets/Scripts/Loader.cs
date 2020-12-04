using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public static class Loader { 

    public enum Scene { LevelScene, MainMenuScene }

    public static void Load(Scene scene)
    {
        if (scene.ToString() == "LevelScene")
        {
            //GraphicsSettings.renderPipelineAsset.r
        }
        SceneManager.LoadScene(scene.ToString());
    }
}
