using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;


public class EJNote : MonoBehaviour
{
    //01.Note_Flow
    //02.Note_Connect
    //03.Note_autoDestroy

    //01.Note_Flow Variables
    int bpm = 120;
    float spb;
    public float speed = 5.5f;

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
        transform.position += Vector3.down * Time.deltaTime * speed/* * spb*/;

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

        print("*****현재 autoDestroy가 실행되는 Note의 isLongStart값은" + noteInfo.isLongNoteStart);
        Destroy(gameObject);
        
    }

    //03.Note_Connect
    //03-1.endNote가 나오면 start-end사이의 거리를 구해 이어주는 방식
    public void connectNote(GameObject endN)
    {
        print("* connectNote가 실행되었습니다");

        startN = this.gameObject;

        if (endN == null) return;

        linkNote = Instantiate(linkNotePrefab, (startN.transform.position + endN.transform.position) / 2, Quaternion.identity);
        linkNote.transform.SetParent(endN.transform);
        

        //넣게되면 pad를 지나간 것에 대한 체크를 또 하기 때문!

        //linkNote.AddComponent<EJNote>();
        //EJNote linknote = linkNote.GetComponent<EJNote>();
        //linknote.noteInfo.type = (int)NoteType.LONG;
        //linknote.speed = 0;



        float length = (endN.transform.localPosition.y - startN.transform.localPosition.y);
        linkNote.transform.localScale += new Vector3(0, length, 0);             
    }


    //03-2.startNote가 나오면 linkNote의 거리를 늘리다가, end에 닿으면 자라지 않는 방식
    public IEnumerator connectNote2(GameObject endN)
    {
        linkNote = Instantiate(linkNotePrefab, (startN.transform.position + endN.transform.position) / 2, Quaternion.identity);
        linkNote.transform.SetParent(startN.transform);
        float length = (endN.transform.localPosition.y - startN.transform.localPosition.y);

        while (linkNote.transform.position.y == length)
        {
            linkNote.transform.position += new Vector3(0, 1, 0);
            yield return null;
        }
    }

    public void startConnecting(GameObject endN)
    {
        StartCoroutine(connectNote2(endN));
    }
}
