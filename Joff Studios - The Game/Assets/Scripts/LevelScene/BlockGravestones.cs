using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGravestones : MonoBehaviour
{
    public List<Gravestone> GravesInCollider;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Gravestone>())
        {
            GravesInCollider.Add(collision.GetComponent<Gravestone>());
            Events.current.BlockGravestone(collision.GetComponent<Gravestone>());
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Gravestone>())
        {
            GravesInCollider.Remove(collision.GetComponent<Gravestone>());
            Events.current.UnblockGravestone(collision.GetComponent<Gravestone>());
        }
    }


}