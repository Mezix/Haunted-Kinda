using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public DayNightLighting lighting;
    private Player player;

    public GameObject portrait;
    public GameObject DashMeter;
    public GameObject EndScreen;
    public GameObject PauseScreen;
    public GameObject SunDialObject;
    public GameObject SunDial;
    public GameObject Buttons;
    public Image buttonRend;
    public GameObject Instructions;

    public Sprite E;
    public Sprite Q;

    //DIALOGUE

    public GameObject UIDialogObj;


    private void Update()
    {
        SetSundialRotation();
        SetDashmeterFill();

        SelectButtonToShow();
    }
    public void SetPlayerRef()
    {
        player = References.Player.GetComponent<Player>();
    }
    private void SelectButtonToShow()
    {
        if(player._offeringsCollider.OfferingsInCollider.Count > 0)
        {
            foreach(Offering offering in player._offeringsCollider.OfferingsInCollider)
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
            if(!player.IsPossessing)
            {
                buttonRend.sprite = Q;
                Buttons.SetActive(true);
                return;
            }
        }
        Buttons.SetActive(false);
    }

    void SetDashmeterFill()
    {
        DashMeter.transform.Find("bar").GetComponent<Image>().fillAmount = Mathf.Min(1, player.TimeSinceLastDash)/player._dashCooldown;
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
        portrait.SetActive(true);
        DashMeter.SetActive(true);
        SunDialObject.SetActive(true);
        SunDial.SetActive(true);

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
        SunDialObject.SetActive(true);
        SunDial.SetActive(true);
    }
    public void HidePlayerUI()
    {
        portrait.SetActive(false);
        DashMeter.SetActive(false);
        SunDialObject.SetActive(false);
        SunDial.SetActive(false);
    }

    public void UIPause()
    {
        PauseScreen.SetActive(true);
    }
    public void UIUnpause()
    {
        PauseScreen.SetActive(false);
        Instructions.SetActive(false);
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
