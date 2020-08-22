using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravestone : MonoBehaviour
{
    [SerializeField]
    private GameObject InhabitedGhost;

    private void Awake()
    {
        
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<PlayerMovement>())
        {
            if(InhabitedGhost)
            InhabitedGhost.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>())
        {
            if(InhabitedGhost)
            InhabitedGhost.GetComponent<GraveGhost>().FadeAway();
        }
    }
}
