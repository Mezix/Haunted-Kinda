using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public DayNightLighting lighting;
    private Player player;

    //enabled at the start of the scene

    public GameObject portrait;
    public GameObject DashMeter;
    public GameObject ScreamMeter;
    public GameObject SunDialObject;
    public GameObject SunDial;
    public GameObject Inventory;

    //disabled at the start of the scene

    public GameObject EndScreen;
    public GameObject PauseScreen;
    public GameObject Buttons;
    public GameObject Instructions;

    public Image buttonRend;
    public Sprite E;
    public Sprite Q;

    //DIALOGUE

    public GameObject UIDialogObj;
    private InventoryUI _inventory;

    private void Awake()
    {
        _inventory = Inventory.GetComponent<InventoryUI>();
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
        if(!LevelSceneManager.paused)
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
        DashMeter.transform.Find("bar").GetComponent<Image>().fillAmount = Mathf.Min(1, player.TimeSinceLastDash/player._dashCooldown);
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
    public void StartGame()
    {
        //enabled
        ShowPlayerUI();

        //disabled
        EndScreen.SetActive(false);
        PauseScreen.SetActive(false);
        Instructions.SetActive(false);
        Buttons.SetActive(false);
        UIDialogObj.SetActive(false);
    }

    public void UIDash()
    {
        StartCoroutine(Dash());
    }
    public void UIScream()
    {
        StartCoroutine(Scream());
    }

    public void ShowEndScreen()
    {
        EndScreen.SetActive(true);
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

    public void ShowPlayerUI()
    {
        portrait.SetActive(true);
        DashMeter.SetActive(true);
        ScreamMeter.SetActive(true);
        SunDialObject.SetActive(true);
        SunDial.SetActive(true);
        Inventory.SetActive(true);
    }
    public void HidePlayerUI()
    {
        portrait.SetActive(false);
        DashMeter.SetActive(false);
        ScreamMeter.SetActive(false);
        SunDialObject.SetActive(false);
        SunDial.SetActive(false);
        Inventory.SetActive(false);
    }

    public void UIPause()
    {
        PauseScreen.SetActive(true);
        TurnOffDialogue();
    }
    public void UIUnpause()
    {
        PauseScreen.SetActive(false);
        Instructions.SetActive(false);
        TurnOnDialogue();
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
}
