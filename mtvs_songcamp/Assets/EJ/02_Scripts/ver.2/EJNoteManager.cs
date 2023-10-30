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
    //임의
    int bpm = 72;
    public Camera maincam;

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

    enum DraggingState
    {
        None,
        Dragging_RIGHT,
        Dragging_LEFT,
    }

    DraggingState draggingState;

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
        //InputTestMIXEDNote();
        InputTestFLOP();
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
                //if (currTime >= noteInfo_Rails[i][0].time)
                if (currTime >= noteInfo_Rails[i][0].time / bpm)
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

                    #region 함수로 묶어준 부분 안되면 풀기
                    //noteInstance.noteInfo = noteInfo_Rails[i][0];
                    //noteInstance_Rails[i].Add(noteInstance);
                    ////Instantiated되면 대기열에서 지워주기
                    //noteInfo_Rails[i].RemoveAt(0);
                    #endregion

                    noteInstantiate(i, noteInstance);

                    //01-1-2.NoteType_LONG
                    //LONG이라면 endNote를 생성
                    if (noteInstance.noteInfo.type == (int)NoteType.LONG)
                    {
                        if (noteInstance.noteInfo.isLongNoteStart)
                        {
                            print("*00000 noteInstantiate 실행 - noteInstance의 type은" + noteInstance.noteInfo.type + "noteInstance의 isLongStart는" + noteInstance.noteInfo.isLongNoteStart + "현재열의 0번에 담긴 것은" + noteInstance_Rails[i][0]);

                            startNoteArr[i] = noteInstance;
                            //startNote = firstNoteInstance.gameObject;
                        }
                        else
                        {
                            print("*11111 noteInstantiate 실행 - noteInstance의 type은" + noteInstance.noteInfo.type + "noteInstance의 isLongStart는" + noteInstance.noteInfo.isLongNoteStart + "현재열의 0번에 담긴 것은" + noteInstance_Rails[i][0]);

                            int startNoteIdx = noteInstance_Rails[i].Count - 1 - 1;
                            noteInstance_Rails[i][startNoteIdx].GetComponent<EJNote>().connectNote(noteInstance.gameObject);
                            
                            //생성된 startNote 칸을 지워준다.
                            //그래야 endNote를 0번째 인덱스로 체크할 수 있으니까
                            //noteInstance_Rails[i].RemoveAt(0);
                            //noteRemove(i);
                            print("*22222 noteInstantiate 실행 - noteInstance의 type은" + noteInstance.noteInfo.type + "noteInstance의 isLongStart는" + noteInstance.noteInfo.isLongNoteStart + "현재열의 0번에 담긴 것의 isLongNoteStart는" + noteInstance_Rails[i][0].noteInfo.isLongNoteStart);
                        }
                    }


                    //01-2. Note_AutoDestroy
                    //print("*55555 noteInstance의 enable상태는" + noteInstance.noteInfo.isNoteEnabled);

                    noteInstance.autoDestroyAction = (railIdx, noteInfo, isPassed) =>
                    {
                        //Pass without Press
                        if (isPassed) isTouchPadPressed[railIdx] = false;
                        //Pass >> remove from List
                        //
                        noteInstance_Rails[railIdx].Remove(noteInfo);

                        //if (noteInstance.noteInfo.type == (int)NoteType.LONG && !noteInstance.noteInfo.isLongNoteStart && noteInstance.noteInfo.isNoteEnabled) return;

                        if (noteInstance.noteInfo.isNoteEnabled)
                        {
                            //LongNote가 성공하고도 계속 눌려있는 경우기 때문에 miss가 아님!
                            if (noteInstance.noteInfo.type == (int)NoteType.LONG && !noteInstance.noteInfo.isLongNoteStart)
                            {

                            }
                            else
                            {
                                print("*****현재 autoDestroyAction에 담긴 실행되는 Note의 isLongStart값은" + noteInstance.noteInfo.isLongNoteStart);
                                //showScoreText(4);
                                EJScoreManager.instance.StartShowScoreText("Miss",railIdx,0);
                                EJcamShake.instance.StartShake(0.2f, 0.5f, 1);
                            }
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

                    dicCurrTouchPadIdx[0] = -1;
                    touchedFX(touchIdx, 0);

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

                    touchedFX(touchIdx, 0);

                    if (noteInstance_Rails[touchStartedIdx].Count > 0)
                    {
                        if (touchStartedIdx != touchIdx)
                        {
                            print("방향 체크 전 touchId는" + touchIdx + "touchStartedIdx는" + touchStartedIdx);

                            //방향 체크
                            if (touchIdx < touchStartedIdx)     //왼쪽 드래그
                            {
                                draggingState = DraggingState.Dragging_LEFT;

                                if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                    noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    print("현재 idx는" + touchIdx + "이고" + " startedIdx는" + touchStartedIdx);
                                    print("왼쪽으로 드래그되고 있습니다");

                                    //deltaPos가 오른쪽인지를 체크하기! 같은 방향으로 움직이고 있는지
                                    PressingScore(touchStartedIdx);
                                    //showScoreText(9);
                                }
                            }
                            else    //오른쪽 드래그
                            {
                                draggingState = DraggingState.Dragging_RIGHT;

                                if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                    noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                {
                                    print("오른쪽으로 드래그되고 있습니다");
                                    PressingScore(touchStartedIdx);
                                    //showScoreText(10);
                                }
                            }
                        }
                        else //같은 버튼을 꾹 누르는 것   checkFINISHED !!!
                        {
                            draggingState = DraggingState.None;

                            if (noteInstance_Rails[touchIdx][0].noteInfo.type == (int)NoteType.LONG)
                            {
                                PressingScore(touchIdx);
                                print("*55666 LongNote가 눌리고 있습니다");
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

                releasedFX(0);
                dicCurrTouchPadIdx.Remove(0);

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
                                ExitCheck_DRAG(touchStartedIdx);
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
                //touch.fingerId = i;

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

                        dicCurrTouchPadIdx[touch.fingerId] = -1;
                        touchedFX(touchIdx, touch.fingerId);
                        print(i + "번째 touch일 때" + touchIdx + "번의 터치패드가 눌렸고" + "touchedFX 함수가 실행되었다.");

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

                        touchedFX(touchIdx, touch.fingerId);

                        if (noteInstance_Rails[touchStartedIdx].Count > 0)
                        {
                            if (touchStartedIdx != touchIdx)
                            {
                                //방향 체크
                                if (touchIdx < touchStartedIdx)     //왼쪽 드래그
                                {
                                    draggingState = DraggingState.Dragging_LEFT;

                                    if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                        noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                    {
                                        print("현재 idx는" + touchIdx + "이고" + " startedIdx는" + touchStartedIdx);
                                        print("왼쪽으로 드래그되고 있습니다");

                                        //deltaPos가 오른쪽인지를 체크하기! 같은 방향으로 움직이고 있는지
                                        PressingScore(touchStartedIdx);
                                        //showScoreText(9);
                                    }
                                }
                                else    //오른쪽 드래그
                                {
                                    draggingState = DraggingState.Dragging_RIGHT;

                                    if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                        noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                    {
                                        print("오른쪽으로 드래그되고 있습니다");
                                        PressingScore(touchStartedIdx);
                                        //showScoreText(10);
                                    }
                                }
                            }
                            else //같은 버튼을 꾹 누르는 것   checkFINISHED !!!
                            {
                                draggingState = DraggingState.None;

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

                    releasedFX(touch.fingerId);
                    dicCurrTouchPadIdx.Remove(touch.fingerId);

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
                                    ExitCheck_DRAG(touchStartedIdx);
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


    void noteInstantiate(int n, EJNote noteInstance)
    {
        //현재 리스트에 대기열의 정보를 넣어서 Insert
        noteInstance.noteInfo = noteInfo_Rails[n][0];
        noteInstance_Rails[n].Add(noteInstance);
        //대기열에서 Remove
        noteInfo_Rails[n].RemoveAt(0);
    }

    void noteRemove(int n)
    {
        //현재 리스트에서 Remove
        noteInstance_Rails[n].RemoveAt(0);
    }

    void noteUnable(int n)
    {
        //현재 리스트의 unable
        noteInstance_Rails[n][0].noteInfo.isNoteEnabled = false;

        //현재 리스트에서 remove
        //noteInstance_Rails[n].RemoveAt(0);
    }

    //현재 touch된 부분만 켜지고 나머지는 꺼지도록 check!!!
    //int currTouchPadIdx = -1;

    //Dictionary<fingerID, railIdx>
    Dictionary<int, int> dicCurrTouchPadIdx = new Dictionary<int, int>();

    //fingerId별 startIdx가 필요    
    //*****Dictionary<fingerID, startIdx>
    Dictionary<int, int> dicStartIdx = new Dictionary<int, int>();

    void touchedFX(int n, int fingerId)
    {
        if (dicCurrTouchPadIdx.ContainsKey(fingerId) == false) return;
        if (n == dicCurrTouchPadIdx[fingerId]) return;


        if (dicCurrTouchPadIdx[fingerId] != -1)
        {
            releasedFX(fingerId);
        }

        if (!touchpads[n].GetComponent<MeshRenderer>().enabled)
        {
            touchpads[n].GetComponent<MeshRenderer>().enabled = true;
        }

        dicCurrTouchPadIdx[fingerId] = n;
    }

    void releasedFX(int fingerId)
    {
        if (dicCurrTouchPadIdx.ContainsKey(fingerId) == false) return;
        int n = dicCurrTouchPadIdx[fingerId];
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
            //showScoreText(3);
            EJScoreManager.instance.StartShowScoreText("Bad",n,badScore);
            EJScoreManager.instance.SCORE += badScore;
        }
        else if (distAbs > greatZone)
        {
            //Good
            //showScoreText(2);
            EJScoreManager.instance.StartShowScoreText("Good",n,goodScore);
            EJScoreManager.instance.SCORE += goodScore;
        }
        else if (distAbs > excellentZone)
        {
            //Great
            //showScoreText(1);
            EJScoreManager.instance.StartShowScoreText("Great",n,greatScore);
            EJScoreManager.instance.SCORE += greatScore;
        }
        else
        {
            //Excellent
            //showScoreText(0);
            EJScoreManager.instance.StartShowScoreText("Excellent", n, excellentScore);
            EJScoreManager.instance.SCORE += excellentScore;
        }

        Handheld.Vibrate();
        PressDestroy(n);
    } //check_FINISHED!!!

    public void EnterCheck_LONG(int n)
    {
        print("*33333 EnterCheckLong이 실행되었고 현재 noteInstance의 0번째 칸엔" + noteInstance_Rails[n][0] + "이 담겼다");
        if (noteInstance_Rails[n][0] == null) return;

        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        distAbs = Mathf.Abs(dist);


        if (distAbs < badZone)
        {
           
            //success
            print("*44444 LongNote가 Enter에 성공했고 현재 noteInstance의 0번째 칸의 isLongNotestart값은" + noteInstance_Rails[n][0].noteInfo.isLongNoteStart + "이 담겼다");
            //showScoreText(5);
            noteUnable(n);
            noteRemove(n);
            print("*55555 LongNote가 Enter에 성공했고 noteRemove실행 후 현재 noteInstance의 0번째 칸의 isLongNotestart값은" + noteInstance_Rails[n][0].noteInfo.isLongNoteStart + "이 담겼다");
        }
        else
        {
            //아직 내려오기 전
            //터치패드 지난 후엔 autoDestroy
            print("***** startNote에 대한 badZone이후가 들어옴");
            noteUnable(n);
            //성공하면 endNote와 체크하도록 지워줘야함
            noteRemove(n);

        }
    } //check_FINISHED!!!

    public void ExitCheck_LONG(int n)
    {
        print("*66666 LongNote에 대한 ExitCheck가 실행되었다.");

        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        distAbs = Mathf.Abs(dist);

        if (distAbs < badZone)
        {
            //success
            //showScoreText(0);
            EJScoreManager.instance.StartShowScoreText("Excellent", n, excellentScore);
            //noteUnable(n);
            //misscheck를 하면 안된다!!!

        }
        else if (dist > badZone)
        {
            //miss
            print("*77777 LongNote에 대한 Exit이 실패했고, 현재 longNote의 enable상태는" + noteInstance_Rails[n][0].noteInfo.isNoteEnabled);
            noteUnable(n);
            print("*88888 LongNote에 대한 Exit이 실패했고, unable함수 실행 후, 현재 longNote의 enable상태는" + noteInstance_Rails[n][0].noteInfo.isNoteEnabled);

            //noteInstance_Rails[n].RemoveAt(0);

            MissCheck();
            print("*99999 longNoteExitCheck에서의 misscheck가 실행되었다");
            //print("*22222-2 remove함수 실행 후" + noteInstance_Rails[n][0] + "은 null 이어야 하는데!");
            //unabled
        }    
        
        noteRemove(n);
        //noteInstance_Rails[n].RemoveAt(0);
    }   //check_FINISHED!!!

    public void EnterCheck_DRAG(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        distAbs = Mathf.Abs(dist);


        if(distAbs < badZone)
        {
            //success
            print("***dragEnter 성공 했다!");
        }
        //else if (dist < 0)
        //{
        //    //혹시나 startIndex누르지 않고 autoDestroy에 걸리는 타이밍엔 unable 상태 체크 안해도 될거 같긴 함.
        //    noteUnable(n);
        //    print("***noteUnable 되었다!");
        //    return;
        //}


    }   //check FINISHED!!!

    public void ExitCheck_DRAG(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

        //이미 뗀 곳이 올바른 위치라는 것을 확인한 후니까!!!
        distAbs = Mathf.Abs(touchpads[n].transform.position.y - noteInstance_Rails[n][0].transform.position.y);
        dist = noteInstance_Rails[n][0].transform.position.y - touchpads[n].transform.position.y;
        
        if (distAbs < badZone)
        {
            //success
            if (touchReleasedIdx == noteInstance_Rails[touchStartedIdx][0].noteInfo.DRAG_release_idx && draggingState == DraggingState.Dragging_LEFT && noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
            {
                //success

                print("왼쪽 드래그 노트 성공했어요!");
                PressDestroy(touchStartedIdx);
                //showScoreText(0);
                EJScoreManager.instance.StartShowScoreText("Excellent",n,excellentScore);
            }
            else if (touchReleasedIdx == noteInstance_Rails[touchStartedIdx][0].noteInfo.DRAG_release_idx && draggingState == DraggingState.Dragging_RIGHT && noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
            {
                print("오른쪽 드래그 노트 성공했어요!");
                PressDestroy(touchStartedIdx);
                //showScoreText(0);
                EJScoreManager.instance.StartShowScoreText("Excellent", n, excellentScore);
            }
            else
            {
                MissCheck();
            }

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

        //if (distAbs < badZone)
        {
            EJScoreManager.instance.SCORE += pressScore * Time.deltaTime;

        }
        //else
        {
            //note가 판정 범위 내에 있지 않은 경우 score증가 X
        }
    }

    public void MissCheck()
    {
        //showScoreText(4);
        EJScoreManager.instance.StartShowScoreText("Miss",0,0);

        //안되는 이유..? 한프레임이라?
        EJcamShake.instance.StartShake(0.2f, 0.5f, 1);      
    }

    public void PressDestroy(int n)
    {
        Destroy(noteInstance_Rails[n][0].gameObject);
        noteInstance_Rails[n].RemoveAt(0);

        //note에서 FX나오기
    }

    public void MissUnabled(int n)
    {
        print("note가 enabled == false되었다");
        //long이나 drag가 눌리다가 끝까지 눌리지 못한 경우
        //passDestroy까지의 기간 동안 점수 체크가 되지 못하도록 해야함.

        noteInstance_Rails[n][0].noteInfo.isNoteEnabled = false;
        noteInstance_Rails[n].RemoveAt(0);
    }

    //01. NoteType.SHORT test
    #region SHORT
    void InputTestSHORTNotes()
    {
        NoteInfo info = new NoteInfo();

        info.railIdx = 0;
        info.type = (int)NoteType.SHORT;
        info.time = 1 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 2 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 3 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 4 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 5 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 6 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
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
        info.time = 1 *bpm;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 4 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);


        //info.railIdx = 3;
        //info.type = (int)NoteType.LONG;
        //info.time = 4;
        //info.isLongNoteStart = true;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info.railIdx = 3;
        //info.type = (int)NoteType.LONG;
        //info.time = 5;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

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
        info.time = 1*bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 4 * bpm;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 4 * bpm;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 4 * bpm;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 6 * bpm;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 6 * bpm;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

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
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 3;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 7;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 9;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 2;
        info.isLongNoteStart = true;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 4;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 7;
        info.isLongNoteStart = true;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 8;
        info.isLongNoteStart = false;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
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

    //05. FLOP test
    #region FLOP
    void InputTestFLOP()
    {
        NoteInfo info = new NoteInfo();

        #region Pattern01

        //마디 1) Pattern 1 - Short
        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 1;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 31;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 61;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 76;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 121;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 151;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 181;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 211;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 2) Pattern 1
        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 241;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 271;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 301;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 316;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 361;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 391;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 421;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 451;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 3) Pattern 1
        info = new NoteInfo();
        info.railIdx = 0;
        info.type = (int)NoteType.SHORT;
        info.time = 481;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 511;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 541;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 556;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 601;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 631;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 661;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 691;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 4) Pattern 1
        info = new NoteInfo();
        info.railIdx = 0;
        info.type = (int)NoteType.SHORT;
        info.time = 721;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 751;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 781;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 796;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 841;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 871;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 901;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 931;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);


        #endregion

        #region Pattern02
        //마디 5) Pattern 2
        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 961;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 976;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 983.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 998.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1006;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1021;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1028.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1043.5f;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1058.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1081f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1096f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1103.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1118.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1126f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1141f;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1178.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 6) Pattern 2
        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1201;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1216;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1223.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1238.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1246;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 1261;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1305;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1320;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1327.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1342.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1350;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.LONG;
        info.time = 1365;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 5;
        info.type = (int)NoteType.LONG;
        info.time = 1411;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 7) Pattern 2
        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1441;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1456;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1463.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1478.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1486;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1501;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1508.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1523.5f;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //***
        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1550;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1561;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1576;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1583.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1598.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1606;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1621;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.LONG;
        info.time = 1658.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 8) Pattern 2

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1681;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1696;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1703.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1718.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1726;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 1741;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1785;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1800;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 1807.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 1822.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1830;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.LONG;
        info.time = 1845;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.LONG;
        info.time = 1891;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        #endregion

        #region Pattern03

        //마디 9) Pattern 3
        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 1921;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 1921;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 1951;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 1981;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 1981;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 2011;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2041;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2041;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2071;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 2101;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2101;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2131;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //마디 10) Pattern 3

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2161;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2161;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 2191;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2221;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2221;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2251;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 2281;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2281;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2311;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2341;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2363.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 2371;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 2393.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 11) Pattern 3
        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2401;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2401;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 2431;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2461;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2461;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2491;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.DRAG_RIGHT;
        info.time = 2521;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 5;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2521;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2551;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2581;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2581;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 2611;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //마디 12) Pattern 3
        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2641;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2641;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2671;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2701;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 2701;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = 0;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2731;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.DRAG_RIGHT;
        //info.time = 2761;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 5;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.DRAG_LEFT;
        //info.time = 2761;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = 0;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 2791;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 2821;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 2;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2843.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 2851;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 2873.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);
        #endregion

        #region Pattern04

        //마디 13-14) Pattern 4
        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.LONG;
        info.time = 2881;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.LONG;
        info.time = 3090;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 3091;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 3300;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = false;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 3301;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 3323.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 3331;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 3353.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //마디 15-16) Pattern 4

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.LONG;
        info.time = 3361;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.LONG;
        info.time = 3570;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 3571;
        info.isLongNoteStart = true;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 3780;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 4;
        //info.type = (int)NoteType.SHORT;
        //info.time = 3781;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        //info = new NoteInfo();
        //info.railIdx = 3;
        //info.type = (int)NoteType.SHORT;
        //info.time = 3803.5f;
        //info.isLongNoteStart = false;
        //info.DRAG_release_idx = -1;
        //info.isNoteEnabled = true;
        //allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 3811;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 3833.5f;
        info.isLongNoteStart = false;
        info.DRAG_release_idx = -1;
        info.isNoteEnabled = true;
        allNoteInfo.Add(info);

        #endregion


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
        GameObject scoreText = Instantiate(scoreTexts[n], canvas.transform.position - Vector3.forward*2, Quaternion.identity);
        scoreText.transform.SetParent(canvas.transform);

        Destroy(scoreText, 0.5f);
    }

}
