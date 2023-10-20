using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EJLongNoteMaker_test : MonoBehaviour
{
    public GameObject note;
    public GameObject linkLinePrefab;
    public Transform noteFactory;
    public Transform touchPad;
    public Transform vacantNoteFac;

    double currentTime = 0;
    double cumulatedTime;
    LineRenderer lr;

    float firstNoteTime = 1;
    float endNoteTime = 2;
    float termNoteTime = 3;
    bool isStartNoteDone;

    GameObject startNote;
    GameObject endNote;
    GameObject linkLine;
    

    // Start is called before the first frame update
    void Start()
    {
        lr = note.GetComponent<LineRenderer>();  
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= firstNoteTime)
        {
            if (!isStartNoteDone)
            {
                startNote = Instantiate(note, noteFactory.position + Vector3.forward * (-0.5f), Quaternion.identity);              
                startNote.transform.forward = note.transform.forward;
                startNote.transform.SetParent(vacantNoteFac);
                startNote.tag = "startNote";
                isStartNoteDone = true;
            }
        }
        
        //setPosition 할 때 까지 계속 그려줘야 한다.
        //lr Update로 해주는 방법

        if (currentTime >= endNoteTime)
        {
            if (isStartNoteDone)
            {
                endNote = Instantiate(note, noteFactory.position + Vector3.forward * (-0.5f), Quaternion.identity);
                endNote.transform.forward = note.transform.forward;
                endNote.transform.SetParent(vacantNoteFac);
                endNote.tag = "endNote";

                linkLine = Instantiate(linkLinePrefab, (startNote.transform.position + endNote.transform.position) / 2 , Quaternion.identity);
                linkLine.transform.SetParent(vacantNoteFac);
                linkLine.tag = "linkNote";
              
                float length = (endNote.transform.position.y - startNote.transform.position.y);
                linkLine.transform.localScale += new Vector3(0,length, 0);

                isStartNoteDone = false;
                currentTime -= (endNoteTime + 1000000);
                lr.SetPosition(1, endNote.transform.position);
            }

        }

    }

}
