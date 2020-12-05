using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour
{
    private static Text TimeText;
    public static int hours;
    public static int minutes;

    private void Awake()
    {
        TimeText = GetComponent<Text>();
    }

    private void Update()
    {
        SetTime();
    }
    public static void SetTime()
    {
        hours = Mathf.FloorToInt((LevelSceneManager.instance._lighting.DayTimerForClock * 12) % 24);
        minutes = Mathf.FloorToInt(LevelSceneManager.instance._lighting.DayTimerForClock * 60 * 12 % 60);

        if (hours < 10)
        {
            TimeText.text = "0" + hours.ToString() + ":";
        }
        else
        {
            TimeText.text = hours.ToString() + ":";
        }
        if(minutes < 10)
        {
            TimeText.text += "0" + minutes.ToString();
        }
        else
        {
            TimeText.text += minutes.ToString();
        }
    }
}
