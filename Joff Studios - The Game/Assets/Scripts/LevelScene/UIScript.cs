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
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if(hit.collider)
        {
            if (hit.collider.TryGetComponent(out Offering offering))
            {
                if (!offering.disappearing)
                {
                    buttonRend.sprite = E;
                    Buttons.SetActive(true);
                    return;
                }
            }
        }

        hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider)
        {
            if (hit.collider.TryGetComponent(out PosessableObject possessable))
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
        q.eulerAngles = new Vector3(0,0,lighting.DayToNightRatio * 180);
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
}
