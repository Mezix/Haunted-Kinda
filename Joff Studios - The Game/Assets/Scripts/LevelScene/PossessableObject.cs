using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessableObject : MonoBehaviour
{
    [SerializeField]
    private bool isPossessed;
    public Gravestone grave;
    public bool _canMove;
    public GameObject _moveablePart;
    public float _moveSpeed;
    public GameObject CastShadow;

    private Rigidbody2D possessableRB;

    public GameObject exclamation;
    public bool lockMovement;

    private void Awake()
    {
        possessableRB = GetComponent<Rigidbody2D>();
    }

    public void Possess()
    {
        isPossessed = true;
        if(_canMove)
        {
            StopCoroutine(DropPossessable());
            StartCoroutine(FloatPossessable());
        }
        StartCoroutine(ExclamationMark());
    }

    public void Deposses()
    {
        if (_canMove)
        {
            StopCoroutine(FloatPossessable());
            StartCoroutine(DropPossessable());
        }
        isPossessed = false;
    }
    private void FixedUpdate()
    {
        if(isPossessed)
        {
            if(grave && !grave.GetComponentInChildren<GraveGhost>().lootStolen) //if our loot has been stolen, we cant fix the grave yet!
            {
                StartCoroutine(RestoreGrave());
            }
            else if (_canMove && !lockMovement)
            {
                MovePossessableObject();
            }
        }
    }

    private void MovePossessableObject()
    {
        Vector2 movement;
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        //print(movement);
        possessableRB.MovePosition(possessableRB.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
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

    private IEnumerator FloatPossessable()
    {
        _moveablePart.transform.localPosition = Vector2.zero;
        while (Vector2.Distance(_moveablePart.transform.localPosition, new Vector2(0, 0.5f)) > 0.05f)
        {
            _moveablePart.transform.localPosition = Vector2.Lerp(_moveablePart.transform.localPosition, new Vector2(0, 0.5f), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        _moveablePart.transform.localPosition = new Vector2(0, 0.5f);
    }
    private IEnumerator DropPossessable()
    {
        _moveablePart.transform.localPosition = new Vector2(0, 0.5f);
        while (Vector2.Distance(_moveablePart.transform.localPosition, new Vector2(0, 0)) > 0.05f)
        {
            _moveablePart.transform.localPosition = Vector2.Lerp(_moveablePart.transform.localPosition, new Vector2(0, 0f), 0.05f);
            yield return new WaitForFixedUpdate();
        }
        _moveablePart.transform.localPosition = new Vector2(0, 0f);
        lockMovement = false;
    }
    private IEnumerator ExclamationMark()
    {
        if(exclamation)
        {
            exclamation.SetActive(true);
            exclamation.GetComponent<Animator>().SetBool("Possessed", true);
            yield return new WaitForSeconds(0.1f);
            exclamation.GetComponent<Animator>().SetBool("Possessed", false);
            yield return new WaitForSeconds(1f);
            exclamation.SetActive(false);
        }
    }
}
