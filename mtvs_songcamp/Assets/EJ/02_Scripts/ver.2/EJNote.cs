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
    public int bpm = 120;
    float spb;

    //02.Note_Connect Variables
    public NoteInfo noteInfo;

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
        transform.position = Vector3.down * Time.deltaTime * spb;

        //02.Note_autoDestroy
        if (transform.position.y < touchpad.position.y)
        {
            autoDestroy(true);
        }
        
    }

    //02.Note_autoDestroy
    public void autoDestroy(bool isPassed = false)
    {
        if (autoDestroyAction != null) autoDestroyAction(noteInfo.railIdx, this, isPassed);
        Destroy(gameObject);
    }

    //03.Note_Connect
    public void connectNote()
    {
        if (noteInfo.type == (int)NoteType.LONG)
        {

        }
        else if (noteInfo.type == (int)NoteType.DRAG)
        {

        }
    }

    //note만들기 함수
    //noteType분별해서 만들기 구분

    //NoteMaker Script에서 함수 호출
}
