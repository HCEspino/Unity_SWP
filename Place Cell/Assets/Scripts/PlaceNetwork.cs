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
    public bool hebbian = true;
    public float hebbActivation = 0.01f;
    public float closeThreshold = 0.05f;
    public bool spiking = false;
    public int spike = 1;
    public int refractory = -10;
    public int timesteps = 0;
    public bool foundGoal = false;
    public List<Vector2Int> aer;

    private int startID;
    private int endID;
    public List<int> path;

    public GameObject pathlenObject;
    private PathCounter pathlenCounter;

    // Start is called before the first frame update
    void Start()
    {
        placenetwork = this.gameObject.GetComponent<PlaceNetwork>();
        pathlenCounter = pathlenObject.GetComponent<PathCounter>();
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
        if (Agent.Instance.pathWalk)
        {
            if(Agent.Instance.pathNum >= path.Count)
            {
                Agent.Instance.pathWalk = false;
            }
            else if(Agent.Instance.targetPosition != cells[path[Agent.Instance.pathNum]].transform.position)
            {
                Agent.Instance.targetPosition = cells[path[Agent.Instance.pathNum]].transform.position;
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
            float wgt = SetWeightsBetween(cells[closestID], newcell);
            CreateConnection(cells[closestID].gameObject, newcell.gameObject, wgt);
        }
    }

    private float SetWeightsBetween(PlaceCell a, PlaceCell b)
    {

        // Debug.Log("Connections");
        //a to b
        a.connections.Add(b.ID, b);
        float aActb = a.Activation(Vector3.Distance(a.transform.position, b.transform.position));
        float aWgt = (6f - (4f * aActb + 1f));
        a.wgts.Add(b.ID, (int)aWgt);
        a.delaybuffs.Add(b.ID, 0);
        // Debug.Log(aWgt);
        //b to a
        b.connections.Add(a.ID, a);
        float bActa = b.Activation(Vector3.Distance(b.transform.position, a.transform.position));
        float bWgt = (6f - (4f * bActa + 1f));
        b.wgts.Add(a.ID, (int)bWgt);
        b.delaybuffs.Add(a.ID, 0);
        // Debug.Log(bWgt);

        return aWgt;
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

    private List<PlaceCell> GetActivated(Vector3 point, float minActivation)
    {

        List<PlaceCell> activated = new List<PlaceCell>();

        foreach (PlaceCell cell in cells)
        {
            float distance = Vector3.Distance(cell.transform.position, point);
            float activation = cell.Activation(distance);
            if(activation > minActivation)
            {
                //Debug.Log(activation);
                activated.Add(cell);
            }
        }

        return activated;
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

                if(cellActivation > closeThreshold)
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

    void CreateConnection(GameObject startObject, GameObject endObject, float wgt)
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
        cylinder.transform.localScale = new Vector3(wgt/10, 0.5f * length, radius);

        // Disable the collider on the cylinder so that it doesn't interfere with the objects it connects
        Destroy(cylinder.GetComponent<Collider>());
    }

    private void SpikeWaveStart()
    {
        foreach (PlaceCell cell in cells)
        {
            cell.reset();
        }
        spiking = true;
        foundGoal = false;
        timesteps = 0;
        aer = new List<Vector2Int>();
        cells[startID].v = spike;
        InvokeRepeating("SpikeWaveStep", 0.05f, 0.05f);
    }

    //WIP
    private void SpikeWaveStepConnectionless()
    {
        timesteps++;
        if(foundGoal)
        {
            Debug.Log("Goal Found!");
            CancelInvoke();
            GetPath();
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

            // Spiked neurons send spike (For this version, Spiked neurons are those that are active at that location)
            for (int i = 0; i < inx; i++)
            {
                cells[fid[i]].u = refractory;
                
                //Get Activated Cells Nearby
                List<PlaceCell> activated = GetActivated(cells[fid[i]].transform.position, 0.01f);

                foreach (PlaceCell connected in activated)
                {
                    continue;
                }

                int[] keyList = new int[cells[fid[i]].delaybuffs.Count];
                cells[fid[i]].delaybuffs.Keys.CopyTo(keyList, 0);
                foreach (int cellID in keyList)
                {
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
                    int[] keyList = new int[cell.delaybuffs.Count];
                    cell.delaybuffs.Keys.CopyTo(keyList, 0);
                    foreach (int cellID in keyList)
                    {
                        int delay = (int)cell.delaybuffs[cellID];
                        PlaceCell connected = (PlaceCell)cell.connections[cellID];

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

    private void SpikeWaveStep()
    {
        timesteps++;
        if(foundGoal)
        {
            Debug.Log("Goal Found!");
            CancelInvoke();
            GetPath();
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


            if(hebbian)
            {
                // Connect spiking neurons
                for (int i = 0; i < fid.Count; i++)
                {
                    List<PlaceCell> activated = GetActivated(cells[fid[i]].transform.position, hebbActivation);
                    foreach (PlaceCell cell in activated)
                    {
                        if(cell.ID != cells[fid[i]].ID && !IsConnected(fid[i], cell.ID))
                        {
                            CreateConnection(cells[fid[i]].gameObject, cell.gameObject, 10f);
                            SetWeightsBetween(cells[fid[i]], cell);
                        }
                    }
                }
            }

            // Spiked neurons send spike
            for (int i = 0; i < inx; i++)
            {
                cells[fid[i]].u = refractory;


                int[] keyList = new int[cells[fid[i]].delaybuffs.Count];
                cells[fid[i]].delaybuffs.Keys.CopyTo(keyList, 0);
                foreach (int cellID in keyList)
                {
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
                    int[] keyList = new int[cell.delaybuffs.Count];
                    cell.delaybuffs.Keys.CopyTo(keyList, 0);
                    foreach (int cellID in keyList)
                    {
                        int delay = (int)cell.delaybuffs[cellID];
                        PlaceCell connected = (PlaceCell)cell.connections[cellID];

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

    public void GetPath()
    {
        path = new List<int>();
        path.Add(endID);

        int oldestIdx = 0;
        int connectingSpike = 0;

        int currSpikeIdx = 0;

        for (int i = 0; i < aer.Count; i++)
        {
            if (aer[i][1] == endID)
            {
                currSpikeIdx = i;
            }
        }

        while(path[path.Count - 1] != startID)
        {
            oldestIdx = -1;
            connectingSpike = -1;

            for (int i = currSpikeIdx - 1; i > -1; i--)
            {
                if (IsConnected(aer[currSpikeIdx][1], aer[i][1]))
                {
                    connectingSpike = aer[i][1];
                    oldestIdx = i;
                }
            }

            currSpikeIdx = oldestIdx;
            path.Add(connectingSpike);
        }

        path.Reverse();
        spiking = false;
        GetPathLength();
        Agent.Instance.pathWalk = true;
        Agent.Instance.pathNum = 0;
        Agent.Instance.targetPosition = cells[path[Agent.Instance.pathNum]].transform.position;
    }

    public void GetPathLength()
    {
        float len = 0;
        for(int i = 0; i < path.Count - 1; i++)
        {
            len += Vector3.Distance(cells[path[i]].transform.position, cells[path[i+1]].transform.position);
        }
        pathlenCounter.SetPathText(len);
    }

    public bool IsConnected(int a, int b)
    {
        if(cells[a].connections.ContainsKey(b) && cells[b].connections.ContainsKey(a))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ToggleChooseEnd()
    {
        choosingEnd = true;
    }

}
