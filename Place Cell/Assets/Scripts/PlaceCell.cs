using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceCell : MonoBehaviour
{
    public int ID;
    public float colorValue = 0.0f; // The value used to calculate the color on the gradient
    private Renderer _renderer; // The renderer component of the object
    private float activation;
    public PlaceNetwork network;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>(); // Get the renderer component of the object
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, Agent.Instance.transform.position);
        if(distance > 10)
        {
            activation = 0;
        }
        else
        {
            activation = Activation(distance);
        }

        ChangeColor(activation);
        MoveFromLists(activation);
    }

    private float Activation(float x)
    {
        float gauss = GameManager.Instance.Gaussian(x, 1.0f, 0.0f, 2.0f);
        return gauss;
    }

    private void ChangeColor(float x)
    {
        // Calculate the color on the gradient based on the color value
        Color color = Color.Lerp(Color.red, Color.white, x);
        // Set the color of the object's material
        _renderer.material.color = color;
    }

    private void MoveFromLists(float x)
    {
        if(x > 0)
        { 
            if(!network.active.ContainsKey(ID))
            {
                network.active.Add(ID, x);
            }
            else
            {
                network.active[ID] = x;
            }
        }
        else if(x == 0 && network.active.ContainsKey(ID))
        {
            network.active.Remove(ID);
        }
    }
}
