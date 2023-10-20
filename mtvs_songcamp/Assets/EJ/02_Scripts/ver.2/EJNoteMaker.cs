using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//01. Note_Instantiate
//02. Note_pressCheck
public enum NoteType
{
    SHORT,
    LONG,
    DRAG
}

[System.Serializable]
public struct NoteInfo
{
    public int railIdx;
    public int type;
    public float time;
    public bool press_start;
    public int pressedRail_idx;
}

public class EJNoteMaker : MonoBehaviour
{
    //01. Note_Instantiate
    public GameObject[] notePrefabs;
    public Transform[] noteSpawnPos;
    public Transform[] touchpads;

    const int railCount = 6;
    float currTime;

        //01-1.noteData
    List<NoteInfo> noteInfo = new List<NoteInfo>();
    List<NoteInfo>[] noteInfo_Rails = new List<NoteInfo>[railCount];


    //02. Note_pressCheck
        //02-2.Hierarchy - instance noteData
    List<EJNote>[] noteInstance_Rails = new List<EJNote>[railCount];
    bool[] isPressed = new bool[railCount];


    void Start()
    {
        
        for (int i = 0; i < noteInstance_Rails.Length; i++)
        {
            noteInstance_Rails[i] = new List<EJNote>();
        }
    }


    void Update()
    {      
        currTime += Time.deltaTime;

        #region note 생성,파괴
        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            if (noteInfo_Rails[i].Count>0)
            {
                if(currTime >= noteInfo_Rails[i][0].time) 
                {
                    //node생성
                    GameObject note = Instantiate(notePrefabs[0], noteSpawnPos[i].position, Quaternion.identity);

                    EJNote noteInstance = note.GetComponent<EJNote>();
                    noteInstance.noteInfo = noteInfo_Rails[i][0];

                    //touchpad 통과 후, node 파괴
                    //지나가서 파괴되었는지 눌러서 파괴되었는지
                    noteInstance.autoDestroyAction = (railIdx, noteInfo, isPassed) =>
                    {
                        if (isPassed) isPressed[railIdx] = false;
                        noteInstance_Rails[railIdx].Remove(noteInfo);
                    };



                    noteInfo_Rails[i].RemoveAt(0);
                    noteInstance_Rails[i].Add(noteInstance);                   
                }
            }
        }
        #endregion

        #region TouchPad별 PressCheck
        //0,1,2,3,4,5 ~ a,s,d,j,k,l

        //keyDown
        if (Input.GetKeyDown(KeyCode.A))
        {
            isPressDown(0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            isPressDown(1);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            isPressDown(2);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            isPressDown(3);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            isPressDown(4);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            isPressDown(5);
        }
        //keyUp
        if (Input.GetKeyUp(KeyCode.A))
        {
            isPressUp(0);
            isPressed[0] = false;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            isPressUp(1);
            isPressed[1] = false;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            isPressUp(2);
            isPressed[2] = false;
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            isPressUp(3);
            isPressed[3] = false;
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            isPressUp(4);
            isPressed[4] = false;
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            isPressUp(5);
            isPressed[5] = false;
        }

        #endregion

        #region 점수판정
        for (int i = 0; i < isPressed.Length; i++)
        {
            if (isPressed[i] == true)
            {
                //누르는 중
            }
        }

        for (int i = 0; i < noteInstance_Rails.Length; i++)
        {
            if (noteInstance_Rails[i].Count == 0) continue;

            if (noteInstance_Rails[i][0].noteInfo.type == (int)NoteType.LONG && noteInstance_Rails[i][0].noteInfo.press_start == false)
            {
                if (isPressed[i] == false)
                {
                    //실패중
                }
            }
        }

        #endregion
    }

    void isPressDown(int n)
    {
        if (noteInstance_Rails[n].Count > 0)
        {
            NoteInfo info = noteInstance_Rails[n][0].noteInfo;

            if (info.type == (int)NoteType.LONG && info.press_start == true)
            {
                //누르기 시작
                isPressed[n] = true;
            }else
            {
                //성공
            }
            noteInstance_Rails[n][0].autoDestroy();
        }
    }

    void isPressUp(int n)
    {
        if (noteInstance_Rails[n].Count > 0)
        {
            if (!(noteInstance_Rails[n][0].noteInfo.type == (int)NoteType.LONG && noteInstance_Rails[n][0].noteInfo.press_start == false)) return;

            float dist = Mathf.Abs(noteInstance_Rails[n][0].transform.position.y - touchpads[n].position.y);

            if (dist < 0.4f)
            {
                if (isPressed[n])
                {
                    //성공
                    noteInstance_Rails[n][0].autoDestroy() ;
                }
            }
        }
    }
}
