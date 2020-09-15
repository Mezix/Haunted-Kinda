using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;

public class UIGrave : MonoBehaviour
{
    public GameObject StartButtonObj;
    public GameObject SettingsButtonObj;
    public GameObject QuitButtonObj;

    private void Start()
    { 
        StartCoroutine(RaiseGrave());
    }
    public IEnumerator RaiseGrave()
    {
        yield return new WaitForSeconds(6f);

        Destroy(Camera.main.GetComponent<SimpleCameraController>()); //regain control of our camera again
        float timer = 0f;
        while(transform.position.y <= 0.45f)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, 0.45f, transform.position.z), 0.05f);
            yield return new WaitForFixedUpdate();
            if(timer > 1f)
            {
                ButtonsActivatable();
            }
        }
        print("done");
        transform.position = new Vector3(transform.position.x, 0.45f, transform.position.z);
    }

    private void ButtonsActivatable()
    {
        StartButtonObj.tag = "3D UI";
        SettingsButtonObj.tag = "3D UI";
        QuitButtonObj.tag = "3D UI";
    }
}
