using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GraveRobber : MonoBehaviour
{
    //MOVEMENT

    public float _moveSpeed = 1f;
    private Vector2 movement;
    private bool moving; //a bool determíning if we should move or not

    //REFERNCES TO OBJECTS ON OUR ROBBER

    private Rigidbody2D robberRB;
    private Animator animator;
    private SpriteRenderer rend; //the sprite renderer of our robber

    //FEAR

    private FearLevel fear; //the fear script on our player
    private bool canBeFeared; //wether or not the robber can be feared by the player
    private bool wasTerrified; //if our robber hits max fear, this should be true

    //MISC

    private GameObject player; //reference to the player in the scene
    private List<Gravestone> AllPossibleGravestones = new List<Gravestone>(); //all gravestones in the scene, for pathfinding purposes
    private Gravestone nearestGrave; //the gravestone we will initially seek out
    public GameObject _lootBagPrefab; //the prefab of our lootbag that we spawn when we are terrifed
    private GameObject escapePosition; //the position we will flee to if terrified

    //PATHFINDING

    private Path path;
    private int currentWaypoint = 0;
    private float nextWayPointDistance = 3f;
    private bool reachedEndOfPath = false;
    private Seeker seeker;
    private GameObject positionToSeekOut;

    private void Awake()
    {
        player = GameObject.Find("PlayerCharacter"); //get all references
        fear = GetComponent<FearLevel>();
        robberRB = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        rend = GetComponentInChildren<SpriteRenderer>();
        seeker = GetComponent<Seeker>();
    }

    void Start()
    {
        Events.current.ObjectPossessed += StopFearNearPlayer; //subscribe to the possession event, so that if the player possesses something, we arent taking damage from the invisible player

        canBeFeared = true; //by default, our robber should be fearable
        _moveSpeed = 2f;
        movement = new Vector2(0, 0); //start off with 0 movement
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>()); //dont collide with the player
        if (fear)
        {
            fear.InitMaxFear(100); //init our max fear level
        }
        FindNearestGravestone(); //when we spawn, seek out the nearest gravestone, get the reference...

        //StartCoroutine(MoveToNearestGrave()); //...and then move there

        //InvokeRepeating("UpdatePath", 0f, .5f);
    }
    void Update()
    {
        
        animator.SetFloat("Horizontal", movement.x); //constantly give our animator values so we can show the right movement animation
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);

    }
    private void FixedUpdate()
    {
        if (wasTerrified) //if we have been terrified, move at 3x speed
        {
            animator.SetBool("IsEscaping", false); //and set our bools appropriately for the animator
            animator.SetBool("Digging", false);
            MoveOnPath();
        }
        else
        {
            MoveOnPath();
            ReduceFearWhenFarAway();
        }
    }

    private void MoveOnPath()
    {
        if (path == null)
        {
            return;
        }
        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - robberRB.position).normalized;
        Vector2 force = direction * _moveSpeed * Time.deltaTime;

        float distance = Vector2.Distance(robberRB.position, path.vectorPath[currentWaypoint]);

        //TODO: Problem: unit can overshoot target

        robberRB.MovePosition(robberRB.position + force);

        if (distance < nextWayPointDistance && !(currentWaypoint == path.vectorPath.Count - 1)) //if we are not on our last way point
        {
            currentWaypoint++;
        }
    }
    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
    private void UpdatePath()
    {
        if(seeker.IsDone())
        {
            print(positionToSeekOut);
            seeker.StartPath(transform.position, positionToSeekOut.transform.position, OnPathComplete);
        }
    }
    
    public void InitRobber(List<Gravestone> graves, GameObject escapePos) //sets our escape position for the robber and all gravestones in the scene
    {
        AllPossibleGravestones = graves;
        escapePosition = escapePos;
    }

    //FEAR STUFF

    private void ReduceFearWhenFarAway()
    {
        if (Vector2.Distance(player.transform.position, transform.position) >= 6 && !canBeFeared) //if close to player, continously add fear
        {
            fear.ReduceFear(0.1f);
        }
    }
    public void TakeFearDamage(float dmg)
    {
        if (canBeFeared)
        {
            bool runAway = fear.AddFear(dmg);
            if (runAway)
            {
                StartCoroutine(RunAway());
            }
        }
    }

    public void StopFearNearPlayer(GameObject possessed)
    {
        if (!possessed.Equals(player)) //if we are possessing anything than isnt the player, dont get feared by the players presence anymore
        {
            canBeFeared = false;
        }
        else
        {
            canBeFeared = true;
        }
    }

    //GRAVESTONE PATHFINDING

    private bool FindNearestGravestone() //find the closest (undestroyed) gravestone and set the reference
    {
        bool undestroyedGraveAvailable = false; //checks to see if we all gravestones have been destroyed or not
        float Distance = 100;
        foreach (Gravestone grave in AllPossibleGravestones) //check all possible gravestones
        {
            if(!grave.Destroyed) //as long as the grave we are looking at isnt destroyed...
            {
                if(Vector2.Distance(grave.transform.position, robberRB.transform.position) <= Distance) //..and the distance is closer than the last grave we checked
                {
                    nearestGrave = grave; //override our nearest grave
                    positionToSeekOut = nearestGrave.gameObject;
                    Distance = Vector2.Distance(grave.transform.position, robberRB.transform.position);
                }
                undestroyedGraveAvailable = true; //change the value so we know we can actually find a new gravestone
            }
        }
        UpdatePath();
        return undestroyedGraveAvailable; //return wether or not undestroyed gravestones are actually available
    }
    private IEnumerator MoveToNearestGrave()
    {
        if(nearestGrave)
        {
            Vector2 NearGravePos = new Vector2(nearestGrave.transform.position.x, nearestGrave.transform.position.y) + new Vector2(0.5f, -0.25f);
            while (Vector2.Distance(robberRB.position, NearGravePos) > 0.1f)
            {
                movement = (NearGravePos - robberRB.position).normalized;
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
        while(!nearestGrave.Destroyed)
        {
            if(wasTerrified)
            {
                animator.SetBool("Digging", false);
                break;
            }
            nearestGrave.TakeDamage(0.3f);
            yield return new WaitForSeconds(0.1f);
        }
        if(!wasTerrified)
        {
            animator.SetBool("Digging", false);
            StartCoroutine(RunAwayWithLoot());
        }
    }

    private IEnumerator RunAwayWithLoot()
    {
        //print("hehe suckers");
        animator.SetBool("IsEscaping", true);
        while(!wasTerrified)
        {
            movement = (escapePosition.transform.position - transform.position).normalized;
            if (EscapePossible()) //check to see if we ran away safely
            {
                StartCoroutine(EscapeWithLootAnimation());
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
        Instantiate(_lootBagPrefab, transform.position, transform.rotation);
    }

    bool EscapePossible()
    {
        if (Vector2.Distance(transform.position, escapePosition.transform.position) <= 2f)
        {
            return true;
        }
        return false;
    }

    IEnumerator EscapeWithLootAnimation()
    {
        canBeFeared = false;
        //print("cya suckers");
        animator.SetBool("HasEscaped", true);
        for(int i = 100; i > 0; i--)
        {
            rend.color = new Color(1, 1, 1, i/100f);
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(gameObject);
    }

    IEnumerator FleeAnimation()
    {
        for (int i = 100; i > 0; i--)
        {
            rend.color = new Color(1, 1, 1, i / 100f);
            yield return new WaitForSeconds(0.005f);
        }
        Destroy(gameObject);
    }
    

    public IEnumerator RunAway()
    {
        _moveSpeed = 6f;
        wasTerrified = true;
        positionToSeekOut = escapePosition;
        UpdatePath();
        animator.SetBool("Digging", false);

        while(!EscapePossible())
        {
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(FleeAnimation());
        Events.current.DespawnGraveRobber(gameObject);
    }
}
