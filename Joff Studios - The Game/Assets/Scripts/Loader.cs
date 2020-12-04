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
            GraphicsSettings.renderPipelineAsset = MenuSceneManager.instance.Pipeline2D;
            QualitySettings.renderPipeline = MenuSceneManager.instance.Pipeline2D;
        }
        if(scene.ToString() == "MainMenuScene")
        {
            GraphicsSettings.renderPipelineAsset = LevelSceneManager.instance.Pipeline3D;
            QualitySettings.renderPipeline = LevelSceneManager.instance.Pipeline3D;
        }
        SceneManager.LoadScene(scene.ToString());
    }
}
