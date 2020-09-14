using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MenuSceneSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public bool settingsRaised;

   public void SetVolume(float Volume)
   {
        audioMixer.SetFloat("MasterVolume", Volume);
   }
   
    public void Raise()
    {
        StartCoroutine(RaiseGrave());
    }
    public void Lower()
    {
        StartCoroutine(LowerGrave());
    }
    private IEnumerator RaiseGrave()
    {
        StopCoroutine(LowerGrave());

        settingsRaised = false;
        while (transform.position.y <= 0.33f)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0.35f, transform.position.z), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        print("done");
        settingsRaised = true;
        transform.position = new Vector3(transform.position.x, 0.35f, transform.position.z);
    }
    private IEnumerator LowerGrave()
    {
        StopCoroutine(RaiseGrave());
        settingsRaised = true;
        while (transform.position.y >= -0.68f)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, -0.7f, transform.position.z), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        settingsRaised = false;
        transform.position = new Vector3(transform.position.x, -0.7f, transform.position.z);
    }
}
