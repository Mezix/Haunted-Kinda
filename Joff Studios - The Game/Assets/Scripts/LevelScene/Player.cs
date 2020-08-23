using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float _moveSpeed = 1f;
    private Rigidbody2D rb;
    private Animator animator;

    [SerializeField]
    private Vector2 movement;
    private bool lockMovement;
    public float _dashDistance = 10f;
    private bool dashing;

    private float screamCooldown;
    private float timeSinceLastScream;

    private bool isPossessing;
    private PosessableObject possessedObject;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        screamCooldown = 5;
        timeSinceLastScream = screamCooldown;
    }
    private void Update()
    {
        timeSinceLastScream += Time.deltaTime;

        if(!lockMovement)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }

        if(!dashing) //if we arent dashing, continously set movement for the animations, otherwise override it
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }
        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(!dashing)
            {
                StartCoroutine(Dash());
            }
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(timeSinceLastScream >= screamCooldown)
            {
                timeSinceLastScream = 0;
                StartCoroutine(Scream());
            }
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(!isPossessing)
            {
                PossessObject();
            }
            else
            {
                DepossessObject();
            }
        }
    }

    private void FixedUpdate()
    {
        //make movement be more floaty
        rb.MovePosition(rb.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
    }

    private void PossessObject()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if(hit.collider)
        {
            if(hit.collider.TryGetComponent<PosessableObject>(out PosessableObject possessable))
            {
                //print("possessing!");

                movement = Vector2.zero; //make sure we dont move anymore
                isPossessing = true;
                lockMovement = true;
                possessable.Possess();
                possessedObject = possessable;
                animator.SetBool("Disappear", true);
            }
        }
    }

    private void DepossessObject()
    {
        //print("back to moving!");

        transform.position = possessedObject.transform.position;

        isPossessing = false;
        lockMovement = false;
        animator.SetBool("Disappear", false);
        possessedObject.Deposses();
        possessedObject = null;
    }

    private IEnumerator Scream()
    {
        animator.SetBool("Screaming", true);
        yield return new WaitForSeconds(1f);
        animator.SetBool("Screaming", false);
    }

    private IEnumerator Dash()
    {
        int framesOfTryingToMove = 0;

        dashing = true;
        

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        //either move to the position or the max distance, whichever is smaller
        Vector2 vectorToMove = Vector2.Min(((mousePos - rb.position).normalized * _dashDistance), (mousePos - rb.position));
        Vector2 initialRbPos = rb.position;

        animator.SetBool("Dashing", dashing);
        animator.SetFloat("Horizontal", vectorToMove.x);
        animator.SetFloat("Vertical", vectorToMove.y);

        while (Vector2.Distance(rb.position, vectorToMove + initialRbPos) > 0.5f)
        {
            rb.position = Vector2.Lerp(rb.position, initialRbPos + vectorToMove, 0.1f);
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
