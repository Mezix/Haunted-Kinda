using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public UI ui;
    public float _moveSpeed = 1f;
    private Rigidbody2D rb;
    private Animator animator;
    public GameObject GhostGlow;
    public GameObject Shadow;

    private Vector2 movement;
    private bool lockMovement;
    public float _dashDistance = 10f;
    public float dashCooldown;
    public float timeSinceLastDash;
    private bool dashing;

    private float screamCooldown;
    private float timeSinceLastScream;

    private bool isPossessing;
    private PosessableObject possessedObject;
    private float PossessionRange;

    private List<Offering> CollectedOfferings;

    public TriggerReturnCollissions DashCollider;
    public TriggerReturnCollissions ScreamCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        dashCooldown = 1;
        screamCooldown = 5;
        timeSinceLastScream = screamCooldown;
        timeSinceLastDash = dashCooldown;
        PossessionRange = 5;

        CollectedOfferings = new List<Offering>();
    }
    private void Update()
    {
        timeSinceLastScream += Time.deltaTime;
        timeSinceLastDash += Time.deltaTime;

        if(!LevelSceneManager.paused)
        {
            if (!lockMovement)
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");
            }

            if (!dashing) //if we arent dashing, continously set movement for the animations, otherwise override it
            {
                animator.SetFloat("Horizontal", movement.x);
                animator.SetFloat("Vertical", movement.y);
            }
            animator.SetFloat("Speed", movement.sqrMagnitude);

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (!dashing && timeSinceLastDash >= dashCooldown)
                {
                    timeSinceLastDash = 0;
                    StartCoroutine(Dash());
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (timeSinceLastScream >= screamCooldown)
                {
                    timeSinceLastScream = 0;
                    StartCoroutine(Scream());
                }
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!isPossessing)
                {
                    StartCoroutine(PossessObject());
                }
                else
                {
                    StartCoroutine(DepossessObject());
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
                PickUpOffering();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (CollectedOfferings.Count > 0)
                {
                    PlaceDownOffering();
                }
            }
        }
        
    }

    private void FixedUpdate()
    {
        //make movement be more floaty
        rb.MovePosition(rb.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
    }

    void Interact()
    {
        PickUpOffering();
    }
    private void PickUpOffering()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 9); 
        if (hit.collider.TryGetComponent<Offering>(out Offering offering))
        {
            if(!offering.disappearing)
            {
                CollectedOfferings.Add(offering);
                offering.gameObject.SetActive(false);
            }
        }
    }

    private void PlaceDownOffering()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 8); //the layer gravestones are on
        if (hit.collider.TryGetComponent<Gravestone>(out Gravestone grave))
        {
            Offering offering = CollectedOfferings[0];
            offering.gameObject.SetActive(true);
            offering.transform.position = grave.OfferingPos.transform.position;
            grave.Restore(offering.HealAmount);
            offering.FadeAway();
            CollectedOfferings.RemoveAt(0);
        }
    }

    private IEnumerator PossessObject()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 8); // 1 bitshift 8 is the 8th layer, where possesable things are at

        if(hit.collider)
        {
            if(hit.collider.TryGetComponent<PosessableObject>(out PosessableObject possessable))
            {
                //print("possessing!");

                if (Vector3.Distance(hit.transform.position, transform.position) <= PossessionRange)
                {
                    lockMovement = true;
                    movement = Vector2.zero; //make sure we dont move anymore
                    possessable.Possess();
                    possessedObject = possessable;
                    animator.SetBool("Disappear", true);
                    GhostGlow.SetActive(false);
                    Shadow.SetActive(false);
                    Events.current.PossessObject(possessedObject.gameObject);
                    yield return new WaitForSeconds(0.25f);
                    isPossessing = true;
                }
            }
        }
    }

    private IEnumerator DepossessObject()
    {
        //print("back to moving!");

        transform.position = possessedObject.transform.position;

        isPossessing = false;
        animator.SetBool("Disappear", false);
        GhostGlow.SetActive(true);
        Shadow.SetActive(true);
        possessedObject.Deposses();
        possessedObject = null;
        Events.current.PossessObject(gameObject);
        yield return new WaitForSeconds(0.75f);
        lockMovement = false;
    }

    private IEnumerator Scream()
    {
        ui.UIScream();

        animator.SetBool("Screaming", true);
        foreach(GraveRobber robber in ScreamCollider.GraveRobbersInCollider)
        {
            robber.TakeFearDamage(60f);
        }
        yield return new WaitForSeconds(1f);
        animator.SetBool("Screaming", false);
    }

    private IEnumerator Dash()
    {
        int framesOfTryingToMove = 0;

        dashing = true;

        ui.UIDash();

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //either move to the position or the max distance, whichever is smaller
        Vector2 maxDash = (mousePos - rb.position).normalized * _dashDistance;
        Vector2 cursorDash = (mousePos - rb.position);

        Vector2 vectorToMove;
        if (maxDash.magnitude < cursorDash.magnitude)
        {
            vectorToMove = maxDash;
        }
        else
        {
            vectorToMove = cursorDash;
        }
        Vector2 initialRbPos = rb.position;

        animator.SetBool("Dashing", dashing);
        animator.SetFloat("Horizontal", vectorToMove.x);
        animator.SetFloat("Vertical", vectorToMove.y);

        List<GraveRobber> AlreadyDamagedRobbers = new List<GraveRobber>();
        while (Vector2.Distance(rb.position, vectorToMove + initialRbPos) > 0.5f)
        {
            foreach(GraveRobber robber in DashCollider.GraveRobbersInCollider)
            {
                if(!AlreadyDamagedRobbers.Contains(robber))
                {
                    robber.TakeFearDamage(30f);
                    AlreadyDamagedRobbers.Add(robber);
                }
            }


            rb.position = Vector2.Lerp(rb.position, initialRbPos + vectorToMove, 0.1f);
            //rb.MovePosition(rb.position + (initialRbPos + vectorToMove).normalized * _moveSpeed * Time.deltaTime);
            print((initialRbPos + vectorToMove).normalized * _moveSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();

            if (framesOfTryingToMove > 15) //if we try to move enough, break out of our dash
            {
                animator.SetBool("Dashing", false);
                break;
            }
            if (movement.magnitude > 0) //if we are trying to move, tick our timer up
            {
                framesOfTryingToMove++;
            }
            else
            {
                framesOfTryingToMove = 0;
            }
        }

        animator.SetBool("Dashing", false);
        dashing = false;
    }
}
