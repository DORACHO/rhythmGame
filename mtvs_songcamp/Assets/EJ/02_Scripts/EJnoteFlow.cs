using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJnoteFlow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition += Vector3.down * 5 * Time.deltaTime;
    }
}
