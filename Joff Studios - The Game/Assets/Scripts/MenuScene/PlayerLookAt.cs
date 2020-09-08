using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAt : MonoBehaviour
{
    private GameObject objectToLookAt;
    public GameObject DefaultLookAtObj;
    public GameObject rotateGuide;

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
            rotateGuide.transform.LookAt(objectToLookAt.transform);
            SlowlyLookAt();
        }
    }

    public void SlowlyLookAt()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateGuide.transform.rotation, 6f);

        //Vector3 currentRotation = transform.rotation.eulerAngles;
        //Vector3 newRotation = rotateGuide.transform.rotation.eulerAngles;

        //newRotation = Vector3.Lerp(currentRotation, newRotation, 0.5f);
        //Quaternion q = Quaternion.identity;
        //q.eulerAngles = newRotation;
        //transform.rotation = q;
    }
    private void MouseHover()
    {
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            GameObject hitObj = hit.collider.gameObject;
            print(hitObj);
            if (hitObj.CompareTag("3D UI"))
            {
                print("foudn somethin");
                objectToLookAt = hitObj;
            }
            else
            {
                objectToLookAt = DefaultLookAtObj;
            }
        }
        else
        {
            print("nothing");
            objectToLookAt = DefaultLookAtObj;
        }
    }
}
