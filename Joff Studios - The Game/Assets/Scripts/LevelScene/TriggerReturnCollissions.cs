using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerReturnCollissions : MonoBehaviour
{
    public List<GraveRobber> GraveRobbersInCollider;
    public List<GraveGhost> GhostsInCollider;
    public List<Gravestone> GravesInCollider;
    public List<Offering> OfferingsInCollider;
    public List<PosessableObject> PossessablesInCollider;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<GraveRobber>())
        {
            GraveRobbersInCollider.Add(collision.GetComponent<GraveRobber>());
        }
        if (collision.GetComponent<GraveGhost>())
        {
            GhostsInCollider.Add(collision.GetComponent<GraveGhost>());
        }
        if (collision.GetComponent<Gravestone>())
        {
            GravesInCollider.Add(collision.GetComponent<Gravestone>());
        }
        if (collision.GetComponent<Offering>())
        {
            OfferingsInCollider.Add(collision.GetComponent<Offering>());
        }
        if (collision.GetComponent<PosessableObject>())
        {
            PossessablesInCollider.Add(collision.GetComponent<PosessableObject>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<GraveRobber>())
        {
            GraveRobbersInCollider.Remove(collision.GetComponent<GraveRobber>());
        }
        if (collision.GetComponent<GraveGhost>())
        {
            GhostsInCollider.Remove(collision.GetComponent<GraveGhost>());
        }
        if (collision.GetComponent<Gravestone>())
        {
            GravesInCollider.Remove(collision.GetComponent<Gravestone>());
        }
        if (collision.GetComponent<Offering>())
        {
            OfferingsInCollider.Remove(collision.GetComponent<Offering>());
        }
        if (collision.GetComponent<PosessableObject>())
        {
            PossessablesInCollider.Remove(collision.GetComponent<PosessableObject>());
        }
    }
}
