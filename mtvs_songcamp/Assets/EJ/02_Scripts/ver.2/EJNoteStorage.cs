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

    //longNote�϶� start�̸� true, end�̸� false
    public bool isLongNoteStart;

    //dragNote�� �� ���� rail�� idx?
    //dragNote�� �� ������ rail idx?
    public int released_idx;
}

public class EJNoteStorage : MonoBehaviour
{

}

