using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PossessableObject : MonoBehaviour
{
    [SerializeField]
    public bool isPossessed;
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
    public float TimeSinceLastBroomSweep;
    public float TimeBetweenSweeps;
    public List<Gravestone> nearbyGraves = new List<Gravestone>();
    public AnimationClip BroomSweep;

    public bool usingWateringCan;
    public GameObject WateringSFX;
    public GameObject waterParticlesSpawn;
    public GameObject waterParticles;
    public GameObject[] Flowers;
    public int flowerIndex = 0;

    //SHADERS FOR POSSESSION

    public Shader Outline;
    public Shader SpriteUnlit;

    private void Awake()
    {
        possessableRB = GetComponent<Rigidbody2D>();
        grave = GetComponent<Gravestone>();
        TimeBetweenSweeps = 0.75f;
    }

    private void Start()
    {
        _dashCooldown = 1f; //set our starting values
        _dashSpeed = 25;
        _dashTime = 0.3f;
        TimeSinceLastDash = _dashCooldown;
    }
    private void FixedUpdate()
    {
        if (isPossessed)
        {
            if (tag != "Broom" && tag != "WateringCan")
            {
                if(_moveablePart)
                WigglePossessable();
            }
        }
    }
    
    
    private void Update()
    {
        TimeSinceLastDash += Time.deltaTime;
        if(tag == "Broom")
        {
            TimeSinceLastBroomSweep += Time.deltaTime;
        }
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
                        if (!dashing && TimeSinceLastDash >= _dashCooldown && movement.magnitude > 0 && !LevelSceneManager._isPlayingTutorial)
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
                    if(!sweepingBroom && TimeSinceLastBroomSweep > TimeBetweenSweeps)
                    {
                        SweepBroom();
                    }
                }
                if (tag == "WateringCan")
                {
                    if (!usingWateringCan)
                    {
                        StartCoroutine(StartWatering());
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.F) && tag == "WateringCan")
            {
                NextFlower();
            }
        }
    }
    public void Possess()
    {
        if (tag != "Broom" && tag != "WateringCan")
        {
            GetComponentInChildren<SpriteRenderer>().material.shader = LevelSceneManager.instance.Outline;
            GetComponentInChildren<SpriteRenderer>().material.SetFloat("Vector1_53CFC1A5", 0.05f);
            GetComponentInChildren<SpriteRenderer>().color = new Color(0.7f, 0.8f, 1f, 0.75f);
        }
        isPossessed = true;
        if (_canMove)
        {
            StopCoroutine(DropPossessable());
            StartCoroutine(FloatPossessable());
        }
        StartCoroutine(ShowExclamationMark());
    }

    public void Depossess()
    {
        GetComponentInChildren<SpriteRenderer>().material.shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        GetComponentInChildren<SpriteRenderer>().material.SetFloat("Vector1_53CFC1A5", 0);
        GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        if (_canMove)
        {
            StopCoroutine(FloatPossessable());
            StartCoroutine(DropPossessable());
        }
        isPossessed = false;
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
        if(tag != "Broom")
        {
            _moveablePart.transform.localPosition = Vector2.zero;
            while (Vector2.Distance(_moveablePart.transform.localPosition, new Vector2(0, 0.5f)) > 0.05f)
            {
                _moveablePart.transform.localPosition = Vector2.Lerp(_moveablePart.transform.localPosition, new Vector2(0, 0.5f), 0.05f);
                yield return new WaitForFixedUpdate();
            }
            _moveablePart.transform.localPosition = new Vector2(0, 0.5f);
        }
        else
        {
            _moveablePart.transform.localPosition = Vector2.zero;
            while (Vector2.Distance(_moveablePart.transform.localPosition, new Vector2(0, 0.2f)) > 0.05f)
            {
                _moveablePart.transform.localPosition = Vector2.Lerp(_moveablePart.transform.localPosition, new Vector2(0, 0.5f), 0.05f);
                yield return new WaitForFixedUpdate();
            }
            _moveablePart.transform.localPosition = new Vector2(0, 0.2f);
        }
    }
    private IEnumerator DropPossessable()
    {
        StartCoroutine(RotateBackToNormal());
        if (tag == "Broom")
        {
            _moveablePart.transform.localPosition = new Vector2(0, 0.2f);
        }
        else
        {
            _moveablePart.transform.localPosition = new Vector2(0, 0.5f);
        }
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

    private void SweepBroom()
    {
        TimeSinceLastBroomSweep = 0f;
        sweepingBroom = true;
        StartCoroutine(PlayBroomAnimation());
    }

    private IEnumerator PlayBroomAnimation()
    {
        GetComponentInChildren<Animator>().Play("BroomSweep");
        yield return new WaitForSeconds(0.25f);
        foreach (Gravestone grave in nearbyGraves)
        {
            grave.RaiseHappiness(15f);
            if(grave.inhabitedGhost.timesGraveWasDestroyed > 0)
            {
                grave.inhabitedGhost.timesGraveWasDestroyed--;
            }
        }
        sweepingBroom = false;
    }
    private IEnumerator StartWatering()
    {
        StopCoroutine(EndWatering());
        usingWateringCan = true;
        float timeSinceLastWaterDrop = 10f;
        float timeBetweenWaterDrops = 0.03f;
        while (Input.GetKey(KeyCode.Space))
        {
            timeSinceLastWaterDrop += Time.deltaTime;
            if(_moveablePart.transform.rotation.eulerAngles.z < 45)
            {
                _moveablePart.transform.Rotate(new Vector3(0,0,4));
                yield return new WaitForFixedUpdate();
            }
            if(_moveablePart.transform.rotation.eulerAngles.z >= 44)
            {
                if(timeSinceLastWaterDrop >= timeBetweenWaterDrops)
                {
                    WateringSFX.SetActive(true);
                    Instantiate(waterParticles, waterParticlesSpawn.transform.position + new Vector3(Random.Range(-0.01f, 0.01f),0,0), new Quaternion());
                    timeSinceLastWaterDrop = 0f;
                    LevelSceneManager.instance.timeSinceLastMiscPrompt = 0f;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(EndWatering());
    }
    private IEnumerator EndWatering()
    {
        WateringSFX.SetActive(false);
        while (_moveablePart.transform.rotation.eulerAngles.z > 5)
        {
            _moveablePart.transform.Rotate(new Vector3(0, 0, -4));
            yield return new WaitForFixedUpdate();
        }
        Quaternion q = _moveablePart.transform.rotation; //rotate back to 0 degrees
        q.eulerAngles = new Vector3(0,0,0);
        _moveablePart.transform.rotation = q;

        usingWateringCan = false;
    }
    private void NextFlower()
    {
        if(flowerIndex < 3)
        {
            flowerIndex++;
            LevelSceneManager.instance._UIScript.ShowNewFlower(Flowers[flowerIndex].GetComponent<Flower>().spriteRenderer.sprite);
        }
        else
        {
            flowerIndex = 0;
            LevelSceneManager.instance._UIScript.ShowNewFlower(Flowers[flowerIndex].GetComponent<Flower>().spriteRenderer.sprite);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(tag == "Broom")
        {
            if(collision.TryGetComponent(out Gravestone grave))
            {
                nearbyGraves.Add(grave);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (tag == "Broom")
        {
            if (collision.TryGetComponent(out Gravestone grave))
            {
                nearbyGraves.Remove(grave);
            }
        }
    }
}
