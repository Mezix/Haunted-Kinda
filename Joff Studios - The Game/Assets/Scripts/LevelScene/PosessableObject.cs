using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosessableObject : MonoBehaviour
{
    [SerializeField]
    private bool isPossessed;
    public bool canWalk;
    public bool isGrave;

    public GameObject exclamation;

    void Start()
    {
        canWalk = true;
    }

    public void Possess()
    {
        isPossessed = true;
        StartCoroutine(Animate());
    }

    public void Deposses()
    {
        isPossessed = false;
    }
    private void FixedUpdate()
    {
        if(isPossessed)
        {
            if(isGrave)
            {
                //StartCoroutine(RestoreGrave());
            }
            else if (canWalk)
            {

            }
        }
    }

    IEnumerator RestoreGrave()
    {
        Gravestone grave = GetComponent<Gravestone>();
        while(grave.currentHealth < grave.maxHealth)
        {
            if(!isPossessed)
            {
                break;
            }
            grave.Restore(0.5f);
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator Animate()
    {
        print("Animate");
        exclamation.SetActive(true);
        exclamation.GetComponent<Animator>().SetBool("Possessed", true);
        yield return new WaitForSeconds(0.1f);
        exclamation.GetComponent<Animator>().SetBool("Possessed", false);
        yield return new WaitForSeconds(1f);
        exclamation.SetActive(false);
    }
}
