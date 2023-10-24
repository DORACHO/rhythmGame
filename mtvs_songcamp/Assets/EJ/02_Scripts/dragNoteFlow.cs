using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dragNoteFlow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.down * Time.deltaTime * 3;
    }
}
