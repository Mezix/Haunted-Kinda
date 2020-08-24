using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveRobber : MonoBehaviour
{
    public SpriteRenderer rend;

    public float _moveSpeed = 1f;
    private Vector2 movement;
    private bool moving;
    private Rigidbody2D rb;
    private Animator animator;

    private FearLevel fear;
    private bool canBeFearedByPlayer;
    private bool wasTerrified;
    private GameObject player;

    private List<Gravestone> AllPossibleGravestones = new List<Gravestone>();
    private Gravestone NearestGrave;

    public GameObject LootBag;

    public GameObject EscapePosition;
    private void Awake()
    {
        player = GameObject.Find("PlayerCharacter");
        fear = GetComponent<FearLevel>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        Events.current.ObjectPossessed += StopFearNearPlayer;

        canBeFearedByPlayer = true;
        _moveSpeed = 2f;
        movement = new Vector2(0, 0);
        if(fear)
        {
            fear.InitMaxFear(100);
        }
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());

        FindNearestGravestone();
        StartCoroutine(MoveToNearestGrave());
    }


    void Update()
    {
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

    }
    private void FixedUpdate()
    {
        if (wasTerrified)
        {
            animator.SetBool("IsEscaping", false);
            animator.SetBool("Digging", false);
            rb.MovePosition(rb.position + movement.normalized * 3 * _moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
            ReduceFearWhenFarAway();
        }
    }

    public void Init(List<Gravestone> graves, GameObject escapePos)
    {
        AllPossibleGravestones = graves;
        EscapePosition = escapePos;
        //print("all the graves!");
    }
    private bool FindNearestGravestone()
    {
        bool undestroyedGraveAvailable = false;
        float Distance = 100;
        //print("finding grave");
        foreach (Gravestone grave in AllPossibleGravestones)
        {
            if(!grave.Destroyed)
            {
                if(Vector2.Distance(grave.transform.position, rb.transform.position) <= Distance)
                {
                    NearestGrave = grave;
                    Distance = Vector2.Distance(grave.transform.position, rb.transform.position);
                }
                undestroyedGraveAvailable = true;
                //print(NearestGrave);
            }
        }
        return undestroyedGraveAvailable;
    }
    private IEnumerator MoveToNearestGrave()
    {
        //print("moving to grave");
        if(NearestGrave)
        {
            Vector2 NearGravePos = new Vector2(NearestGrave.transform.position.x, NearestGrave.transform.position.y);
            while (Vector2.Distance(rb.position, NearGravePos) > 0.5f)
            {
                movement = (NearGravePos - rb.position).normalized;
                yield return new WaitForFixedUpdate();
            }
            if(!wasTerrified)
            {
                movement = Vector2.zero;
                StartCoroutine(DigGrave());
            }
        }
    }

    private IEnumerator DigGrave()
    {
       //print("i am a dwarf and im digging a holeeeeeeeee");
        animator.SetBool("Digging", true);
        while(!NearestGrave.Destroyed)
        {
            if(wasTerrified)
            {
                animator.SetBool("Digging", false);
                break;
            }
            NearestGrave.TakeDamage(1f);
            yield return new WaitForSeconds(0.1f);
        }
        if(!wasTerrified)
        {
            animator.SetBool("Digging", false);
            //if(FindNearestGravestone()) //repeat if we have another gravestone
            //{
            //    FindNearestGravestone();
            //    StartCoroutine(MoveToNearestGrave());
            //}
            //else
            //{
                StartCoroutine(RunAwayWithLoot());
            //}
        }
    }

    private IEnumerator RunAwayWithLoot()
    {
        //print("hehe suckers");
        animator.SetBool("IsEscaping", true);
        while(!wasTerrified)
        {
            movement = (EscapePosition.transform.position - transform.position).normalized;
            if (EscapePossible()) //check to see if we ran away safely
            {
                EscapeWithLootAnimation();
            }
            yield return new WaitForFixedUpdate();
        }
        if(wasTerrified)
        {
            DropLoot();
        }
    }
    void DropLoot()
    {
        Instantiate(LootBag, transform.position, transform.rotation);
    }

    bool EscapePossible() //if we make it to the fence, were scott free
    {
        //print(Vector2.Distance(transform.position, EscapePosition.transform.position));
        if (Vector2.Distance(transform.position, EscapePosition.transform.position) <= 2f)
        {
            return true;
        }
        return false;
    }

    void EscapeWithLootAnimation()
    {
        print("cya suckers");

        Destroy(gameObject);
    }

    IEnumerator FleeAnimation()
    {
        rend.color = new Color(1, 1, 1, 0.5f);
        yield return new WaitForSeconds(5f);
        print("deleete");
        Destroy(gameObject);
    }
    private void ReduceFearWhenFarAway()
    {
        if(Vector2.Distance(player.transform.position, transform.position) >= 6) //if close to player, continously add fear
        {
            fear.ReduceFear(0.1f);
        }
    }
    public void TakeFearDamage(float dmg)
    {
        if(canBeFearedByPlayer)
        {
            bool runAway = fear.AddFear(dmg);
            if (runAway)
            {
                wasTerrified = runAway;

                StartCoroutine(RunAway());
            }
        }
    }

    public void StopFearNearPlayer(GameObject possessed)
    {
        if (!possessed.Equals(player)) //if we are possessing anything than isnt the player, dont get feared by the players presence anymore
        {
            canBeFearedByPlayer = false;
        }
        else
        {
            canBeFearedByPlayer = true;
        }
    }

    public IEnumerator RunAway()
    {
        animator.SetBool("Digging", false);
        print("AHHHHHHHHHHHHHHHHHHHHHHHHH");
        float time = 0f;
        while(!EscapePossible() && time <= 10f)
        {
            time += Time.deltaTime;
            movement = (EscapePosition.transform.position - transform.position).normalized;
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(FleeAnimation());
        Events.current.DespawnGraveRobber(gameObject);
    }
}
