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
    private bool canBeFearedByPlayer;
    private bool wasTerrified;
    private GameObject player;

    private List<Gravestone> AllPossibleGravestones = new List<Gravestone>();
    private Gravestone NearestGrave;

    public GameObject LootBag;
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

    public void StopFearNearPlayer(GameObject possessed)
    {
        if(!possessed.Equals(player)) //if we are possessing anything than isnt the player, dont get feared by the players presence anymore
        {
            canBeFearedByPlayer = false;
        }
        else
        {
            canBeFearedByPlayer = true;
        }
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
            movement.x = 1;
            rb.MovePosition(rb.position + movement.normalized * 3 * _moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
            UpdateFear();
        }
    }

    public void InitAllGravestones(List<Gravestone> graves)
    {
        AllPossibleGravestones = graves;
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
            if(FindNearestGravestone()) //repeat if we have another gravestone
            {
                FindNearestGravestone();
                StartCoroutine(MoveToNearestGrave());
            }
            else
            {
                StartCoroutine(RunAwayWithLoot());
            }
        }
    }

    private IEnumerator RunAwayWithLoot()
    {
        //print("hehe suckers");
        animator.SetBool("IsEscaping", true);
        while(!wasTerrified)
        {
            movement = new Vector2(1, 0);
            yield return new WaitForFixedUpdate();
            if (CheckEscape()) //check to see if we ran away safely
            {
                EscapeAnimation();
            }
        }
        if(wasTerrified)
        {
            DropLoot();
        }
    }
    void DropLoot()
    {
        print("drop it and go boys");
        Instantiate(LootBag, transform.position, transform.rotation);
    }

    bool CheckEscape() //if we make it to the fence, were scott free
    {
        return false;
    }

    void EscapeAnimation()
    {

    }
    private void UpdateFear()
    {
        if(Vector2.Distance(player.transform.position, transform.position) <= 3 && canBeFearedByPlayer) //if close to player, continously add fear
        {
            bool runAway = fear.AddFear(1f);
            if(runAway)
            {
                wasTerrified = runAway;
                
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
        animator.SetBool("Digging", false);
        //print("AHHHHHHHHHHHHHHHHHHHHHHHHH");
        yield return new WaitForSeconds(10f);
        Events.current.DespawnGraveRobber(gameObject);
    }
}
