using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EJLongNote_test : MonoBehaviour
{
    public GameObject note;
    public Transform noteFactory;

    double currentTime = 0;
    LineRenderer lr;

    float firstNoteTime = 1;
    float endNoteTime = 6;
    bool isStartNoteDone;

    GameObject startNote;
    GameObject endNote;

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
                //note.transform.SetParent(noteFactory.transform);

            }
                lr.SetPosition(0, startNote.transform.position);           
                isStartNoteDone = true;
        }
        
        //setPosition 할 때 까지 계속 그려줘야 한다.

        if (currentTime >= endNoteTime)
        {
            if (isStartNoteDone)
            {
                endNote = Instantiate(note, noteFactory.position + Vector3.forward * (-0.5f), Quaternion.identity);
                endNote.transform.forward = note.transform.forward;
                //note.transform.SetParent(noteFactory.transform);

                lr.SetPosition(1, endNote.transform.position);
               
                isStartNoteDone = false;
                currentTime -= endNoteTime;
            }
        }
    }
}
