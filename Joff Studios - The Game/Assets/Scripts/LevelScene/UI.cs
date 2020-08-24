using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject portrait;
    public GameObject DashMeter;
    public GameObject EndScreen;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void StartGame()
    {
        //enabled
        portrait.SetActive(true);
        DashMeter.SetActive(true);

        //disabled
        EndScreen.SetActive(false);
    }

    public void UIDash()
    {

    }
    public void UIScream()
    {

    }

    public void ShowEndScreen()
    {
        EndScreen.SetActive(true);
    }
}
