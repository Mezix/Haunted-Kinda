using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerReturnCollissions : MonoBehaviour
{
    public List<GraveRobber> GraveRobbersInCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<GraveRobber>())
        {
            GraveRobbersInCollider.Add(collision.GetComponent<GraveRobber>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<GraveRobber>())
        {
            GraveRobbersInCollider.Remove(collision.GetComponent<GraveRobber>());
        }
    }
}
