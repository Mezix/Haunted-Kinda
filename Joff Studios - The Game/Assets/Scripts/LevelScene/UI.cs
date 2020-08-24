using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public DayNightLighting lighting;
    public Player player;

    public GameObject portrait;
    public GameObject DashMeter;
    public GameObject EndScreen;
    public GameObject PauseScreen;
    public GameObject SunDialObject;
    public GameObject SunDial;

    private void Update()
    {
        SetSundialRotation();
        SetDashmeterFill();
    }

    void SetDashmeterFill()
    {
        DashMeter.transform.Find("bar").GetComponent<Image>().fillAmount = Mathf.Min(1, player.timeSinceLastDash)/player.dashCooldown;
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

        //disabled
        EndScreen.SetActive(false);
        PauseScreen.SetActive(false);
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
        //Time.timeScale = 1;
    }
}
