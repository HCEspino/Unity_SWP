using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceNetwork : MonoBehaviour
{

    public GameObject cell;
    private int cellCount = 0;
    private PlaceNetwork placenetwork;
    public float radius = 0.1f;
    public List<PlaceCell> cells;

    private bool noClose;

    // Hashtable for activate and inactive neurons
    public Hashtable active;

    // Start is called before the first frame update
    void Start()
    {
        placenetwork = this.gameObject.GetComponent<PlaceNetwork>();

        active = new Hashtable();

        cells = new List<PlaceCell>();

        SpawnCell();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        int closestID = AllCellsFar();
        if(noClose)
        {
            PlaceCell newcell = SpawnCell();
            CreateConnection(cells[closestID].gameObject, newcell.gameObject);
        }
    }

    private PlaceCell SpawnCell()
    {
        GameObject cellobj = Instantiate(cell, Agent.Instance.transform.position, Quaternion.identity);
        cellobj.transform.SetParent(this.transform);

        PlaceCell newcell = cellobj.GetComponent<PlaceCell>();
        newcell.network = placenetwork;
        cells.Add(newcell);
        newcell.ID = cellCount;
        cellCount++;
        return newcell;
    }

    private int AllCellsFar()
    {

        float maxActivation = 0f;
        int minID = -1;

        if(active.Count > 0)
        {
            foreach (DictionaryEntry entry in active)
            {
                int cellID = (int)entry.Key;
                float cellActivation = (float)entry.Value;

                if(cellActivation > 0.05f)
                {
                    noClose = false;
                    return -1;
                }
                else if(cellActivation > maxActivation)
                {
                    maxActivation = cellActivation;
                    minID = cellID;
                }
            }
            noClose = true;
            return minID;
        }
        else
        {
            noClose = false;
            return -1;
        }
    }

    void CreateConnection(GameObject startObject, GameObject endObject)
    {
        // Calculate the length and direction of the cylinder
        Vector3 direction = endObject.transform.position - startObject.transform.position;
        float length = direction.magnitude;

        // Create the cylinder
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(this.transform);

        // Position the cylinder
        cylinder.transform.position = startObject.transform.position + 0.5f * direction;
        cylinder.transform.up = direction;

        // Scale the cylinder
        cylinder.transform.localScale = new Vector3(radius, 0.5f * length, radius);

        // Disable the collider on the cylinder so that it doesn't interfere with the objects it connects
        Destroy(cylinder.GetComponent<Collider>());
    }

}
