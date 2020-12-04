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

    public static void SetTime(float timeInSeconds, float SecondsInAFullADay)
    {
        hours = Mathf.FloorToInt(timeInSeconds / SecondsInAFullADay * 24);
        minutes = Mathf.FloorToInt((timeInSeconds / SecondsInAFullADay * 60 * 24 % 60));
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
