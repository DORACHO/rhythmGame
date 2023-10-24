using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJDragTest : MonoBehaviour
{
    public GameObject[] touchpads;
    bool[] istouchpadPressed;

    int pressCount;

    public Canvas canvas;
    public GameObject scoreTexts;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            istouchpadPressed[0] =  true;
            touchpads[0].GetComponent<MeshRenderer>().enabled = true;
            pressCount++;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            istouchpadPressed[1] = true;
            touchpads[4].GetComponent<MeshRenderer>().enabled = true;
            pressCount++;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            istouchpadPressed[2] = true;
            touchpads[5].GetComponent<MeshRenderer>().enabled = true;
            pressCount++;
        }

        if (Input.GetKeyUp(KeyCode.J))
        {
            istouchpadPressed[0] = false;
            touchpads[3].GetComponent<MeshRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            istouchpadPressed[1] = false;
            touchpads[4].GetComponent<MeshRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            istouchpadPressed[2] = false;
            touchpads[5].GetComponent<MeshRenderer>().enabled = false;
        }

        if (pressCount == 3)
        {
            GameObject text = Instantiate(scoreTexts, canvas.transform.position, Quaternion.identity);
        }
    }
}
