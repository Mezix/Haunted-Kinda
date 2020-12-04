using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;
using UnityEngine.Rendering;

public class MenuSceneManager : MonoBehaviour
{
    public static MenuSceneManager instance;
    public UIGrave uigrave;
    public MenuSceneSettings settings;

    public static bool playTutorial;

    public RenderPipelineAsset Pipeline3D;
    public RenderPipelineAsset Pipeline2D;

    public GameObject LoadingScreen;

    private void Awake()
    {
        GraphicsSettings.renderPipelineAsset = Pipeline3D;
        QualitySettings.renderPipeline = Pipeline3D;

        instance = this;
        Time.timeScale = 1;
    }
    private void Start()
    {
        LoadingScreen.SetActive(false);
    }
    public void StartGame()
    {
        StartCoroutine(ShowLoadingScreen());
    }

    public IEnumerator ShowLoadingScreen()
    {
        LoadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.5f);
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
