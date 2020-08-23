using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosessableObject : MonoBehaviour
{
    private bool isPossessed;
    private bool canWalk;
    void Start()
    {
        canWalk = true;
    }

    public void Possess()
    {
        isPossessed = true;
    }

    public void Deposses()
    {
        isPossessed = false;
    }
    private void FixedUpdate()
    {
        if(isPossessed)
        {
            if (canWalk)
            {

            }
        }
    }
}
