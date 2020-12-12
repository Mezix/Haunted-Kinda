using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAt : MonoBehaviour
{
    private GameObject objectToLookAt;
    public GameObject DefaultLookAtObj;
    public GameObject rotateGuide;

    public float time;
    private void Start()
    {
        time = 0;
        objectToLookAt = DefaultLookAtObj;
    }
    private void Update()
    {
        time += Time.deltaTime;
        if(time > 6)
        {
            MouseHover();
        } 
    }
    void FixedUpdate()
    {
        if(objectToLookAt)
        {
            rotateGuide.transform.LookAt(objectToLookAt.transform);
            SlowlyLookAt();
        }
    }

    public void SlowlyLookAt()
    {
        Vector3 direction = objectToLookAt.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 5 * Time.deltaTime);
    }
    private void MouseHover()
    {
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag("3D UI"))
            {
                objectToLookAt = hitObj;
            }
            else
            {
                if (!MenuSceneSettings.holdingSlider)
                {
                    objectToLookAt = DefaultLookAtObj;
                }
            }
        }
        else
        {
            if (!MenuSceneSettings.holdingSlider)
            {
                objectToLookAt = DefaultLookAtObj;
            }
        }
    }
}
