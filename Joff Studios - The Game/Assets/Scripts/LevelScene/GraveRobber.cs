using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveRobber : MonoBehaviour
{

    public float _moveSpeed = 1f;
    private Vector2 movement;
    private bool moving;
    private Rigidbody2D rb;
    private Animator animator;

    private FearLevel fear;
    private bool isRunning;
    private GameObject player;

    private void Awake()
    {
        player = GameObject.Find("PlayerCharacter");
        fear = GetComponent<FearLevel>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        _moveSpeed = 2f;
        movement = new Vector2(0, 0);
        if(fear)
        {
            fear.InitMaxFear(100);
        }
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    void Update()
    {
        if (!moving && !isRunning)
        {
            EnemyBehaviour();
        }
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

    }
    private void FixedUpdate()
    {
        if (isRunning)
        {
            movement.x = 1;
            rb.MovePosition(rb.position + movement.normalized * 3 * _moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
            UpdateFear();
        }
    }

    private void UpdateFear()
    {
        if(Vector2.Distance(player.transform.position, transform.position) <= 3) //if close to player, continously add fear
        {
            bool runAway = fear.AddFear(1f);
            if(runAway)
            {
                isRunning = runAway;
                
                StartCoroutine(RunAway());
            }
        }
        else
        {
            fear.ReduceFear(1f);
        }
    }

    public IEnumerator RunAway()
    {
        print("AHHHHHHHHHHHHHHHHHHHHHHHHH");
        yield return new WaitForSeconds(1f);
        Events.current.DespawnGraveRobber(gameObject);
    }

    public void EnemyBehaviour()
    {
        StartCoroutine(SwitchMovement());
    }
    private IEnumerator SwitchMovement()
    {
        moving = true;

        movement.x = 1;
        yield return new WaitForSeconds(1f);
        movement.x = -1;
        yield return new WaitForSeconds(1f);

        moving = false;
    }
}
