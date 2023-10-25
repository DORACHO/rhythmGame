using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//01. Note_Instantiate & Destroy
//02. scoreCheck

public class EJNoteManager : MonoBehaviour
{
    //01. Note_Instantiate
    public GameObject[] notePrefabs;
    public Transform[] noteSpawnRail;
    public Transform[] touchpads;

    GameObject note;
    GameObject startNote;
    GameObject endNote;

    const int railCount = 6;
    float currTime;

    //01-1.noteData _ 일종의 대기열 느낌
    List<NoteInfo> allNoteInfo = new List<NoteInfo>();
    List<NoteInfo>[] noteInfo_Rails = new List<NoteInfo>[railCount];

    //01-2.Hierarchy - instance noteData
    List<EJNote>[] noteInstance_Rails = new List<EJNote>[railCount];
    EJNote[] startNoteArr = new EJNote[railCount];

    //02. Note_pressCheck
    bool[] isTouchPadPressed = new bool[railCount];
    bool[] isDragPressed = new bool[railCount];
    int touchReleasedIdx;

    Touch touch;
    TouchPhase phase;
    Vector2 deltaPos;

    public Material missMat;

    //03. scoreCheck;
    float distAbs;     //touchPad와 note사이의 거리 체크
    float dist;

    float badZone = 2.9f;
    float goodZone = 2f;
    float greatZone = 1f;
    float excellentZone = 0.3f;

    int badScore = 1;
    int goodScore = 2;
    int greatScore = 3;
    int excellentScore = 5;
    int missScore = -1;

    float pressScore = 1;

    public Canvas canvas;
    public GameObject[] scoreTexts;

    void Start()
    {
        // instantiated note in hierarchy <<< Add EJNote Component 
        for (int i = 0; i < noteInstance_Rails.Length; i++)
        {
            //notes properties list per Rails
            noteInstance_Rails[i] = new List<EJNote>();
        }

        //InputTestSHORTNotes();    //test FINISHED!!!
        //InputTestLONGNotes();     //test FINISHED_1차!!!
        //InputTestDRAGNote();
        InputTestMIXEDNote();
    }

    void Update()
    {
        currTime += Time.deltaTime;

        //01. Note_Instantiate & Destroy    //test FINISHED!!!
        #region 01. Note_Instantiate & Destroy

        //01-1. Note_Instantiate
        //note instantiate per rails

        //0~5까지 반복하면서 ex) 0번 레일일 때
        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            // ex) 0번 레일에 만들어질 노트가 있다면
            // i = railIndex 체크 중
            if (noteInfo_Rails[i].Count > 0)
            {
                //Note_Instantiate on Time
                //대기열에 있는 0번 레일의 0번 노트의 생성시간에 생성
                if (currTime >= noteInfo_Rails[i][0].time)
                {
                    //Note_Instantiate by NoteType, SpawnRail
                    //01-1-1.NoteType
                    //notePrefabs[type], noteSpawnRail[0],
                    note = Instantiate(notePrefabs[noteInfo_Rails[i][0].type], noteSpawnRail[i].position + Vector3.forward * (-0.5f), Quaternion.identity);

                    note.transform.forward = notePrefabs[0].transform.forward;
                    note.transform.SetParent(noteSpawnRail[i].transform);

                    //현재 instantiated된 Note의 info에 대기열의 정보를 담아주고
                    //새로운 리스트의 배열에 넣어주고 싶음.
                    EJNote noteInstance = note.GetComponent<EJNote>();
                    noteInstance.noteInfo = noteInfo_Rails[i][0];
                    noteInstance_Rails[i].Add(noteInstance);

                    //Instantiated되면 대기열에서 지워주기
                    noteInfo_Rails[i].RemoveAt(0);

                    //01-1-2.NoteType_LONG
                    //LONG이라면 endNote를 생성
                    if (noteInstance.noteInfo.type == (int)NoteType.LONG)
                    {
                        if (noteInstance.noteInfo.isLongNoteStart)
                        {
                            print("LongNote의 StartNote입니다");
                            startNoteArr[i] = noteInstance;
                            //startNote = firstNoteInstance.gameObject;
                        }
                        else
                        {
                            int startNoteIdx = noteInstance_Rails[i].Count - 1 - 1;
                            noteInstance_Rails[i][startNoteIdx].GetComponent<EJNote>().connectNote(noteInstance.gameObject);
                        }
                    }

                    //01-2. Note_AutoDestroy
                    noteInstance.autoDestroyAction = (railIdx, noteInfo, isPassed) =>
                    {
                        //Pass without Press
                        if (isPassed) isTouchPadPressed[railIdx] = false;
                        //Pass >> remove from List                                                                             
                        noteInstance_Rails[railIdx].Remove(noteInfo);
                        showScoreText(4);
                    };


                }
            }
        }
        #endregion


        //02-1.scoreCheck
        #region scoreCheck by touchPhase

        if (Input.touchCount > 0)
        //if(Input.GetMouseButton(0))
        {
            for (int i = 0; i < Input.touchCount; i++)            
            {
                touch = Input.GetTouch(i);
                //Vector3 touch = Input.mousePosition;

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                //Ray ray = Camera.main.ScreenPointToRay(touch);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                {
                    //j = railCount 
                    for (int j = 0; j < noteInstance_Rails.Length; j++)
                    {
                        //if (noteInstance_Rails[j].Count == 0) continue;
                        //continue: if 조건이 참이라면, 현재 반복을 중단하고 다음 반복을 시작합니다. 
                        //해당 레일에 note가 생성되어 있을 때마 터치가 먹는 거임.

                        if (hitInfo.transform.gameObject == touchpads[j].gameObject)
                        {
                            if (touch.phase == TouchPhase.Began)
                            {
                                //isPressDown(j);
                                touchpads[j].GetComponent<MeshRenderer>().enabled = true;   //이 줄까지 들어오는지 확인
                                //Debug.Log("touchPad의 인덱스는" + j);

                                if (noteInstance_Rails[j].Count == 0) continue;

                                //short Note score check (0~5)
                                if (noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.SHORT)
                                {
                                    ScoreCheck_SHORT(j);
                                }
                                //long Note success check (0,1)
                                else if (noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.LONG)
                                {
                                    SuccessEnter_LONG(j);
                                }
                                //drag Note success check (0,1)
                                else if (noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.DRAG_RIGHT || noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    SuccessEnter_DRAG(j);
                                }

                            }

                            if (touch.phase == TouchPhase.Moved)
                            {
                                if (noteInstance_Rails[j].Count == 0) continue;

                                //long, drag Notes score ++ 
                                if (noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.LONG || noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.DRAG_RIGHT || noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    PressingScore(j);
                                }
                            }

                            if (touch.phase == TouchPhase.Ended)
                            {
                                //isPressUp(j);
                                touchpads[j].GetComponent<MeshRenderer>().enabled = false;

                                if (noteInstance_Rails[j].Count == 0) continue;

                                //long Note score check(0~5)
                                if (noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.LONG && noteInstance_Rails[j][0].noteInfo.isLongNoteStart == false)
                                {
                                    SuccessExit_LONG(j);
                                }

                                //drag Note success check by deltaPosition, released index
                                if (noteInstance_Rails[j - 2][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                {
                                    //touchPad눌린 것보다 2칸 왼쪽의 note.type이 drag_right여야
                                    SuccessExit_DRAG(j);
                                }
                                else if (noteInstance_Rails[j - 1][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                {
                                    //miss
                                    MissCheck();
                                }
                                else if (noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.DRAG_RIGHT) 
                                {
                                    //miss
                                    MissCheck();
                                }

                                if (noteInstance_Rails[j + 2][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    //touchPad눌린 것보다 2칸 오른쪽의 note.type이 drag_left여야
                                    SuccessExit_DRAG(j);
                                }
                                else if (noteInstance_Rails[j + 1][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    //miss
                                    MissCheck();
                                }
                                else if (noteInstance_Rails[j][0].noteInfo.type == (int)NoteType.DRAG_LEFT) 
                                {
                                    //miss
                                    MissCheck();
                                }

                            }
                        }
                    }
                }
            }
            //Debug.LogError("에러체크");
        }


        #endregion
    }

    public void ScoreCheck_SHORT(int n)
    {
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;

        //01.excellentZone
        if (distAbs >= 0 && distAbs < excellentZone)
        {
            //excellent
            showScoreText(0);
            EJScoreManager.instance.SCORE += excellentScore;
        }
        else if (distAbs >= excellentZone && distAbs < greatZone)
        {
            //great
            showScoreText(1);
            EJScoreManager.instance.SCORE += greatScore;
        }
        else if (distAbs >= greatZone && distAbs < goodZone)
        {
            //good
            showScoreText(2);
            EJScoreManager.instance.SCORE += goodScore;
        }
        else if (distAbs >= goodZone && distAbs < badZone)
        {
            //bad
            showScoreText(3);
            EJScoreManager.instance.SCORE += badScore;
        }
        else if (dist < 0)
        {
            //miss
            MissCheck();
            EJScoreManager.instance.SCORE += missScore;
        }
        else
        {
            //note가 아직 touchpad에 닿기 전
        }
    }

    public void SuccessEnter_LONG(int n)
    {
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;

        if (distAbs < badZone)
        {
            //success
        }
        else if (dist < 0)
        {
            //miss
            MissCheck();
        }
    }

    public void SuccessExit_LONG(int n)
    {
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;

        //noteInstance_Rails[j][0]
        //float distNend = Mathf.Abs(touchpads.transform.position.y - endNote.transform.position.y);

        if (distAbs < badZone)
        {
            //success
        }
        else if (dist < 0)
        {
            //miss
            MissCheck();
        }
    }

    public void SuccessEnter_DRAG(int n)
    {
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;

        if (distAbs < badZone)
        {
            //success
        }
        else if (dist < 0)
        {
            //miss
            MissCheck();
        }
    }

    public void SuccessExit_DRAG(int n)
    {
        //이미 뗀 곳이 올바른 위치라는 것을 확인한 후니까
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;

        if (dist < badZone)
        {
            //success
        }
        else
        {
            //miss
            MissCheck();
        }

    }


    public void PressingScore(int n)
    {
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;

        if (distAbs < badZone)
        {
            EJScoreManager.instance.SCORE += pressScore * Time.deltaTime;

        }
        else
        {
            //note가 판정 범위 내에 있지 않은 경우 score증가 X
        }
    }

    public void MissCheck()
    {
        showScoreText(4);
    }

    //01. NoteType.SHORT test
    #region SHORT
    void InputTestSHORTNotes()
    {
        NoteInfo info = new NoteInfo();

        info.railIdx = 0;
        info.type = (int)NoteType.SHORT;
        info.time = 1;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 2;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 3;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            noteInfo_Rails[i] = new List<NoteInfo>();
        }

        for (int i = 0; i < allNoteInfo.Count; i++)
        {
            noteInfo_Rails[allNoteInfo[i].railIdx].Add(allNoteInfo[i]);
        }
    }
    #endregion

    //02. NoteType.Long test
    #region LONG
    void InputTestLONGNotes()
    {
        NoteInfo info = new NoteInfo();

        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 1;
        info.isLongNoteStart = true;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 2;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);


        info.railIdx = 3;
        info.type = (int)NoteType.LONG;
        info.time = 3;
        info.isLongNoteStart = true;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 3;
        info.type = (int)NoteType.LONG;
        info.time = 5;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            noteInfo_Rails[i] = new List<NoteInfo>();
        }

        for (int i = 0; i < allNoteInfo.Count; i++)
        {
            noteInfo_Rails[allNoteInfo[i].railIdx].Add(allNoteInfo[i]);
        }
    }
    #endregion

    //03. NoteType.Drag test
    #region DRAG
    void InputTestDRAGNote()
    {
        NoteInfo info = new NoteInfo();

        //info.railIdx = 1;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1;
        //info.isLongNoteStart = false;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2;
        //info.isLongNoteStart = false;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 3;
        //info.isLongNoteStart = false;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 4;
        //info.isLongNoteStart = false;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        //info.railIdx = 5;
        //info.type = (int)NoteType.SHORT;
        //info.time = 5;
        //info.isLongNoteStart = false;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        //info.railIdx = 6;
        //info.type = (int)NoteType.SHORT;
        //info.time = 6;
        //info.isLongNoteStart = false;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.LONG;
        //info.time = 2;
        //info.isLongNoteStart = true;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.LONG;
        //info.time = 3;
        //info.isLongNoteStart = false;
        //info.press_idx = 0;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            noteInfo_Rails[i] = new List<NoteInfo>();
        }

        for (int i = 0; i < allNoteInfo.Count; i++)
        {
            noteInfo_Rails[allNoteInfo[i].railIdx].Add(allNoteInfo[i]);
        }

    }
    #endregion

    //04. Mixed test
    #region MIXED
    void InputTestMIXEDNote()
    {
        NoteInfo info = new NoteInfo();

        info.railIdx = 0;
        info.type = (int)NoteType.SHORT;
        info.time = 1;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 3;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 4;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 7;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 9;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 2;
        info.isLongNoteStart = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 4;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 7;
        info.isLongNoteStart = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 8;
        info.isLongNoteStart = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.released_idx = 5;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.released_idx = 0;
        allNoteInfo.Add(info);

        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            noteInfo_Rails[i] = new List<NoteInfo>();
        }

        for (int i = 0; i < allNoteInfo.Count; i++)
        {
            noteInfo_Rails[allNoteInfo[i].railIdx].Add(allNoteInfo[i]);
        }

    }

    #endregion


    //scoreManager Script로 이전
    void showScoreText(int n)
    {
        GameObject scoreText = Instantiate(scoreTexts[n], canvas.transform.position - Vector3.forward, Quaternion.identity);
        scoreText.transform.SetParent(canvas.transform);

        Destroy(scoreText, 0.5f);
    }

}
