using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EJTouchPad : MonoBehaviour
{
    public GameObject[] scoreTexts;
    public GameObject[] touchpads;
    public Canvas canvas;
    float currentTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        print("touchpad에 triggerEnter된 것은" + other.gameObject);
        //touchpad 빛나기?
    }
    private void OnTriggerStay(Collider other)
    {
        currentTime += Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
        {
            print("triggerEnter되었고 space바를 눌렀다");
            Destroy(other.gameObject);

            if (Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position) < 1.5f && Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position)>=1f)
            {
                GameObject good = Instantiate(scoreTexts[0], canvas.transform.position - Vector3.forward, Quaternion.identity);
                good.transform.SetParent(canvas.transform);

            }
            else if (Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position) < 1f && Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position) >= 0.5f)
            {
                GameObject great = Instantiate(scoreTexts[1], canvas.transform.position - Vector3.forward, Quaternion.identity);
                great.transform.SetParent(canvas.transform);
  
            }
            else if (Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position) < 0.5f && Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position) >= 0f)
            {
                GameObject excellent = Instantiate(scoreTexts[2], canvas.transform.position - Vector3.forward, Quaternion.identity);
                excellent.transform.SetParent(canvas.transform);


  
            }
            else if (Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position) < 3f && Vector3.Distance(other.gameObject.transform.position, touchpads[0].transform.position) >= 1.5f)
            {
                GameObject bad = Instantiate(scoreTexts[3], canvas.transform.position - Vector3.forward, Quaternion.identity);
                bad.transform.SetParent(canvas.transform);


            }
        }


    }

    private void OnTriggerExit(Collider other)
    {
        print("touchpad에 triggerExit된 것은"+ other.gameObject);

        if (other.CompareTag("Note"))
        {
            //other.GetComponent<MeshRenderer>().enabled = false;
            Destroy(other.gameObject);
        }
    }
}
