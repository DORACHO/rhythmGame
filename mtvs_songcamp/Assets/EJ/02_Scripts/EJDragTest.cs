using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EJDragTest : MonoBehaviour
{
    public GameObject[] touchpads;
    public Material[] mats;
    bool[] istouchpadPressed = new bool[6];

    int pressCount;

    public Canvas canvas;
    public GameObject scoreTexts;

    public GameObject dragNote;

    float currTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        #region inputCheck
        if (Input.GetKeyDown(KeyCode.J))
        {
            istouchpadPressed[0] =  true;
            touchpads[0].GetComponent<MeshRenderer>().material = mats[0];
            pressCount++;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            istouchpadPressed[1] = true;
            touchpads[1].GetComponent<MeshRenderer>().material = mats[1];
            pressCount++;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            istouchpadPressed[2] = true;
            touchpads[2].GetComponent<MeshRenderer>().material = mats[2];
            pressCount++;
            print("pressCount는"+ pressCount);
        }

        if (Input.GetKeyUp(KeyCode.J))
        {
            //istouchpadPressed[0] = false;
            touchpads[0].GetComponent<MeshRenderer>().material = mats[1];
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            //istouchpadPressed[1] = false;
            touchpads[1].GetComponent<MeshRenderer>().material = mats[2];
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            //istouchpadPressed[2] = false;
            touchpads[2].GetComponent<MeshRenderer>().material = mats[0];
        }
        #endregion

        //if (pressCount == 3)
        //{
        //    GameObject text = Instantiate(scoreTexts, canvas.transform.position, Quaternion.identity);
        //    text.transform.SetParent(canvas.transform);

        //    pressCount -= 3;
        //}

        float dist = Mathf.Abs(dragNote.transform.position.y - touchpads[0].transform.position.y);

        //dragNote가 지나갈 때
        //mouse pointer로 

        //Camera.screenToRay 
        if (dist <3)
        {
            if (istouchpadPressed[0] == true)
            {
                //currTime += Time.deltaTime;
                if (istouchpadPressed[1] == true)
                {
                    if (istouchpadPressed[2] == true)
                    {
                        GameObject text = Instantiate(scoreTexts, canvas.transform.position, Quaternion.identity);
                        text.transform.SetParent(canvas.transform);
                    }
                }
            }
        }

        //dragNote가 지나갈 때 drag 시간차 계산
        if (dist < 3)
        {
            if (istouchpadPressed[0] == true)
            {
                currTime += Time.deltaTime;
                if (istouchpadPressed[1] == true && currTime <0.1f)
                {
                    currTime = 0;
                    currTime += Time.deltaTime;
                    if (istouchpadPressed[2] == true && currTime<0.1f)
                    {
                        GameObject text = Instantiate(scoreTexts, canvas.transform.position, Quaternion.identity);
                        text.transform.SetParent(canvas.transform);
                    }
                }
            }
        }
    }
}
