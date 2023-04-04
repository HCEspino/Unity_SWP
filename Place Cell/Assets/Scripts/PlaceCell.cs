using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaceCell : MonoBehaviour
{
    public int ID;
    public float colorValue = 0.0f; // The value used to calculate the color on the gradient
    private Renderer _renderer; // The renderer component of the object
    private float activation;
    public PlaceNetwork network;

    // Spikewave stuff
    public int v = 0;
    public int u = 0;
    public int I = 0;
    public Hashtable connections;
    public Hashtable wgts;
    public Hashtable delaybuffs;

    private float spikeColor = 0.0f;

    public void reset()
    {
        v = 0;
        u = 0;
        I = 0;

        int[] keyList = new int[delaybuffs.Count];
        delaybuffs.Keys.CopyTo(keyList, 0);
        foreach (int key in keyList)
        {
            delaybuffs[key] = 0;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        connections = new Hashtable();
        wgts = new Hashtable();
        delaybuffs = new Hashtable();
        reset();
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

        if(network.spiking)
        {
            SpikeColor(Mathf.Min(1.0f, (float)v));
        }
        else
        {
            if(spikeColor != 0f)
            {
                spikeColor = 0f;
            }
            ChangeColor(activation);
        }

        MoveFromLists(activation);
    }

    public float Activation(float x)
    {
        float gauss = GameManager.Instance.Gaussian(x, 1.0f, 0.0f, 2.0f);
        return gauss;
    }

    private void SpikeColor(float x)
    {
        Color color = Color.red;
        if(x == 1.0f)
        {
            color = Color.Lerp(Color.red, Color.white, x);
            spikeColor = 1.0f;
        }
        else if (spikeColor != 0.0f)
        {
            spikeColor = Mathf.Max(0.0f, spikeColor - (1f * Time.deltaTime));
            color = Color.Lerp(Color.red, Color.white, spikeColor);
        }
        _renderer.material.color = color;
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
