using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;
using UnityEngine.Rendering;
using System;

public class MenuSceneManager : MonoBehaviour
{
    public static MenuSceneManager instance;
    public UIGrave uigrave;
    public MenuSceneSettings settings;

    public static bool playTutorial;

    public RenderPipelineAsset Pipeline3D;
    public RenderPipelineAsset Pipeline2D;

    public GameObject LoadingScreen;
    public GameObject Vignette;

    private void Awake()
    {
        Vignette.SetActive(true);
        Vignette.transform.localScale = new Vector3(1,1,1);
        GraphicsSettings.renderPipelineAsset = Pipeline3D;
        QualitySettings.renderPipeline = Pipeline3D;

        instance = this;
        Time.timeScale = 1;
    }
    private void Start()
    {
        LoadingScreen.SetActive(false);
        StartCoroutine(OpenVignette());
    }

    private IEnumerator OpenVignette()
    {
        yield return new WaitForSeconds(2f);
        float time = 0f;
        while(time < 6f)
        {
            time += Time.deltaTime;
            Vignette.transform.localScale = new Vector3(1 + time / 6, 1 + time / 6, 1 + time / 6);
            yield return new WaitForFixedUpdate();
        }
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
