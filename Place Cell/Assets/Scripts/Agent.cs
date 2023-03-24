using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float speed = 10.0f;       // the speed at which to move the object
    private Vector3 targetPosition;  // the position to move to
    

    private void Start()
    {
        // Choose a random target position within the specified range
        targetPosition = GetRandomTargetPosition();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
        float randomPosition = GameManager.Instance.Levy(1.0f, 1.9f);
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0.0f;
        Vector3 newPosition = transform.position + randomDirection.normalized * randomPosition;

        while(GameManager.Instance.IsTrajectoryBlocked(transform.position, newPosition))
        {
            randomPosition = GameManager.Instance.Levy(1.0f, 1.9f);
            randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0.0f;
            newPosition = transform.position + randomDirection.normalized * randomPosition;
        }
        

        return newPosition;
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
