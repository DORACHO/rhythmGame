using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lr : MonoBehaviour
{
    public GameObject start;
    public GameObject end;

    public Transform startT;
    public Transform endT;
    GameObject startQ;
    GameObject endQ;

    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        startQ = Instantiate(start, startT.position, Quaternion.identity);
        endQ = Instantiate(start, endT.position, Quaternion.identity);
        lineRenderer = startQ.GetComponent<LineRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, startQ.transform.position);
        lineRenderer.SetPosition(1, endQ.transform.position);
    }
}
