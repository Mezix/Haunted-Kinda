using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAt : MonoBehaviour
{
    private GameObject objectToLookAt;
    public GameObject DefaultLookAtObj;

    private void Start()
    {
        objectToLookAt = DefaultLookAtObj;
    }
    private void Update()
    {
        MouseHover();   
    }
    void FixedUpdate()
    {
        if(objectToLookAt)
        {
            transform.LookAt(objectToLookAt.transform);
        }
    }
    private void MouseHover()
    {
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag("3D UI"))
            {
                //print("foudn somethin");
                objectToLookAt = hitObj;
            }
            else
            {
                objectToLookAt = DefaultLookAtObj;
            }
        }
        else
        {
            objectToLookAt = DefaultLookAtObj;
        }
    }
}
