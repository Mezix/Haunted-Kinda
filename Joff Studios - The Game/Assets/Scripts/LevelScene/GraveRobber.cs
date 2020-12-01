using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class GraveRobber : MonoBehaviour
{
    //MOVEMENT

    public float _moveSpeed = 1f;

    //REFERNCES TO OBJECTS ON OUR ROBBER

    private Rigidbody2D robberRB;
    private Animator animator;
    private SpriteRenderer graverobberRenderer; //the sprite renderer of our robber
    public GameObject UIRobberPrefab;
    private GameObject UIRobberInstance;

    //FEAR

    public FearLevel fearLevel { get; private set; } //the fear script on our player
    private bool canBeFeared; //wether or not the robber can be feared by the player
    public bool isTerrified { get; set;} //if our robber hits max fear, this should be true
    public GameObject _fearParticles;

    //MISC

    private GameObject player; //reference to the player in the scene
    private List<Gravestone> allPossibleGravestones = new List<Gravestone>(); //all gravestones in the scene, for pathfinding purposes
    public List<Gravestone> blockedGraves = new List<Gravestone>();
    private Gravestone nearestGrave; //the gravestone we will initially seek out
    public GameObject _lootBagPrefab; //the prefab of our lootbag that we spawn when we are terrifed
    private GameObject escapePosition; //the position we will flee to if terrified
    private bool hasLoot;
    private GameObject currentLoot = null;
    private bool isDigging; //determines wether we are currently digging
    private bool isEscaping;
    public bool lockMovement;

    //PATHFINDING

    private Seeker seeker; //the seeker component which creates paths
    private Path path; //the path our unit needs to take
    private int currentWaypoint = 0; //the index of our path.vectorPath
    private float nextWayPointDistance = 0.5f; //the distance before we seek out our next waypoint => the higher, the smoother the movement
    private bool reachedEndOfPath = false; //wether or not we have gotten to our last checkpoint
    private GameObject positionToSeekOut; //the gameobject we are currently looking to get to (e.g. escapePos, nearestGrave)

    private void Awake()
    {
        player = References.Player;
        fearLevel = GetComponent<FearLevel>();
        robberRB = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        graverobberRenderer = GetComponentInChildren<SpriteRenderer>();
        seeker = GetComponent<Seeker>();

        UIRobberInstance = Instantiate(UIRobberPrefab, Vector3.zero, Quaternion.identity);
        UIRobberInstance.transform.parent = gameObject.transform;
        UIRobberInstance.SetActive(false);
        if(_fearParticles)
        {
            _fearParticles.SetActive(false);
        }
    }

    void Start()
    {
        Events.current.ObjectPossessed += StopFearNearPlayer; //subscribe to the possession event, so that if the player possesses something, we arent taking damage from the invisible player

        canBeFeared = true; //by default, our robber should be fearable
        _moveSpeed = 2f;
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>()); //dont collide with the player
        if (fearLevel)
        {
            fearLevel.InitMaxFear(100); //init our max fear level
        }

        FindNearestGravestone(); //find a gravestone initially
        if (!nearestGrave) //if no grave is available, continously check to see if we can access one
        {
            InvokeRepeating("Idle", 0f, .5f);
        }
    }
    void Update()
    {
        if (path != null) //as long as we have a path to follow, we have movement, and can set our animator values accordingly
        {
            animator.SetFloat("Horizontal", path.vectorPath[currentWaypoint].x - transform.position.x); //constantly give our animator values so we can show the right movement animation
            animator.SetFloat("Vertical", path.vectorPath[currentWaypoint].y - transform.position.y);
            animator.SetFloat("Speed", (transform.position - path.vectorPath[currentWaypoint]).sqrMagnitude);
        }

        Vector3 screenPoint = Camera.main.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        if(!onScreen && !isEscaping)
        {
            UIRobberInstance.SetActive(true);
            SetUIRobberLocation();
        }
        else
        {
            UIRobberInstance.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (nearestGrave)
        {
            CancelInvoke("Idle"); //stop us idling/finding a new gravestone
        }
        if (!lockMovement)
        {
            GraveRobberBehaviour();
        }
        if (isTerrified) //if we have been terrified, set our bools appropriately for the animator
        {
            animator.SetBool("IsEscaping", false);
            animator.SetBool("Digging", false);
        }
        else
        {
            ReduceFearWhenFarAway(); //otherwise continously reduce our fear over time if we are far enough from the player
        }
    }

    public void InitRobber(List<Gravestone> graves, GameObject escapePos) //sets our escape position for the robber and all gravestones in the scene
    {
        allPossibleGravestones = graves;
        escapePosition = escapePos;
    }
    private void GraveRobberBehaviour() //this script handles all our graverobbers actions
    {
        if(!reachedEndOfPath) //if we havent reached the end of our path, always move to our paths location
        {
            MoveOnPath();
        }
        else if (reachedEndOfPath && !hasLoot && !isDigging && !isTerrified) //if we reached the end of our Path, and dont have loot, we must be at a grave, so start digging
        {
            StartCoroutine(DigGrave());
        }

        if (EscapePossible() && !isTerrified) //if we are at the escapePosition, and have loot, were home free!
        {
            StartCoroutine(EscapeWithLootAnimation());
        }
    }

    private void Idle()
    {
        print("just chilling");
        FindNearestGravestone();
    }
    private void MoveOnPath()
    {
        if(!reachedEndOfPath) //if we reached the end, just stop
        {
            if (path == null) //stop the method if we dont even have a path
            {
                return;
            }

            Vector2 vectorToMove = ((Vector2)path.vectorPath[currentWaypoint] - robberRB.position).normalized * _moveSpeed * Time.deltaTime;

            float distance = Vector2.Distance(robberRB.position, path.vectorPath[currentWaypoint]);

            //TODO: Problem: unit can overshoot target

            robberRB.MovePosition(robberRB.position + vectorToMove);

            if((distance < nextWayPointDistance) && !(currentWaypoint == path.vectorPath.Count-1))
            {
                currentWaypoint++;
            }
            else
            {
                if(distance < 0.1f)
                {
                    robberRB.position = path.vectorPath[currentWaypoint];
                    reachedEndOfPath = true;
                }
            }
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
    private void UpdatePath(GameObject newPos) //resets everything related to the path, then creates a new one
    {
        currentWaypoint = 0;
        reachedEndOfPath = false;
        if(seeker.IsDone())
        {
            seeker.StartPath(transform.position, newPos.transform.position, OnPathComplete);
        }
    }

    //FEAR STUFF

    private void ReduceFearWhenFarAway() //if we are far from the player, reduce our fear continously
    {
        if (Vector2.Distance(player.transform.position, transform.position) >= 6 && !canBeFeared) //if close to player, continously add fear
        {
            fearLevel.ReduceFear(0.1f);
        }
    }
    public void TakeFearDamage(float dmg)
    {
        if (canBeFeared)
        {
            bool runAway = fearLevel.AddFear(dmg); //if we take enough damage...
            if (runAway)
            {
                StartCoroutine(Flee()); //...flee
            }
        }
    }

    public void StopFearNearPlayer(GameObject possessed) //called by the possession event
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

    //GRAVESTONE STUFF

    private bool FindNearestGravestone() //sets the reference to the nearest gravestone
    {
        bool undestroyedGraveAvailable = false; //checks to see if we all gravestones have been destroyed or not
        float Distance = 100;
        foreach (Gravestone grave in allPossibleGravestones) //check all possible gravestones
        {
            if(!grave._destroyed && !grave.IsBeingTargeted && !blockedGraves.Contains(grave)) //as long as the grave we are looking at isnt destroyed and not focused by another robber
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
        if(undestroyedGraveAvailable) //only if we exited the previous loop having found a grave...
        {
            nearestGrave.IsBeingTargeted = true; //attack that grave, so others cant
            UpdatePath(positionToSeekOut); //update our path to this gravestone
        }
        return undestroyedGraveAvailable; //return wether or not undestroyed gravestones are actually available
    }

    private IEnumerator DigGrave()
    {
        isDigging = true; //stop us digging multiple times
        animator.SetBool("Digging", true); //set the animator bool so we start digging
        nearestGrave.AttackGrave();

        while(!nearestGrave._destroyed) //as long as the grave we are targeting isnt destroyed, keep digging
        {
            //robberRB.position = currentPos; //stay in place, dont get bumped by other robbers
            //robberRB.velocity = Vector2.zero;
            GetComponent<Collider2D>().isTrigger = true; //stop all collisions
            if (isTerrified) //if weve gotten terrified, break the digging loop
            {
                animator.SetBool("Digging", false);
                break;
            }
            nearestGrave.TakeDamage(1f);
            nearestGrave.LowerHappiness(1f);
            yield return new WaitForSeconds(0.1f);
        }
        if(!isTerrified) //if we exited the loop and werent terrified, runaway with our loot!
        {
            animator.SetBool("Digging", false);
            hasLoot = true;
            nearestGrave.GetComponentInChildren<GraveGhost>().lootStolen = true;
            StartCoroutine(RunAwayWithLoot());
        }

        GetComponent<Collider2D>().isTrigger = false; //start collisions again
        nearestGrave.StopAttackingGrave(); //stop attacking the grave, freeing up others to potentially attack it
        isDigging = false;
        positionToSeekOut = escapePosition; //set our target as the escape position, and update our path
        UpdatePath(positionToSeekOut);
    }

    private IEnumerator RunAwayWithLoot()
    {
        animator.SetBool("IsEscaping", true);
        while(!isTerrified) //as long as we aren't terrified, continue on
        {
            yield return new WaitForFixedUpdate();
        }
        if(isTerrified) //if we exit the loop by being terrified, drop the loot
        {
            DropLoot();
        }
    }
    void DropLoot() //spawn a lootbag
    {
        GameObject go = Instantiate(_lootBagPrefab, transform.position, transform.rotation);
        if (nearestGrave.inhabitedGhost._graveItem)
        {
            go.GetComponent<Lootbag>().lootPrefab = nearestGrave.inhabitedGhost._graveItem;
        }
    }
    public IEnumerator Flee()
    {
        isEscaping = true;
        _moveSpeed = 4f; //triple our movespeed
        isTerrified = true; //set the terrified bool to true
        if(nearestGrave)
        {
            nearestGrave.StopAttackingGrave(); //stop attacking the nearest grave
        }
        graverobberRenderer.color = new Color(156,0,255,255); //Change color of robber to indicate fear
        _fearParticles.SetActive(true); //show fear particles
        positionToSeekOut = escapePosition; //set the position and update the path
        UpdatePath(positionToSeekOut);
        animator.SetBool("Digging", false); //stop digging if we were
        while (!EscapePossible()) //wait until an escape is possible...
        {
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(FleeAnimation()); //...then flee
    }

    private bool EscapePossible() //check if we can escape, depending on how far we are from the escape position
    {
        if (Vector2.Distance(transform.position, escapePosition.transform.position) <= 0.5f)
        {
            return true;
        }
        return false;
    }
    private IEnumerator FleeAnimation()
    {
        GetComponent<Collider2D>().isTrigger = true; //stop all collisions
        for (int i = 200; i > 0; i--) //lower our opacity continously
        {
            graverobberRenderer.color = new Color(1, 1, 1, i / 200f);
            yield return new WaitForSeconds(0.001f);
        }
        Events.current.DespawnGraveRobber(gameObject); //and disappear
    }
    private IEnumerator EscapeWithLootAnimation()
    {
        isEscaping = true;
        canBeFeared = false; //stop us from being feared while we are already free
        fearLevel.ReduceFear(100f); //reset our fear, which at the same time hides the healthbar
        animator.SetBool("HasEscaped", true); //set our animation to the escape!

        Vector2 currentPos = robberRB.position; //stop us being pushed around
        GetComponent<Collider2D>().isTrigger = true; //stop all collisions
        for(int i = 500; i > 0; i--)
        {
            robberRB.position = currentPos;
            graverobberRenderer.color = new Color(1, 1, 1, i/500f);
            yield return new WaitForSeconds(0.001f);
        }
        Events.current.DespawnGraveRobber(gameObject); //finally destroy our grave robber through the level scenemanager
    }

    public void BlockGrave(Gravestone grave)
    {
        if(!blockedGraves.Contains(grave))
        {
            blockedGraves.Add(grave);
        }
        if(nearestGrave.Equals(grave))
        {
            nearestGrave.StopAttackingGrave();
            nearestGrave = null;
            isDigging = false;
            animator.SetBool("Digging", false);
            StopAllCoroutines();
            FindNearestGravestone();
        }
    }

    public void UnblockGrave(Gravestone grave)
    {
        if(blockedGraves.Contains(grave))
        {
            blockedGraves.Remove(grave);
        }
    }

    private void SetUIRobberLocation()
    {
        float maxScreenWidth = Screen.width / 2f;
        float maxScreenHeight = Screen.height / 2f;
        float featherAmount = 50f;

        Vector2 robberLocation = Camera.main.WorldToScreenPoint(gameObject.transform.position) - new Vector3(maxScreenWidth, maxScreenHeight, 0);

        if (robberLocation.x >= maxScreenWidth)
        {
            robberLocation.x = maxScreenWidth - featherAmount;
        }
        else if(robberLocation.x <= -maxScreenWidth)
        {
            robberLocation.x = -maxScreenWidth + featherAmount;
        }
        if (robberLocation.y >= maxScreenHeight)
        {
            robberLocation.y = maxScreenHeight - featherAmount;
        }
        if (robberLocation.y <= -maxScreenHeight)
        {
            robberLocation.y = -maxScreenHeight + featherAmount;
        }
        UIRobberInstance.GetComponent<UIRobber>().rect.anchoredPosition = robberLocation;
    }
}
