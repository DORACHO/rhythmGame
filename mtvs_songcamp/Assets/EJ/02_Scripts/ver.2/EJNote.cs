using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class EJNote : MonoBehaviour
{
    //01.Note_Flow
    //02.Note_Connect
    //03.Note_autoDestroy

    //01.Note_Flow Variables
    int bpm = 120;
    float spb;

    //02.Note_Connect Variables
    public NoteInfo noteInfo;
    public GameObject linkNotePrefab;
    GameObject linkNote;
    GameObject startN;
    GameObject endN;

    //03.Note_autoDestroy
    public Action<int, EJNote, bool> autoDestroyAction;
    Transform touchpad;


    void Start()
    {
        //01.Note_Flow
        spb = 60 / bpm;

        //02.Note_autoDestroy
        touchpad = GameObject.FindWithTag("TouchPad").transform;
    }


    void Update()
    {
        //01.Note_Flow
        //===== 수정 필요 ===== note의 speed = bpm
        transform.position += Vector3.down * Time.deltaTime * 5/* * spb*/;

        //02.Note_autoDestroy isPassed Check
        if (transform.position.y +3f < touchpad.position.y)
        {
            autoDestroy(true);
        }

    }

    //02.Note_autoDestroy
    public void autoDestroy(bool isPassed = false)
    {
        //autoDestroyAction Parameter
        //01. rail_idx
        //02. noteInfo
        //03. passDestroy

        if (autoDestroyAction != null) autoDestroyAction(noteInfo.railIdx, this, isPassed);
        Destroy(gameObject);

    }

    //03.Note_Connect
    public void connectNote(GameObject endN)
    {
        print("connectNote가 실행되었습니다");

        startN = this.gameObject;

        if (endN == null) return;

        linkNote = Instantiate(linkNotePrefab, (startN.transform.position + endN.transform.position) / 2, Quaternion.identity);

        linkNote.transform.SetParent(endN.transform);

        float length = (endN.transform.localPosition.y - startN.transform.localPosition.y);
        linkNote.transform.localScale += new Vector3(0, length, 0);

    }
}
