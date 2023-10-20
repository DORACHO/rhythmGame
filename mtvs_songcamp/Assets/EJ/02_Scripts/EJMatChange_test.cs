using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJMatChange_test : MonoBehaviour
{
    public Material miss;
    Material original;

    bool isOriginal;

    // Start is called before the first frame update
    void Start()
    {
        original = gameObject.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (isOriginal)
            {
                gameObject.GetComponent<MeshRenderer>().material = miss;
                isOriginal = false;
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material = original;
                isOriginal = true;
            }
        }

    }
}
