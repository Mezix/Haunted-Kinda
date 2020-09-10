using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TutorialGhost : MonoBehaviour
{
    private UnityEngine.Experimental.Rendering.Universal.Light2D GhostGlow; //the glow of our ghost
    public SpriteRenderer _shadow;
    private GameObject player; //reference to the player in the scene
    public SpriteRenderer ghostRenderer; //the sprite rendered of our ghost
    private float ghostSpriteOpacity; //the current opacity of our ghost
    private float ghostSpriteMaxOpacity; //the max opacity our ghost should achieve
    private float fadeAmount; //the amount to change our opacity every frame
    private float _moveSpeed = 1f;

    //CONVERSATIONS

    public ConversationScriptObj[] _conversations;
    public int _conversationIndex;

    //PATHFINDING
    private Seeker seeker; //the seeker component which creates paths
    private Path path; //the path our unit needs to take
    private int currentWaypoint = 0; //the index of our path.vectorPath
    private float nextWayPointDistance = 0.5f; //the distance before we seek out our next waypoint => the higher, the smoother the movement
    public bool reachedEndOfPath = false; //wether or not we have gotten to our last checkpoint
    public Transform[] positionsToSeekOut; //the gameobject we are currently looking to get to (e.g. escapePos, nearestGrave)
    private int positionIndex = 0;

    private void Awake()
    {
        GhostGlow = GetComponentInChildren<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        seeker = GetComponent<Seeker>();
    }
    private void Start()
    {
        ghostSpriteOpacity = 0; //set our ghost to be hidden by default
        ghostSpriteMaxOpacity = 0.9f; //our ghost should always be slightly opaque, so keep this value below 0
        _shadow.color = ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity); //apply the ghostopacity value
        GhostGlow.intensity = 0;
        fadeAmount = 0.05f;
        _conversationIndex = 0;
        _moveSpeed = 5f;
    }

    private void FixedUpdate()
    {
        MoveOnPath();
    }

    public IEnumerator FadeIn()
    {
        while(ghostSpriteOpacity < ghostSpriteMaxOpacity)
        {
            yield return new WaitForFixedUpdate();
            ghostSpriteOpacity += fadeAmount;
            _shadow.color = ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
            GhostGlow.intensity = ghostSpriteOpacity / ghostSpriteMaxOpacity; //swing between 0 and 1, depending on our fade
        }
        ghostSpriteOpacity = ghostSpriteMaxOpacity;
    }
    public IEnumerator FadeOut()
    {
        while(ghostSpriteOpacity > 0)
        {
            yield return new WaitForFixedUpdate();
            ghostSpriteOpacity -= fadeAmount;
            _shadow.color = ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
            GhostGlow.intensity = Mathf.Max(0, ghostSpriteOpacity / ghostSpriteMaxOpacity);
        }
        ghostSpriteOpacity = 0;
        GhostGlow.intensity = 0; //this value becomes negative if we dont set it to 0
    }

    private void MoveOnPath()
    {
        if (!reachedEndOfPath) //if we reached the end, just stop
        {
            if (path == null) //stop the method if we dont even have a path
            {
                return;
            }

            Vector3 vectorToMove = (path.vectorPath[currentWaypoint] - transform.position).normalized * _moveSpeed * Time.deltaTime;

            float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

            //TODO: Problem: unit can overshoot target

            transform.position += vectorToMove;

            if ((distance < nextWayPointDistance) && !(currentWaypoint == path.vectorPath.Count - 1))
            {
                currentWaypoint++;
            }
            else
            {
                if (distance < 0.1f)
                {
                    transform.position = path.vectorPath[currentWaypoint];
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
    private void UpdatePath(Transform newPos) //resets everything related to the path, then creates a new one
    {
        currentWaypoint = 0;
        reachedEndOfPath = false;
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, newPos.transform.position, OnPathComplete);
        }
    }

    public void MoveToNextPos()
    {
        if(positionIndex < positionsToSeekOut.Length)
        {
            UpdatePath(positionsToSeekOut[positionIndex]);
            positionIndex++;
        }
    }
}
