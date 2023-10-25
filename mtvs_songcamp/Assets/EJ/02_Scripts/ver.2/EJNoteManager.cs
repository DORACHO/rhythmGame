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
        //InputTestLONGNotes();     //test FINISHED_1��!!!
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
                        //continue: if ������ ���̶��, ���� �ݺ��� �ߴ��ϰ� ���� �ݺ��� �����մϴ�. 
                        //�ش� ���Ͽ� note�� �����Ǿ� ���� ���� ��ġ�� �Դ� ����.

                        if (hitInfo.transform.gameObject == touchpads[j].gameObject)
                        {
                            if (touch.phase == TouchPhase.Began)
                            {
                                //isPressDown(j);
                                touchpads[j].GetComponent<MeshRenderer>().enabled = true;   //�� �ٱ��� �������� Ȯ��
                                //Debug.Log("touchPad�� �ε�����" + j);

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
                                    //touchPad���� �ͺ��� 2ĭ ������ note.type�� drag_right����
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
                                    //touchPad���� �ͺ��� 2ĭ �������� note.type�� drag_left����
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
            //Debug.LogError("����üũ");
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
            //note�� ���� touchpad�� ��� ��
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
        //�̹� �� ���� �ùٸ� ��ġ��� ���� Ȯ���� �Ĵϱ�
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
            //note�� ���� ���� ���� ���� ���� ��� score���� X
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


    //scoreManager Script�� ����
    void showScoreText(int n)
    {
        GameObject scoreText = Instantiate(scoreTexts[n], canvas.transform.position - Vector3.forward, Quaternion.identity);
        scoreText.transform.SetParent(canvas.transform);

        Destroy(scoreText, 0.5f);
    }

}
