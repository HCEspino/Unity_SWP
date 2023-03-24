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
    [System.NonSerialized] public bool choosingEnd = false;
    // Hashtable for activate and inactive neurons
    public Hashtable active;

    // Spikewave Stuff
    public int spikeWaveSpeed = 1;
    public int spike = 1;
    public int refractory = -10;
    public int timesteps = 0;
    public bool foundGoal = false;
    public List<Vector2Int> aer;

    private int startID;
    private int endID;

    // Start is called before the first frame update
    void Start()
    {
        placenetwork = this.gameObject.GetComponent<PlaceNetwork>();
        active = new Hashtable();
        cells = new List<PlaceCell>();

        SpawnCell();
    }

    void Update()
    {
        if (choosingEnd && Input.GetMouseButtonDown(0))
        {
            choosingEnd = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 targetPosition = new Vector3(hit.point.x, Agent.Instance.transform.position.y, hit.point.z);
                endID = GetClosest(targetPosition);
                startID = GetClosest(Agent.Instance.transform.position);

                // Spikewave here
                SpikeWaveStart();
            }  
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        int closestID = AllCellsFar();
        if(noClose && Agent.Instance.freeExplore && !Agent.Instance.followMouse)
        {
            PlaceCell newcell = SpawnCell();
            CreateConnection(cells[closestID].gameObject, newcell.gameObject);
            SetWeightsBetween(cells[closestID], newcell);
        }
    }

    private void SetWeightsBetween(PlaceCell a, PlaceCell b)
    {

        // Debug.Log("Connections");
        //a to b
        a.connections.Add(b.ID, b);
        float aActb = a.Activation(Vector3.Distance(a.transform.position, b.transform.position));
        float aWgt = (11f - (9f * aActb + 1f));
        a.wgts.Add(b.ID, (int)aWgt);
        a.delaybuffs.Add(b.ID, 0);
        // Debug.Log(aWgt);
        //b to a
        b.connections.Add(a.ID, a);
        float bActa = b.Activation(Vector3.Distance(b.transform.position, a.transform.position));
        float bWgt = (11f - (9f * bActa + 1f));
        b.wgts.Add(a.ID, (int)bWgt);
        b.delaybuffs.Add(a.ID, 0);
        // Debug.Log(bWgt);
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

    private int GetClosest(Vector3 point)
    {
        float maxActivation = 0f;
        int minID = -1;

        foreach (PlaceCell cell in cells)
        {
            float distance = Vector3.Distance(cell.transform.position, point);
            float activation = cell.Activation(distance);
            if(activation > maxActivation)
            {
                maxActivation = activation;
                minID = cell.ID;
            }
        }
        return minID;
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

    private void SpikeWaveStart()
    {
        foreach (PlaceCell cell in cells)
        {
            cell.reset();
        }
        foundGoal = false;
        timesteps = 0;
        aer = new List<Vector2Int>();
        cells[startID].v = spike;
        //InvokeRepeating("SpikeWaveStep()", 0.2f, 0.2f);
    }

    private void SpikeWaveStep()
    {
        Debug.Log("Here");
        if(foundGoal)
        {
            Debug.Log("Goal Found!");
            CancelInvoke();
        }
        else
        {
            // Find neurons that spike
            int inx = 0;
            List<int> fid = new List<int>();
            foreach (PlaceCell cell in cells)
            {
                if (cell.v >= spike)
                {
                    fid.Add(cell.ID);
                    Vector2Int spikeinfo = new Vector2Int(timesteps, cell.ID);
                    aer.Add(spikeinfo);
                    inx++;
                }
            }

            Debug.Log("Here");
            // Spiked neurons send spike
            for (int i = 0; i < inx; i++)
            {
                cells[fid[i]].u = refractory;

                foreach (DictionaryEntry cons in cells[fid[i]].delaybuffs)
                {
                    int cellID = (int)cons.Key;
                    cells[fid[i]].delaybuffs[cellID] = cells[fid[i]].wgts[cellID];
                }

                if(cells[fid[i]].ID == endID)
                {
                    foundGoal = true;
                }
            }

            if(!foundGoal)
            {
                // Update I
                foreach (PlaceCell cell in cells)
                {
                    cell.I = cell.u;
                }

                // Decrement delay buffers
                foreach (PlaceCell cell in cells)
                {
                    foreach (DictionaryEntry cons in cell.delaybuffs)
                    {
                        int cellID = (int)cons.Key;
                        int delay = (int)cell.delaybuffs[cellID];
                        PlaceCell connected = (PlaceCell)cell.connections[cons];

                        if (delay == 1)
                        {
                            connected.I += 1;
                        }

                        cell.delaybuffs[cellID] = Mathf.Max(0, delay - 1);
                    }
                }

                foreach (PlaceCell cell in cells)
                {
                    cell.v = cell.I;
                    cell.u = Mathf.Min(cell.u + 1, 0);
                }
            }
        }
    }

    public void ToggleChooseEnd()
    {
        choosingEnd = true;
    }

}
