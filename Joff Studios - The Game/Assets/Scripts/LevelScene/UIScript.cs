using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UIScript : MonoBehaviour
{
    public AudioMixer audioMixer;

    public DayNightLighting lighting;
    private Player player;

    //enabled at the start of the game

    public GameObject portrait;
    public bool portraitHidden;
    public GameObject DashMeter;
    public bool DashMeterHidden;
    public GameObject ScreamMeter;
    public bool ScreamMeterHidden;
    public GameObject TimeDisplay;
    public bool TimeDisplayHidden;
    public Text daysText;
    public GameObject SunDial;
    public bool SundialHidden;
    public GameObject Inventory;
    public bool InventoryHidden;
    public GameObject DarkOverlay;

    //disabled at the start of the scene

    public GameObject PauseScreen;
    public GameObject PauseOverlay;
    public GameObject SettingsScreen;
    public Slider MasterVolume;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;
    public GameObject Buttons;
    public GameObject Instructions;

    public Image buttonRend;
    public Sprite E;
    public Sprite Q;

    //DIALOGUE

    public GameObject UIDialogObj;
    public bool DialogueShown;

    private InventoryUI _inventory;
    public bool proximityButtonsEnabled;

    //TUTORIAL PROMPTS

    public Image TutorialPromptsBackground;
    public Text PromptText;
    private float promptFadeAmount;
    private float promptOpacity = 0;

    //CREDITS & ENDSCREEN

    public GameObject EndScreen;
    public Text ActualGrade;
    public Text GradeMessage;
    public GameObject CreditScreen;
    public GameObject MainMenuButton;

    private void Awake()
    {
        _inventory = Inventory.GetComponent<InventoryUI>();
        proximityButtonsEnabled = true;
        promptOpacity = 0;
        promptFadeAmount = 0.05f;
        TutorialPromptsBackground.color = PromptText.color = new Color(1, 1, 1, promptOpacity);

        Events.current.ObjectPossessed += UIPossess;
    }

    private void Update()
    {
        SetSundialRotation();
        SetDashMeterFill();
        SetScreamMeterFill();
        SelectButtonToShow();
    }

    public void SetPlayerRef()
    {
        player = References.Player.GetComponent<Player>();
    }
    private void SelectButtonToShow()
    {
        if(!LevelSceneManager.paused && proximityButtonsEnabled)
        {
            if (player._offeringsCollider.OfferingsInCollider.Count > 0)
            {
                foreach (Offering offering in player._offeringsCollider.OfferingsInCollider)
                {
                    if (!offering.disappearing)
                    {
                        buttonRend.sprite = E;
                        Buttons.SetActive(true);
                        return;
                    }
                }
            }
            if (player._possessableCollider.PossessablesInCollider.Count > 0)
            {
                if (!player.IsPossessing)
                {
                    buttonRend.sprite = Q;
                    Buttons.SetActive(true);
                    return;
                }
            }
        }
        Buttons.SetActive(false);
    }

    void SetDashMeterFill()
    {
        if(!References.playerScript.IsPossessing)
        {
            DashMeter.transform.Find("bar").GetComponent<Image>().fillAmount = Mathf.Min(1, player.TimeSinceLastDash / player._dashCooldown);
        }
        else
        {
            if(References.playerScript.possessedObject._canMove)
            {
                DashMeter.transform.Find("bar").GetComponent<Image>().fillAmount = Mathf.Min(1, References.playerScript.possessedObject.TimeSinceLastDash / References.playerScript.possessedObject._dashCooldown);}
            }
    }
    private void SetScreamMeterFill()
    {
        ScreamMeter.transform.Find("bar").GetComponent<Image>().fillAmount = Mathf.Min(1, player.TimeSinceLastScream/player._screamCooldown);
    }
    void SetSundialRotation()
    {
        Quaternion q = SunDial.transform.rotation;
        q.eulerAngles = new Vector3(0, 0, lighting.DayToNightRatio * 180);
        SunDial.transform.rotation = q;
    }

    public void SetDays(int Day, int maxDays)
    {
        daysText.text = Day + "/" + maxDays;
    }
    public void StartGame()
    {
        //enabled
        ShowPlayerUI();
        DarkOverlay.SetActive(true);
        

        //disabled
        HideCredits();
        EndScreen.SetActive(false);
        PauseScreen.SetActive(false);
        PauseOverlay.SetActive(false);
        Instructions.SetActive(false);
        Buttons.SetActive(false);
        UIDialogObj.SetActive(false);
        SettingsScreen.SetActive(false);
        HideCredits();

        MasterVolume.value = MenuSceneSettings.masterVolume;
        MusicVolumeSlider.value = MenuSceneSettings.musicVolume;
        SFXVolumeSlider.value = MenuSceneSettings.SFXVolume;
    }

    public void StartTutorial()
    {
        HidePlayerUI();
        DarkOverlay.SetActive(true);
        HideCredits();
        EndScreen.SetActive(false);
        PauseScreen.SetActive(false);
        PauseOverlay.SetActive(false);
        Instructions.SetActive(false);
        Buttons.SetActive(false);
        UIDialogObj.SetActive(false);
        SettingsScreen.SetActive(false);

        proximityButtonsEnabled = false;
        portraitHidden = true;
        DashMeterHidden = true;
        ScreamMeterHidden = true;
        SundialHidden = true;
        TimeDisplayHidden = true;
        InventoryHidden = true;

        MasterVolume.value = MenuSceneSettings.masterVolume;
        MusicVolumeSlider.value = MenuSceneSettings.musicVolume;
        SFXVolumeSlider.value = MenuSceneSettings.SFXVolume;
    }

    public void UIDash()
    {
        StartCoroutine(Dash());
    }
    public void UIScream()
    {
        StartCoroutine(Scream());
    }

    IEnumerator Dash()
    {
        portrait.GetComponentInChildren<Animator>().SetBool("Dash", true);
        yield return new WaitForSeconds(0.5f);
        portrait.GetComponentInChildren<Animator>().SetBool("Dash", false);
    }
    IEnumerator Scream()
    {
        portrait.GetComponentInChildren<Animator>().SetBool("Scream", true);
        yield return new WaitForSeconds(0.75f);
        portrait.GetComponentInChildren<Animator>().SetBool("Scream", false);
    }
    public void UIPossess(GameObject obj)
    {
        if (obj != References.Player)
        {
            ScreamMeter.SetActive(false);
            Inventory.SetActive(false);
            if(!obj.GetComponent<PossessableObject>()._canMove)
            {
                DashMeter.SetActive(false);
            }
        }
        else
        {
            UIDepossess();
        }
    }
    public void UIDepossess()
    {
        ScreamMeter.SetActive(true);
        Inventory.SetActive(true);
        DashMeter.SetActive(true);
    }

    public void ShowPlayerUI()
    {
        if (!portraitHidden)
        {
            portrait.SetActive(true);
        }
        if (!DashMeterHidden)
        {
            DashMeter.SetActive(true);
        }
        if (!ScreamMeterHidden)
        {
            ScreamMeter.SetActive(true);
        }
        if (!SundialHidden)
        {
            SunDial.SetActive(true);
        }
        if (!TimeDisplayHidden)
        {
            TimeDisplay.SetActive(true);
        }
        if (!InventoryHidden)
        {
            Inventory.SetActive(true);
        }
    }
    public void HidePlayerUI()
    {
        portrait.SetActive(false);
        DashMeter.SetActive(false);
        ScreamMeter.SetActive(false);
        TimeDisplay.SetActive(false);
        SunDial.SetActive(false);
        Inventory.SetActive(false);
    }

    public void UIPause()
    {
        PauseOverlay.SetActive(true);
        PauseScreen.SetActive(true);
        SettingsScreen.SetActive(false);
        TurnOffDialogue();
    }
    public void UIUnpause()
    {
        PauseOverlay.SetActive(false);
        PauseScreen.SetActive(false);
        SettingsScreen.SetActive(false);
        Instructions.SetActive(false);

        if(DialogueShown)
        {
            TurnOnDialogue();
        }
    }

    public IEnumerator FadeFromBlack()
    {
        Image overlay = DarkOverlay.GetComponent<Image>();
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);
        for (int i = 256; i > 0; i--)
        {
            overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, (i / 255f));
            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator FadeToBlack()
    {
        Image overlay = DarkOverlay.GetComponent<Image>();
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);
        for (int i = 0; i < 256; i++)
        {
            overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, (i / 255f));
            yield return new WaitForFixedUpdate();
        }
    }

    public void Help()
    {
        if(Instructions.activeSelf)
        {
            Instructions.SetActive(false);
        }
        else
        {
            Instructions.SetActive(true);
        }
    }

    //DIALOGUE

    public void TurnOnDialogue()
    {
        UIDialogObj.SetActive(true);
        HidePlayerUI();
    }
    public void TurnOffDialogue()
    {
        UIDialogObj.SetActive(false);
        ShowPlayerUI();
    }

    //TUTORIAL PROMPTS

    public IEnumerator FadeInPrompts()
    {
        StopCoroutine(FadeOutPrompts());
        promptOpacity = 0;
        while (promptOpacity <= 1)
        {
            promptOpacity += promptFadeAmount;
            yield return new WaitForFixedUpdate();
            TutorialPromptsBackground.color = PromptText.color = new Color(1, 1, 1, promptOpacity);
        }
        promptOpacity = 1;
        TutorialPromptsBackground.color = PromptText.color = new Color(1, 1, 1, 1);
    }
    public IEnumerator FadeOutPrompts()
    {
        StopCoroutine(FadeInPrompts());
        promptOpacity = 1;
        while (promptOpacity >= 0)
        {
            promptOpacity -= promptFadeAmount * 2;
            yield return new WaitForFixedUpdate();
            TutorialPromptsBackground.color = PromptText.color = new Color(1, 1, 1, promptOpacity);
        }
        promptOpacity = 0;
        TutorialPromptsBackground.color = PromptText.color = new Color(1, 1, 1, 0);
    }

    public void PromptScream()
    {
        PromptText.text = "Press 'Spacebar' to Scream!";
    }
    public void PromptPossess()
    {
        PromptText.text = "Press 'Q' to Fix up the grave!";
    }
    public void PromptDepossess()
    {
        PromptText.text = "Press 'Q' again to leave the grave!";
    }
    public void PromptDash()
    {
        PromptText.text = "Press 'Shift' to Dash!";
    }
    public void PromptPossessShades()
    {
        PromptText.text = "Press 'Q' to Possess the sunglasses!";
    }
    public void PromptDepossessShades()
    {
        PromptText.text = "Press 'Q' to place down the sunglasses!";
    }
    public void PromptMoveWithArrowKeys()
    {
        PromptText.text = "Use the arrow keys or WASD to move around!";
    }
    public void PromptPickUp()
    {
        PromptText.text = "Press 'E' to pick up offerings near you!";
    }
    public void PromptPlaceDown()
    {
        PromptText.text = "Press 'F' to give the offerings to the ghosts!";
    }
    public void ShowEndScreen()
    {
        EndScreen.SetActive(true);
        ScoringSystem.instance.CalculateScore();
        ScoringSystem.instance.UIEndScreenCalculation();
        string grade = ScoringSystem.instance.CalculateGrade();
        ActualGrade.text = grade;
        string message = " Dunno";
        if(grade == "D")
        {
            message = "Well, at least you tried your best :)";
        }
        else if(grade == "C")
        {
            message = "Not bad friend!";
        }
        else if (grade == "B")
        {
            message = "Hey well done!";
        }
        else if (grade == "A")
        {
            message = "Wow fantastic!";
        }
        else if (grade == "S")
        {
            message = "Incredible Job!";
        }
        else if (grade == "S+")
        {
            message = "You are a true friend of ghosts!";
        }
        GradeMessage.text = message;
    }
    public void ShowCredits()
    {
        CreditScreen.SetActive(true);
        MainMenuButton.SetActive(true);
    }
    public void HideCredits()
    {
        CreditScreen.SetActive(false);
        MainMenuButton.SetActive(false);
    }
    public void ShowSettings()
    {
        PauseScreen.SetActive(false);
        SettingsScreen.SetActive(true);
    }
    public void HideSettings()
    {
        PauseScreen.SetActive(true);
        SettingsScreen.SetActive(false);
    }
    public void SetMasterVolume(float Volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Volume) * 20);
        MenuSceneSettings.masterVolume = Volume;
    }
    public void SetMusicVolume(float Volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Volume) * 20);
        MenuSceneSettings.masterVolume = Volume;
    }
    public void SetSFXVolume(float Volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Volume) * 20);
        MenuSceneSettings.masterVolume = Volume;
    }
}
