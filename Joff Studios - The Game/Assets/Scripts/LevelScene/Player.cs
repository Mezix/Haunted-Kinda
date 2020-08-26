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
            foreach (GraveRobber robber in _dashCollider.GraveRobbersInCollider) //constantly damage all robbers in our collider, but only once per dash
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
        _screamObj.SetActive(true);
        screamAnim.SetBool("Scream", true);
        _ui.UIScream();
        _screamSound.Play();

        playerAnimator.SetBool("Screaming", true);
        foreach (GraveRobber robber in _screamCollider.GraveRobbersInCollider)
        {
            robber.TakeFearDamage(_screamDamage);
        }
        yield return new WaitForSeconds(1f);
        playerAnimator.SetBool("Screaming", false);
        screamAnim.SetBool("Scream", false);
        _screamObj.SetActive(false);
    }
    private IEnumerator PossessObject()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 8); // 1 bitshift 8 is the 8th layer, where possesable things are at

        if(hit.collider)
        {
            if(hit.collider.TryGetComponent<PosessableObject>(out PosessableObject possessable))
            {
                //print("possessing!");

                if (Vector3.Distance(hit.transform.position, transform.position) <= _possessionRange)
                {
                    lockMovement = true;
                    movement = Vector2.zero; //make sure we dont move anymore
                    possessable.Possess();
                    possessedObject = possessable;
                    playerAnimator.SetBool("Disappear", true);
                    _ghostGlow.SetActive(false);
                    _shadow.SetActive(false);
                    Events.current.PossessObject(possessedObject.gameObject);
                    yield return new WaitForSeconds(0.25f);
                    isPossessing = true;
                }
            }
        }
    }
    private IEnumerator DepossessObject()
    {
        transform.position = possessedObject.transform.position;

        isPossessing = false;
        playerAnimator.SetBool("Disappear", false);
        _ghostGlow.SetActive(true);
        _shadow.SetActive(true);
        possessedObject.Deposses();
        possessedObject = null;
        Events.current.PossessObject(gameObject);
        yield return new WaitForSeconds(0.75f);
        lockMovement = false;
    }
    private void PickUpOffering()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 9);
        if (hit.collider.TryGetComponent(out Offering offering))
        {
            if (!offering.disappearing)
            {
                collectedOfferings.Add(offering);
                offering.gameObject.SetActive(false);
            }
        }
    }
    private void PlaceDownOffering()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << 8); //the layer gravestones are on
        if (hit.collider.TryGetComponent<Gravestone>(out Gravestone grave))
        {
            if (grave.currentOffering != null)
            {
                return;
            }
            Offering offering = collectedOfferings[0];
            offering.gameObject.SetActive(true);
            offering.transform.position = grave.OfferingPos.transform.position;
            grave.Restore(offering.HealAmount);
            offering.FadeAway(grave);
            collectedOfferings.RemoveAt(0);
        }
    }
}
