using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }

            return instance;
        }
    }

    // Declare any public variables and game state logic here

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Declare any other necessary functions here
    public float Gaussian(float x, float a, float b, float c)
    {
        float exponent = -Mathf.Pow(x - b, 2.0f) / (2.0f * Mathf.Pow(c, 2.0f));
        return a * Mathf.Exp(exponent);
    }

    public float Levy(float alpha, float beta)
    {
        float u = Random.value;
        float v = Random.value;
        float w = Mathf.Tan(Mathf.PI * v - 0.5f * Mathf.PI);

        return beta / (Mathf.Pow(w, 1.0f / alpha) * Mathf.Sqrt(u));
    }

    public bool IsTrajectoryBlocked(Vector3 startPosition, Vector3 endPosition, float sphereRadius = 0.1f)
    {
        Vector3 direction = endPosition - startPosition;
        float distance = direction.magnitude;

        RaycastHit hitInfo;
        if (Physics.SphereCast(startPosition, sphereRadius, direction.normalized, out hitInfo, distance))
        {
            return true;
        }

        return false;
    }

}