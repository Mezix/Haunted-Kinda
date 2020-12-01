using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessableObject : MonoBehaviour
{
    [SerializeField]
    private bool isPossessed;
    public Gravestone grave { get; private set; }
    private bool isRestoring;
    private bool rotateRight;
    public GameObject CastShadow;
    public GameObject exclamation;

    //MOVEMENT

    public bool _canMove;
    public GameObject _moveablePart;
    private Rigidbody2D possessableRB;
    public float _moveSpeed;
    Vector2 movement;
    public bool lockMovement;

    //DASH

    public float _dashSpeed; //the Velocity multiplier of our dash
    public float _dashTime; //how long does our dash last
    public float _dashCooldown; //the cooldown of our dash in seconds
    public float TimeSinceLastDash { get; private set; } //time elapsed since we last dashed, to determine when we can dash next
    private bool dashing; //check if we are dashing, so we can't dash again immediately

    //AFTERIMAGE

    public GameObject AfterImagePrefab;
    public Vector3 lastAfterImagePos;

    //BROOM AND WATERINGCAN

    private bool sweepingBroom;
    private bool watering;

    private void Awake()
    {
        possessableRB = GetComponent<Rigidbody2D>();
        grave = GetComponent<Gravestone>();
    }

    private void Start()
    {
        _dashCooldown = 1f; //set our starting values
        _dashSpeed = 25;
        _dashTime = 0.3f;
        TimeSinceLastDash = _dashCooldown;
    }
    public void Possess()
    {
        isPossessed = true;
        if(_canMove)
        {
            StopCoroutine(DropPossessable());
            StartCoroutine(FloatPossessable());
        }
        StartCoroutine(ShowExclamationMark());
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
        if (isPossessed)
        {
            if (tag != "Broom" && tag != "WateringCan")
            {
                WigglePossessable();
            }
        }
    }

    private void Update()
    {
        TimeSinceLastDash += Time.deltaTime;
        if (isPossessed)
        {
            if (grave)//if our loot has been stolen, we cant fix the grave yet!
            {
                if (!isRestoring)
                {
                    StartCoroutine(RestoreGrave());
                }
            }
            else
            {
                
                if (_canMove && !lockMovement)
                {
                    MovePossessableObject();
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        if (!dashing && TimeSinceLastDash >= _dashCooldown && movement.magnitude > 0)
                        {
                            TimeSinceLastDash = 0;
                            StartCoroutine(Dash());
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (tag == "Broom")
                {
                    if(!sweepingBroom)
                    {
                        StartCoroutine(SweepBroom());
                    }
                }
                if (tag == "WateringCan")
                {
                    if (!watering)
                    {
                        StartCoroutine(StartWatering());
                    }
                }
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

    //MOVEABLE POSSESSABLES
    private void MovePossessableObject()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        possessableRB.MovePosition(possessableRB.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
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
    private IEnumerator ShowExclamationMark()
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
    public void ReturnPossessableToGhost()
    {
        Destroy(gameObject);
    }

    private IEnumerator Dash()
    {
        print("dash");
        dashing = true;
        lockMovement = true;
        Vector2 direction = movement.normalized;
        References.playerScript._dashSound.Play();

        float timer = 0;

        GameObject afterimage = Instantiate(AfterImagePrefab, transform.position, transform.rotation);
        afterimage.transform.position = _moveablePart.transform.position;
        afterimage.GetComponent<AfterImage>().SetAndFadeAfterimage(GetComponentInChildren<SpriteRenderer>());
        lastAfterImagePos = transform.position;
        while (timer <= _dashTime)
        {
            if (Vector3.Distance(transform.position, lastAfterImagePos) >= 0.5f)
            {
                afterimage = Instantiate(AfterImagePrefab, transform.position, transform.rotation);
                afterimage.transform.position = _moveablePart.transform.position;
                afterimage.GetComponent<AfterImage>().SetAndFadeAfterimage(GetComponentInChildren<SpriteRenderer>());
                lastAfterImagePos = transform.position;
            }

            timer += Time.deltaTime;
            possessableRB.velocity = direction * _dashSpeed - (timer * direction * 20); //decrease our speed over time

            yield return new WaitForFixedUpdate();
        }
        possessableRB.velocity = Vector2.zero;
        dashing = false;
        lockMovement = false;
    }

    //SPECIAL POSSESSABLES, BROOM AND WATERING CAN

    private IEnumerator SweepBroom()
    {
        sweepingBroom = true;
        StartCoroutine(PlayBroomAnimation());
        yield return new WaitForSeconds(0.5f);
        print("Broom");
        sweepingBroom = false;
    }

    private IEnumerator PlayBroomAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        print("broom sweepy");
    }
    private IEnumerator StartWatering()
    {
        print("WateringCan");
        watering = true;
        while (Input.GetKey(KeyCode.Space))
        {
            yield return new WaitForFixedUpdate();
            print("yeah.");
        }
        StartCoroutine(EndWatering());
    }
    private IEnumerator EndWatering()
    {
        yield return new WaitForFixedUpdate();
        watering = false;
    }
}
