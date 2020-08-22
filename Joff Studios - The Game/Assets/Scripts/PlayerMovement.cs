using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float _moveSpeed = 1f;
    private Rigidbody2D rb;
    private Animator animator;

    [SerializeField]
    private Vector2 movement;
    public float _dashDistance = 10f;
    private bool dashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    private void Start()
    {

    }
    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

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
    }

    private void FixedUpdate()
    {
        //make movement be more floaty
        rb.MovePosition(rb.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator Dash()
    {
        print(dashing);
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

        dashing = false;
        animator.SetBool("Dashing", dashing);
    }
}
