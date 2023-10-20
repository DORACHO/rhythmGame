using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJMissPad : MonoBehaviour
{
    public GameObject missTexts;
    public Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (Input.GetKey(KeyCode.Space))
    //    {
    //        GameObject miss = Instantiate(missTexts, canvas.transform.position - Vector3.forward, Quaternion.identity);
    //        miss.transform.SetParent(canvas.transform);
    //        Destroy(miss,0.5f);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Note"))
    //    {
    //        GameObject miss = Instantiate(missTexts, canvas.transform.position - Vector3.forward, Quaternion.identity);
    //        miss.transform.SetParent(canvas.transform);
    //        Destroy(miss, 0.5f);
    //    }
    //}
}
