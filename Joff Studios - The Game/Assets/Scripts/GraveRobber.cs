using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveRobber : MonoBehaviour
{
    public GameObject Player;

    public float _moveSpeed = 1f;
    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 movement;
    private bool moving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        _moveSpeed = 2f;
        movement = new Vector2(0, 0);

        Physics2D.IgnoreCollision(Player.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    void Update()
    {
        if (!moving)
        {
            EnemyBehaviour();
        }
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
    }
    public void EnemyBehaviour()
    {
        StartCoroutine(SwitchMovement());
    }
    private IEnumerator SwitchMovement()
    {
        moving = true;

        print("walk");
        movement.x = 1;
        yield return new WaitForSeconds(1f);
        movement.x = -1;
        print(movement);
        yield return new WaitForSeconds(1f);

        moving = false;
    }
}
