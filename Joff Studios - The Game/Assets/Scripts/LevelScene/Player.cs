using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //REFERENCES TO OBJECTS ON PLAYER

    public GameObject _screamObj; //the object containing our scream animation
    private Animator screamAnim; //the animator component of our scream
    private Rigidbody2D playerRB; //the rigidbody component for our player
    private Animator playerAnimator; //the animator component for our player
    public GameObject _ghostGlow; //the Glowy Background of our Ghost
    public GameObject _shadow; //the shadow below our ghost

    //MOVEMENT AND ABILITIES

    public float _moveSpeed = 1f; 
    private Vector2 movement; //the vector that controls where our player will move to
    private bool lockMovement; //stops our x and y movement vector from being changed
    public float _dashDistance = 10f; //the distance in units that we will dash
    public float _dashCooldown; //the cooldown of our dash in seconds
    public float _dashDamage = 30f;
    public float TimeSinceLastDash { get; private set; } //time elapsed since we last dashed, to determine when we can dash next
    private bool dashing; //check if we are dashing, so we can't dash again immediately

    public float _screamCooldown; //the cooldown of our scream
    public float _screamDamage = 50f;
    public float TimeSinceLastScream { get; private set; } //time elapsed since we last screamed, to determine when we can scream next

    //POSSESSION

    private bool isPossessing; //checks if we are possesing any object
    private PosessableObject possessedObject; //the script of the object were possessing
    public float _possessionRange; //the range at which we can start to possess objects

    //INVENTORY
    private List<Offering> collectedOfferings = new List<Offering>(); //all of the offerings we have collected and can place down

    //COLLIDERS FOR FEARING

    public TriggerReturnCollissions _dashCollider; //the script of two objects that contain colliders, 
    public TriggerReturnCollissions _screamCollider; //which simply returns all enemies in them so we can fear them

    //MISC STUFF
    public Collider2D _waterTilemap; //the water is collidable, but our ghost should be able to float over it, so we need to ignore collisions with this layer!
    public UIScript _ui;

    //SOUND

    public AudioSource _dashSound; //audio sources that the player needs, located within empty gameobjects with the same names 
    public AudioSource _screamSound; //that house only these sounds

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>(); //get all the references to the private gameobjects we need
        playerAnimator = GetComponent<Animator>();
        screamAnim = _screamObj.GetComponentInChildren<Animator>();

        Physics2D.IgnoreCollision(_waterTilemap, GetComponent<Collider2D>()); //make sure we dont collide with the water
    }
    private void Start()
    {
        _dashCooldown = 1; //set our starting values
        _dashDistance = 5;
        _screamCooldown = 5;
        _possessionRange = 20;

        TimeSinceLastScream = _screamCooldown; //make sure we can scream and dash right away
        TimeSinceLastDash = _dashCooldown;

    }
    private void Update()
    {
        TimeSinceLastScream += Time.deltaTime; //tick up our timers for our dash and scream
        TimeSinceLastDash += Time.deltaTime;

        if(!LevelSceneManager.paused) //if the game is paused we shoudnt even be able to make any inputs
        {
            if (!lockMovement) //if our movement is locked, for example through possesion, we shouldnt be able to change the movement vector
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");
            }
            if (!dashing) //if we aren't dashing, continously set movement for the animations, otherwise override it
            {
                playerAnimator.SetFloat("Horizontal", movement.x);
                playerAnimator.SetFloat("Vertical", movement.y);
            }
            playerAnimator.SetFloat("Speed", movement.sqrMagnitude); // continously set the speed, so our animator can switch states as required

            if(!isPossessing) // if we are possesing, dont let us use our abilites (which wouldnt do aynthing, but set them on cooldown)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift)) //Dash
                {
                    // as long as we arent already dashing, and weve waited enough since the last time, we can dash
                    if (!dashing && TimeSinceLastDash >= _dashCooldown) 
                    {
                        TimeSinceLastDash = 0; //reset our time
                        StartCoroutine(Dash());
                    }
                }
                if (Input.GetKeyDown(KeyCode.Space)) //Scream
                {
                    // if weve waited enough since the last time, we can scream
                    if (TimeSinceLastScream >= _screamCooldown)
                    {
                        TimeSinceLastScream = 0;
                        StartCoroutine(Scream());
                    }
                }
                if (Input.GetKeyDown(KeyCode.E)) //pressing E picks up an offering
                {
                    PickUpOffering();
                }
                if (Input.GetKeyDown(KeyCode.F)) //press F to pay respects
                {
                    if (collectedOfferings.Count > 0)
                    {
                        PlaceDownOffering();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!isPossessing) //if we arent possessing yet, do so
                {
                    StartCoroutine(PossessObject());
                }
                else
                {
                    StartCoroutine(DepossessObject()); //if we are possessing, depossess
                }
            }
        }
    }
    private void FixedUpdate()
    {
        //TODO: make movement feel more floaty
        playerRB.MovePosition(playerRB.position + movement.normalized * _moveSpeed * Time.fixedDeltaTime);
    }
    private IEnumerator Dash()
    {
        dashing = true; //set bool to true so we cant call the dash coroutine immediately again

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //find the position which our mouse was over when we decided to dash
        int framesOfTryingToMove = 0; //this variable makes it possible for us to break out of a dash if we hold down the movement keys
        _ui.UIDash(); //play the dash animation on the player portrait
        _dashSound.Play(); //play our dash sound

        //either move to the position or the max distance, whichever is smaller

        Vector2 maxDash = (mousePos - playerRB.position).normalized * _dashDistance;
        //Vector2 cursorDash = (mousePos - playerRB.position);
        Vector2 initialRbPos = playerRB.position;

        Vector2 vectorToMove; //compare cursor and max dash to find the one we want, then use this variable to determine distance
        vectorToMove = maxDash;

        playerAnimator.SetBool("Dashing", dashing);             //  Set our dashing animator bool, so move to the dashing blend tree.
        playerAnimator.SetFloat("Horizontal", vectorToMove.x);  //  Set our horizontal and vertical move values, so our dash animation... 
        playerAnimator.SetFloat("Vertical", vectorToMove.y);    //  ...is determined by the direction we are dashing towards!

        List<GraveRobber> AlreadyDamagedRobbers = new List<GraveRobber>(); //since our collider moves, we might damage robbers twice, which we dont want

        while (Vector2.Distance(playerRB.position, vectorToMove + initialRbPos) > 0.3f)
        {
            List<GraveRobber> allRobbers = _dashCollider.GraveRobbersInCollider;
            foreach (GraveRobber robber in allRobbers) //constantly damage all robbers in our collider, but only once per dash
            {
                if (!AlreadyDamagedRobbers.Contains(robber)) //if we havent damaged this certain robber...
                {
                    robber.TakeFearDamage(_dashDamage); //...take damage...
                    AlreadyDamagedRobbers.Add(robber);  //...and add him to the robbers we cant damage anymore
                }
            }

            playerRB.position = Vector2.Lerp(playerRB.position, initialRbPos + vectorToMove, 0.1f); //constantly lerp towards the dash position
            yield return new WaitForFixedUpdate(); //use fixedupdate, since thats what our normal movement also uses

            if (framesOfTryingToMove > 15) //if we try to move enough, break out of our dash
            {
                //TODO: change this so our inputs change the position we aiming towards instead of breaking completely

                playerAnimator.SetBool("Dashing", false);
                break; //breaks out of the movement part of the dash, cutting straight to stopping the dash animation
            }
            if (movement.magnitude > 0) //if we are trying to move, tick our timer up
            {
                framesOfTryingToMove++;
            }
            else //if we lay off the movement however, reset this short window of breaking the dash
            {
                framesOfTryingToMove = 0;
            }
        }
        playerAnimator.SetBool("Dashing", false); //once our dash is complete, stop the animation
        dashing = false; //free us up to potentially dashing again
    }
    private IEnumerator Scream()
    {
        _screamObj.SetActive(true); //turn on the object which houses the scream animation
        screamAnim.SetBool("Scream", true); //play the scream AOE animation

        _ui.UIScream(); //set the UI scream animation
        _screamSound.Play(); //play the scream sound

        playerAnimator.SetBool("Screaming", true); //finally, play the ghosts scream animation
        foreach (GraveRobber robber in _screamCollider.GraveRobbersInCollider) //damage each enemy in the screams range
        {
            robber.TakeFearDamage(_screamDamage);
        }
        yield return new WaitForSeconds(1f); //wait for the animation to end and then stop it
        playerAnimator.SetBool("Screaming", false);

        screamAnim.SetBool("Scream", false);
        _screamObj.SetActive(false); //turn off the scream AOE Animation
    }
    private IEnumerator PossessObject()
    {
        //TODO: currently can possess multiple objects while we havent finished this function

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 8); // 1 bitshift 8 is the 8th layer, where possesable things are at

        if(hit.collider)
        {
            if(hit.collider.TryGetComponent(out PosessableObject possessable))
            {
                if (Vector3.Distance(hit.transform.position, transform.position) <= _possessionRange)
                {
                    lockMovement = true; //stops the player from being able to move
                    movement = Vector2.zero; //make sure we dont move anymore
                    possessable.Possess(); //possess the highlighted object
                    possessedObject = possessable; //set our local ref to the object were possessing
                    playerAnimator.SetBool("Disappear", true); //play the dissappear animation

                    _ghostGlow.SetActive(false); //disable the glowy effect and our shadow
                    _shadow.SetActive(false);
                    Events.current.PossessObject(possessedObject.gameObject); //send out an event that we are possessing something
                    yield return new WaitForSeconds(0.538f); //lock us from depossessing for the duration of the animation
                    isPossessing = true; //finally set the bool so we can depossess again
                }
            }
        }
    }
    private IEnumerator DepossessObject()
    {
        transform.position = possessedObject.transform.position; //move our player to the possessed object so we can reemerge

        playerAnimator.SetBool("Disappear", false); //play disappear animation
        possessedObject.Deposses(); //leave the possessed object so we may inhabit it again later
        possessedObject = null; //remove the ref to any possessed object
        Events.current.PossessObject(gameObject); //send out a possession event, to say we are back in our own body (important for grave robbers and Cameras)
        yield return new WaitForSeconds(0.75f); //unlock movement around 3/4 through the animation
        _ghostGlow.SetActive(true); //turn back on our glow and shadow
        _shadow.SetActive(true);
        lockMovement = false; //unlock movement
        isPossessing = false; //and the ability to possess again
    }
    private void PickUpOffering()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 9); //offerings are on layer 9, so use that layermask

        if (hit.collider)
        {
            if (hit.collider.TryGetComponent(out Offering offering))
            {
                if (!offering.disappearing) // only pick up offerings which havent been given away to other ghosts
                {
                    collectedOfferings.Add(offering); //add to our internal inventory system
                    offering.gameObject.SetActive(false); //disable the gameobject
                }
            }
        }
    }
    private void PlaceDownOffering()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 8); //the layer gravestones are on

        if (hit.collider)
        {
            if (hit.collider.TryGetComponent(out Gravestone grave))
            {
                if (grave.currentOffering != null) //if the grave already has a offering currently, return out of the function
                {
                    return;
                }
                Offering offering = collectedOfferings[0]; //take the first offering off our list
                offering.gameObject.SetActive(true); //reenable this offering
                offering.transform.position = grave.OfferingPos.transform.position; //and move it our graves offering position
                grave.Restore(offering.HealAmount); //heal our grave
                offering.FadeAway(grave); //slowly fade it away
                collectedOfferings.RemoveAt(0); //remove the offering from our list
            }
        }
    }
}
