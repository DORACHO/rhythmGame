using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
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

    //dragNote가 떼져야 하는 index
    public int DRAG_release_idx;
}

public class EJNoteStorage : MonoBehaviour
{

}

