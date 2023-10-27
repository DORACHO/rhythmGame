using Melanchall.DryWetMidi.Core;
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
    int touchStartedIdx;
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
        InputTestLONGNotes();     //test FINISHED_1차!!!
        //InputTestDRAGNote();
        //InputTestMIXEDNote();
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
                            //생성된 startNote 칸을 지워준다.
                            noteInstance_Rails[i].RemoveAt(0);
                        }
                    }

                    //01-2. Note_AutoDestroy
                    noteInstance.autoDestroyAction = (railIdx, noteInfo, isPassed) =>
                    {
                        //Pass without Press
                        if (isPassed) isTouchPadPressed[railIdx] = false;
                        //Pass >> remove from List                                                                             
                        noteInstance_Rails[railIdx].Remove(noteInfo);

                        if (!(noteInstance.noteInfo.type == (int)NoteType.LONG && noteInstance.noteInfo.isLongNoteStart == true))
                        {
                            showScoreText(4);                       
                        }
                    };


                }
            }
        }
        #endregion


        //02-1.scoreCheck
        #region scoreCheck by touchPhase

#if UNITY_EDITOR //check FINISHED!!!
        //if (Input.touchCount > 0)
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            //이걸로 다시 만들기
            //Drag 0번 눌르고 처음에서 began되고 마지막에서 떼지는지 체크해야함.
            //떼지않고 다른 버튼이 눌린다면 전 버튼이 사라지도록 해야함.

            if (/*touch.phase == TouchPhase.Began*/Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                {
                    //TouchPad 번호 확인
                    string touchPadName = hitInfo.transform.name;
                    touchPadName = touchPadName.Replace("Touch0", "");
                    int touchIdx = int.Parse(touchPadName) - 1;

                    touchStartedIdx = touchIdx;
                    touchedFX(touchIdx);

                    if (noteInstance_Rails[touchIdx].Count > 0)
                    {
                        //NoteType 확인
                        if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.SHORT)
                        {
                            ScoreCheck_SHORT(touchIdx);
                        }
                        else if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.LONG && noteInstance_Rails[touchIdx][0].noteInfo.isLongNoteStart)
                        {
                            EnterCheck_LONG(touchIdx);
                        }
                        else if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT || noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                        {
                            EnterCheck_DRAG(touchIdx);
                        }
                    }
                }
            }   //check FINISHED!!!

            if (/*touch.phase == TouchPhase.Moved*/Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //Ray ray = Camera.main.ScreenPointToRay(touch);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                {
                    string touchPadName = hitInfo.transform.name;
                    touchPadName = touchPadName.Replace("Touch0", "");
                    int touchIdx = int.Parse(touchPadName) - 1;

                    touchedFX(touchIdx);

                    if (noteInstance_Rails[touchStartedIdx].Count > 0)
                    {
                        if (touchStartedIdx != touchIdx)
                        {
                            print("방향 체크 전 touchId는" + touchIdx + "touchStartedIdx는" + touchStartedIdx);

                            //방향 체크
                            if (touchIdx < touchStartedIdx)     //왼쪽 드래그
                            {
                                if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                    noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    print("현재 idx는" + touchIdx + "이고" + " startedIdx는" + touchStartedIdx);
                                    print("왼쪽으로 드래그되고 있습니다");

                                    //deltaPos가 오른쪽인지를 체크하기! 같은 방향으로 움직이고 있는지
                                    PressingScore(touchStartedIdx);
                                    showScoreText(9);
                                }
                            }
                            else    //오른쪽 드래그
                            {
                                if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                    noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                {
                                    print("오른쪽으로 드래그되고 있습니다");
                                    PressingScore(touchStartedIdx);
                                    showScoreText(10);
                                }
                            }
                        }
                        else //같은 버튼을 꾹 누르는 것   checkFINISHED !!!
                        {
                            if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.LONG)
                            {
                                PressingScore(touchIdx);
                                //showScoreText(7);
                            }
                        }
                    }
                }
            }  //check FINISHED!!!

            if (/*touch.phase == TouchPhase.Ended*/Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //Ray ray = Camera.main.ScreenPointToRay(touch);
                RaycastHit hitInfo;

                releasedFX(currTouchPadIdx);
                currTouchPadIdx = -1;

                if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                {
                    string touchPadName = hitInfo.transform.name;
                    touchPadName = touchPadName.Replace("Touch0", "");
                    int touchIdx = int.Parse(touchPadName) - 1;

                    //뗀 곳의 pad 번호 확인
                    touchReleasedIdx = touchIdx;

                    if (noteInstance_Rails[touchStartedIdx].Count > 0)
                    {
                        if (touchStartedIdx != touchReleasedIdx)
                        {
                            if (noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT || noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                            {
                                if (touchReleasedIdx == noteInstance_Rails[touchStartedIdx][0].noteInfo.DRAG_release_idx)
                                {
                                    //success

                                    print("드래그 노트 성공했어요!");
                                    showScoreText(0);
                                }
                                else
                                {
                                    MissCheck();
                                }
                            }
                        }
                        else
                        {
                            if (noteInstance_Rails[touchIdx].Count > 0 && noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.LONG && !noteInstance_Rails[touchIdx][0].noteInfo.isLongNoteStart)
                            {
                                ExitCheck_LONG(touchIdx);
                            }
                        }
                    }
                }
            }   //check FINISHED!!!

        }
#endif


        if (Input.touchCount > 0)
        {
            print("현재 touchCount는" + Input.touchCount);

            for (int i = 0; i < Input.touchCount; i++)
            {
                touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hitInfo;

                    if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                    {
                        //TouchPad 번호 확인
                        string touchPadName = hitInfo.transform.name;
                        touchPadName = touchPadName.Replace("Touch0", "");
                        int touchIdx = int.Parse(touchPadName) - 1;

                        touchStartedIdx = touchIdx;
                        touchedFX(touchIdx);
                        print(i + "번째 touch일 때" + touchIdx + "번의 터치패드가 눌렸고" + "touchedFX 함수가 실행되었다.");

                        if (noteInstance_Rails[touchIdx].Count > 0) 
                        {
                            //NoteType 확인
                            if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.SHORT)
                            {
                                ScoreCheck_SHORT(touchIdx);
                            }
                            else if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.LONG)
                            {
                                EnterCheck_LONG(touchIdx);
                            }
                            else if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT || noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                            {
                                EnterCheck_DRAG(touchIdx);
                            }
                        }                       
                    }
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hitInfo;

                    if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                    {
                        string touchPadName = hitInfo.transform.name;
                        touchPadName = touchPadName.Replace("Touch0", "");
                        int touchIdx = int.Parse(touchPadName) - 1;

                        touchedFX(touchIdx);

                        if (noteInstance_Rails[touchStartedIdx].Count > 0)
                        {
                            if (touchStartedIdx != touchIdx)
                            {
                                //방향 체크
                                if (touchIdx < touchStartedIdx)     //왼쪽 드래그
                                {
                                    if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                        noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                    {
                                        print("현재 idx는" + touchIdx + "이고" + " startedIdx는" + touchStartedIdx);
                                        print("왼쪽으로 드래그되고 있습니다");

                                        //deltaPos가 오른쪽인지를 체크하기! 같은 방향으로 움직이고 있는지
                                        PressingScore(touchStartedIdx);
                                        showScoreText(9);
                                    }
                                }
                                else    //오른쪽 드래그
                                {
                                    if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                        noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                    {
                                        print("오른쪽으로 드래그되고 있습니다");
                                        PressingScore(touchStartedIdx);
                                        showScoreText(10);
                                    }
                                }
                            }
                            else //같은 버튼을 꾹 누르는 것   checkFINISHED !!!
                            {
                                if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.LONG)
                                {
                                    PressingScore(touchIdx);
                                    //showScoreText(7);
                                }
                            }
                        }



                    }

                }

                if (touch.phase == TouchPhase.Ended)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hitInfo;

                    releasedFX(currTouchPadIdx);
                    currTouchPadIdx = -1;

                    if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                    {
                        string touchPadName = hitInfo.transform.name;
                        touchPadName = touchPadName.Replace("Touch0", "");
                        int touchIdx = int.Parse(touchPadName) - 1;

                        //뗀 곳의 pad 번호 확인
                        touchReleasedIdx = touchIdx;

                        if (noteInstance_Rails[touchStartedIdx].Count > 0)
                        {
                            if (touchStartedIdx != touchReleasedIdx)
                            {
                                if (noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT || noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    if (touchReleasedIdx == noteInstance_Rails[touchStartedIdx][0].noteInfo.DRAG_release_idx)
                                    {
                                        //success

                                        print("드래그 노트 성공했어요!");
                                        showScoreText(0);
                                    }
                                    else
                                    {
                                        MissCheck();
                                    }
                                }
                            }
                            else
                            {
                                if (noteInstance_Rails[touchIdx].Count > 0 && noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.LONG && !noteInstance_Rails[touchIdx][0].noteInfo.isLongNoteStart)
                                {
                                    ExitCheck_LONG(touchIdx);
                                }
                            }
                        }
                    }

                }
            }
        }


        #endregion
    }

    //현재 touch된 부분만 켜지고 나머지는 꺼지도록 check!!!
    int currTouchPadIdx = -1;
    void touchedFX(int n)
    {
        if (n == currTouchPadIdx) return;

        
        if (currTouchPadIdx != -1)
        {
            releasedFX(currTouchPadIdx);
        }

        if (!touchpads[n].GetComponent<MeshRenderer>().enabled)
        {
            touchpads[n].GetComponent<MeshRenderer>().enabled = true;
        }

        currTouchPadIdx = n;
    }

    void releasedFX(int n)
    {
        print("releasedFX함수 실행");
        if (n == -1) return;

        if (touchpads[n].GetComponent<MeshRenderer>().enabled)
        {
            touchpads[n].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void ScoreCheck_SHORT(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        distAbs = Mathf.Abs(dist);


        if (distAbs > badZone)
        {
            //내려오는 중이니까 터치해도 의미가 없다.
            return;
            //passDestroy가 되지 않기 위함
        }
        else if (distAbs > goodZone)
        {
            //Bad
            showScoreText(3);
            EJScoreManager.instance.SCORE += badScore;
        }
        else if (distAbs > greatZone)
        {
            //Good
            showScoreText(2);
            EJScoreManager.instance.SCORE += goodScore;
        }
        else if (distAbs > excellentZone)
        {
            //Great
            showScoreText(1);
            EJScoreManager.instance.SCORE += greatScore;
        }
        else
        {
            //Excellent
            showScoreText(0);
            EJScoreManager.instance.SCORE += excellentScore;
        }

        PressDestroy(n);
    } //check_FINISHED!!!

    public void EnterCheck_LONG(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        distAbs = Mathf.Abs(dist);

        if (distAbs < badZone)
        {
            //success
            //showScoreText(5);
        }
        else
        {
            //아직 내려오기 전
            //터치패드 지난 후엔 autoDestroy           
        }
    } //check_FINISHED!!!

    public void ExitCheck_LONG(int n)
    {
        print("ExitCheck_Long 중입니다");

        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        distAbs = Mathf.Abs(dist);

        if (distAbs < badZone)
        {
            //success
            //showScoreText(6);
        }
        else if (dist > badZone)
        {
            //miss
            MissCheck();
            //unabled
        }
    }   //check_FINISHED!!!

    public void EnterCheck_DRAG(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        distAbs = Mathf.Abs(dist);


        if (distAbs < badZone)
        {
            //success
            showScoreText(11);
        }
        else
        {
            //노트가 내려오기 전           
        }
    }   //check FINISHED!!!

    public void ExitCheck_DRAG(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

        //이미 뗀 곳이 올바른 위치라는 것을 확인한 후니까!!!
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;

        if (dist < badZone)
        {
            //success
            showScoreText(8);
        }
        else
        {
            //miss
            MissCheck();
        }

    }


    public void PressingScore(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

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

    public void PressDestroy(int n)
    {
        Destroy(noteInstance_Rails[n][0].gameObject);
        noteInstance_Rails[n].RemoveAt(0);

        //note에서 FX나오기
    }

    public void MissUnabled(int n)
    {
        //long이나 drag가 눌리다가 끝까지 눌리지 못한 경우
        //passDestroy까지의 기간 동안 점수 체크가 되지 못하도록 해야함.

        //일단은 pressdestroy해둠
        Destroy(noteInstance_Rails[n][0].gameObject);
        noteInstance_Rails[n].RemoveAt(0);
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
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 2;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 3;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
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
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 2;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);


        info.railIdx = 3;
        info.type = (int)NoteType.LONG;
        info.time = 4;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 3;
        info.type = (int)NoteType.LONG;
        info.time = 5;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
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

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 1;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 1;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
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
        info.DRAG_release_idx = 5;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
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
