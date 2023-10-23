using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

//Note Storage

//01. NoteType
//02. NoteInfo

//All Notes
public enum NoteType
{
    SHORT,
    LONG,
    DRAG_RIGHT,
    DRAG_LEFT
}

[System.Serializable]
public struct NoteInfo
{
    public int railIdx;
    public int type;
    public float time;

    //longNote일때 start이면 true, end이면 false
    public bool isLongNoteStart;

    //dragNote일 때 line, idxNote
    public int press_idx;
}

public class EJNoteStorage : MonoBehaviour
{

}
