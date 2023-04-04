using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Agent : MonoBehaviour
{

    // Singleton
    private static Agent instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static Agent Instance
    {
        get { return instance; }
    }
    // Singleton END

    public bool freeExplore = true;
    public bool followMouse = false;
    public int numDirections = 16;
    public float speed = 10.0f;       // the speed at which to move the object
    public Vector3 targetPosition;  // the position to move to
    
    public bool pathWalk = false;
    public int pathNum;
    public Vector3 pathPoint; 

    public GameObject exploreObject;
    private ExploreCounter exploreCounter;

    private void Start()
    {
        // Choose a random target position within the specified range
        exploreCounter = exploreObject.GetComponent<ExploreCounter>();
        targetPosition = GetRandomTargetPosition();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && followMouse)
        {
            followMouse = false;
            targetPosition = GetRandomTargetPosition();
        }
        if(followMouse)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            }    
        }
        else if(freeExplore)
        {
            moveToTarget();
        }
        else if(pathWalk)
        {
            moveAlongPath();
        }
    }

    private void moveAlongPath()
    {
        // Calculate the direction and distance to the target position
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0.0f;
        float distanceToTarget = direction.magnitude;

        // Move towards the target position
        if (distanceToTarget > 0.1f)
        {
            direction.Normalize();
            transform.Translate(direction * Mathf.Min(distanceToTarget, speed * Time.deltaTime), Space.World);
        }
        else
        {
            // Choose a new random target position within the specified range
            pathNum++;
        }
    }

    private void moveToTarget()
    {
        // Calculate the direction and distance to the target position
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0.0f;
        float distanceToTarget = direction.magnitude;

        // Move towards the target position
        if (distanceToTarget > 0.1f)
        {
            direction.Normalize();
            transform.Translate(direction * Mathf.Min(distanceToTarget, speed * Time.deltaTime), Space.World);
        }
        else
        {
            // Choose a new random target position within the specified range
            targetPosition = GetRandomTargetPosition();
        }
    }

    private Vector3 GetRandomTargetPosition()
    {
        for(int j = 0; j < 1000; j++)
        {
            float randomPosition = GameManager.Instance.Levy(1.0f, 1.9f);
            List<Vector3> possiblePositions = new List<Vector3>();

            // Populate the list of possible movement directions
            for (int i = 0; i < numDirections; i++)
            {
                float angle = i * Mathf.PI * 2 / numDirections;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 newPosition = transform.position + direction.normalized * randomPosition;
                if (!GameManager.Instance.IsTrajectoryBlocked(transform.position, newPosition))
                {
                    possiblePositions.Add(newPosition);
                }
            }
            if(possiblePositions.Count != 0)
            {
                exploreCounter.ChangeStepCount();
                return possiblePositions[Random.Range(0, possiblePositions.Count)];
            }
        }
        Debug.Log("Stuck! This probably should't happen");
        return Vector3.zero;
    }

    public void IncrementSpeed()
    {
        speed = speed+10f;
        speed = Mathf.Clamp(speed, 0f, 100f);
    }

    public void DecrementSpeed()
    {
        speed = speed-10f;
        speed = Mathf.Clamp(speed, 0f, 100f);
    }

    public void ToggleExplore()
    {
        freeExplore = !freeExplore;
    }

    public void FollowMouseOn()
    {
        followMouse = true;
    }
}
