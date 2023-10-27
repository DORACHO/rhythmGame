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

    //01-1.noteData _ ������ ��⿭ ����
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
    float distAbs;     //touchPad�� note������ �Ÿ� üũ
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
        InputTestLONGNotes();     //test FINISHED_1��!!!
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

        //0~5���� �ݺ��ϸ鼭 ex) 0�� ������ ��
        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            // ex) 0�� ���Ͽ� ������� ��Ʈ�� �ִٸ�
            // i = railIndex üũ ��
            if (noteInfo_Rails[i].Count > 0)
            {
                //Note_Instantiate on Time
                //��⿭�� �ִ� 0�� ������ 0�� ��Ʈ�� �����ð��� ����
                if (currTime >= noteInfo_Rails[i][0].time)
                {
                    //Note_Instantiate by NoteType, SpawnRail
                    //01-1-1.NoteType
                    //notePrefabs[type], noteSpawnRail[0],
                    note = Instantiate(notePrefabs[noteInfo_Rails[i][0].type], noteSpawnRail[i].position + Vector3.forward * (-0.5f), Quaternion.identity);

                    note.transform.forward = notePrefabs[0].transform.forward;
                    note.transform.SetParent(noteSpawnRail[i].transform);

                    //���� instantiated�� Note�� info�� ��⿭�� ������ ����ְ�
                    //���ο� ����Ʈ�� �迭�� �־��ְ� ����.
                    EJNote noteInstance = note.GetComponent<EJNote>();
                    noteInstance.noteInfo = noteInfo_Rails[i][0];
                    noteInstance_Rails[i].Add(noteInstance);

                    //Instantiated�Ǹ� ��⿭���� �����ֱ�
                    noteInfo_Rails[i].RemoveAt(0);

                    //01-1-2.NoteType_LONG
                    //LONG�̶�� endNote�� ����
                    if (noteInstance.noteInfo.type == (int)NoteType.LONG)
                    {
                        if (noteInstance.noteInfo.isLongNoteStart)
                        {
                            print("LongNote�� StartNote�Դϴ�");
                            startNoteArr[i] = noteInstance;
                            //startNote = firstNoteInstance.gameObject;
                        }
                        else
                        {
                            int startNoteIdx = noteInstance_Rails[i].Count - 1 - 1;
                            noteInstance_Rails[i][startNoteIdx].GetComponent<EJNote>().connectNote(noteInstance.gameObject);
                            //������ startNote ĭ�� �����ش�.
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
            //�̰ɷ� �ٽ� �����
            //Drag 0�� ������ ó������ began�ǰ� ���������� �������� üũ�ؾ���.
            //�����ʰ� �ٸ� ��ư�� �����ٸ� �� ��ư�� ��������� �ؾ���.

            if (/*touch.phase == TouchPhase.Began*/Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                {
                    //TouchPad ��ȣ Ȯ��
                    string touchPadName = hitInfo.transform.name;
                    touchPadName = touchPadName.Replace("Touch0", "");
                    int touchIdx = int.Parse(touchPadName) - 1;

                    touchStartedIdx = touchIdx;
                    touchedFX(touchIdx);

                    if (noteInstance_Rails[touchIdx].Count > 0)
                    {
                        //NoteType Ȯ��
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
                            print("���� üũ �� touchId��" + touchIdx + "touchStartedIdx��" + touchStartedIdx);

                            //���� üũ
                            if (touchIdx < touchStartedIdx)     //���� �巡��
                            {
                                if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                    noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                {
                                    print("���� idx��" + touchIdx + "�̰�" + " startedIdx��" + touchStartedIdx);
                                    print("�������� �巡�׵ǰ� �ֽ��ϴ�");

                                    //deltaPos�� ������������ üũ�ϱ�! ���� �������� �����̰� �ִ���
                                    PressingScore(touchStartedIdx);
                                    showScoreText(9);
                                }
                            }
                            else    //������ �巡��
                            {
                                if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                    noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                {
                                    print("���������� �巡�׵ǰ� �ֽ��ϴ�");
                                    PressingScore(touchStartedIdx);
                                    showScoreText(10);
                                }
                            }
                        }
                        else //���� ��ư�� �� ������ ��   checkFINISHED !!!
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

                    //�� ���� pad ��ȣ Ȯ��
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

                                    print("�巡�� ��Ʈ �����߾��!");
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
            print("���� touchCount��" + Input.touchCount);

            for (int i = 0; i < Input.touchCount; i++)
            {
                touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit hitInfo;

                    if (Physics.Raycast(ray, out hitInfo, 100f, 1 << LayerMask.NameToLayer("touchPad")))
                    {
                        //TouchPad ��ȣ Ȯ��
                        string touchPadName = hitInfo.transform.name;
                        touchPadName = touchPadName.Replace("Touch0", "");
                        int touchIdx = int.Parse(touchPadName) - 1;

                        touchStartedIdx = touchIdx;
                        touchedFX(touchIdx);
                        print(i + "��° touch�� ��" + touchIdx + "���� ��ġ�е尡 ���Ȱ�" + "touchedFX �Լ��� ����Ǿ���.");

                        if (noteInstance_Rails[touchIdx].Count > 0) 
                        {
                            //NoteType Ȯ��
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
                                //���� üũ
                                if (touchIdx < touchStartedIdx)     //���� �巡��
                                {
                                    if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                        noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_LEFT)
                                    {
                                        print("���� idx��" + touchIdx + "�̰�" + " startedIdx��" + touchStartedIdx);
                                        print("�������� �巡�׵ǰ� �ֽ��ϴ�");

                                        //deltaPos�� ������������ üũ�ϱ�! ���� �������� �����̰� �ִ���
                                        PressingScore(touchStartedIdx);
                                        showScoreText(9);
                                    }
                                }
                                else    //������ �巡��
                                {
                                    if (noteInstance_Rails[touchStartedIdx].Count > 0 &&
                                        noteInstance_Rails[touchStartedIdx][0].noteInfo.type == (int)NoteType.DRAG_RIGHT)
                                    {
                                        print("���������� �巡�׵ǰ� �ֽ��ϴ�");
                                        PressingScore(touchStartedIdx);
                                        showScoreText(10);
                                    }
                                }
                            }
                            else //���� ��ư�� �� ������ ��   checkFINISHED !!!
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

                        //�� ���� pad ��ȣ Ȯ��
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

                                        print("�巡�� ��Ʈ �����߾��!");
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

    //���� touch�� �κи� ������ �������� �������� check!!!
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
        print("releasedFX�Լ� ����");
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
            //�������� ���̴ϱ� ��ġ�ص� �ǹ̰� ����.
            return;
            //passDestroy�� ���� �ʱ� ����
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
            //���� �������� ��
            //��ġ�е� ���� �Ŀ� autoDestroy           
        }
    } //check_FINISHED!!!

    public void ExitCheck_LONG(int n)
    {
        print("ExitCheck_Long ���Դϴ�");

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
            //��Ʈ�� �������� ��           
        }
    }   //check FINISHED!!!

    public void ExitCheck_DRAG(int n)
    {
        if (noteInstance_Rails[n][0] == null) return;

        //�̹� �� ���� �ùٸ� ��ġ��� ���� Ȯ���� �Ĵϱ�!!!
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
            //note�� ���� ���� ���� ���� ���� ��� score���� X
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

        //note���� FX������
    }

    public void MissUnabled(int n)
    {
        //long�̳� drag�� �����ٰ� ������ ������ ���� ���
        //passDestroy������ �Ⱓ ���� ���� üũ�� ���� ���ϵ��� �ؾ���.

        //�ϴ��� pressdestroy�ص�
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

    //scoreManager Script�� ����
    void showScoreText(int n)
    {
        GameObject scoreText = Instantiate(scoreTexts[n], canvas.transform.position - Vector3.forward, Quaternion.identity);
        scoreText.transform.SetParent(canvas.transform);

        Destroy(scoreText, 0.5f);
    }

}
