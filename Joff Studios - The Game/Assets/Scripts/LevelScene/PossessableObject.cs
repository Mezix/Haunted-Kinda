using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessableObject : MonoBehaviour
{
    [SerializeField]
    private bool isPossessed;
    public Gravestone grave { get; private set; }
    public bool _canMove;
    private bool isRestoring;
    public GameObject _moveablePart;
    [SerializeField]
    private bool rotateRight;
    public float _moveSpeed;
    public GameObject CastShadow;

    private Rigidbody2D possessableRB;

    public GameObject exclamation;
    public bool lockMovement;

    private void Awake()
    {
        possessableRB = GetComponent<Rigidbody2D>();
        grave = GetComponent<Gravestone>();
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

    public void Depossess()
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
            if(grave && !isRestoring)// && !grave.GetComponentInChildren<GraveGhost>().lootStolen) //if our loot has been stolen, we cant fix the grave yet!
            {
                StartCoroutine(RestoreGrave());
            }
            else if (_canMove && !lockMovement)
            {
                WigglePossessable();
                MovePossessableObject();
            }
        }
    }

    private void WigglePossessable()
    {
        if(rotateRight)
        {
            if (_moveablePart.transform.rotation.z * Mathf.Rad2Deg < 8)
            {
                _moveablePart.transform.Rotate(Vector3.forward, 1.5f);
            }
            else
            {
                rotateRight = false;
            }
        }
        else
        {
            if (_moveablePart.transform.rotation.z * Mathf.Rad2Deg > -8)
            {
                _moveablePart.transform.Rotate(Vector3.forward, -1.5f);
            }
            else
            {
                rotateRight = true;
            }
        }
    }

    private IEnumerator RotateBackToNormal()
    {
        while (Mathf.Abs(_moveablePart.transform.rotation.z * Mathf.Rad2Deg) > 1)
        {
            if (_moveablePart.transform.rotation.z > 0)
            {
                _moveablePart.transform.Rotate(Vector3.forward, -1);
            }
            else
            {
                _moveablePart.transform.Rotate(Vector3.forward, 1);
            }
            yield return new WaitForFixedUpdate();
        }
        Quaternion q = _moveablePart.transform.rotation;
        q.eulerAngles = new Vector3(0,0,0);
        _moveablePart.transform.rotation = q;
    }
    private void MovePossessableObject()
    {
        Vector2 movement;
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        possessableRB.MovePosition(possessableRB.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator RestoreGrave()
    {
        isRestoring = true;
        print("restore");
        while(grave.currentHealth < grave.maxHealth)
        {
            if(!isPossessed)
            {
                break;
            }
            grave.RestoreGrave(0.25f);
            yield return new WaitForSeconds(0.01f);
        }
        if(!LevelSceneManager._isPlayingTutorial)
        {
            yield return new WaitWhile(() => !References.playerScript.IsPossessing);
            StartCoroutine(References.playerScript.DepossessObject());
            isRestoring = false;
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
        StartCoroutine(RotateBackToNormal());
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

    public void ReturnPossessable()
    {
        Destroy(gameObject);
    }
}
