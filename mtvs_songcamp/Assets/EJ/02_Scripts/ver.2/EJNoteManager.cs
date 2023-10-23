using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//01. Note_Instantiate & Destroy
//02. Note_pressCheck & scoreCheck

public class EJNoteManager : MonoBehaviour
{
    //01. Note_Instantiate
    public GameObject[] notePrefabs;
    public Transform[] noteSpawnRail;
    public Transform[] touchpads;

    GameObject startNote;
    GameObject endNote;
    bool isBothMade;

    const int railCount = 6;
    float currTime;

    //01-1.noteData _ 일종의 대기열 느낌
    List<NoteInfo> allNoteInfo = new List<NoteInfo>();
    List<NoteInfo>[] noteInfo_Rails = new List<NoteInfo>[railCount];

    //01-2.Hierarchy - instance noteData
    List<EJNote>[] noteInstance_Rails = new List<EJNote>[railCount];
    EJNote [] startNoteArr = new EJNote[railCount];

    //02. Note_pressCheck
    bool[] isTouchPadPressed = new bool[railCount];

    //03. scoreCheck;
    float badZone = 2.9f;
    float goodZone = 2f;
    float greatZone = 1f;
    float excellentZone = 0.3f;

    int badScore = 1;
    int goodScore = 2;
    int greatScore = 3;
    int excellentScore = 5;
    int missScore = -1;

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

        InputTestSHORTNotes();
        //InputTestLONGNotes();
        //InputTestDRAGNote();
    }

    void Update()
    {
        currTime += Time.deltaTime;

        //01. Note_Instantiate & Destroy
        #region 01. Note_Instantiate & Destroy

        //01-1. Note_Instantiate
        //note instantiate per rails

        for (int i = 0; i < noteInfo_Rails.Length; i++)
        {
            if (noteInfo_Rails[i].Count > 0)
            {
                //Note_Instantiate on Time
                if (currTime >= noteInfo_Rails[i][0].time)
                {
                    //Note_Instantiate by NoteType, SpawnRail
                    //01-1-1.NoteType_SHORT
                    GameObject note = Instantiate(notePrefabs[noteInfo_Rails[i][0].type], noteSpawnRail[i].position + Vector3.forward * (-0.5f), Quaternion.identity);

                    note.transform.forward = notePrefabs[0].transform.forward;
                    note.transform.SetParent(noteSpawnRail[0].transform);
                    
                    EJNote firstNoteInstance = note.GetComponent<EJNote>();
                    firstNoteInstance.noteInfo = noteInfo_Rails[i][0];
                    noteInstance_Rails[i].Add(firstNoteInstance);

                    EJNote noteInstance = note.GetComponent<EJNote>();

                    //01-1-2.NoteType_LONG
                    if (noteInstance.noteInfo.type == (int)NoteType.LONG)
                    {
                        print("noteType은 LONG입니다");

                        if (noteInstance.noteInfo.isLongNoteStart)
                        {
                            print("LongNote의 StartNote입니다");
                            startNoteArr[i] = noteInstance;
                            startNote = noteInstance.gameObject;
                            
                        }else
                        {
                            if (startNoteArr[i] != null)
                            {
                                print("LongNote의 endNote입니다");
                                endNote = noteInstance.gameObject;
                                print("endNote에 담긴 것은" + endNote.gameObject);
                                startNote.GetComponent<EJNote>().connectNote(endNote);
                                
                                //startNote[] remove
                                startNoteArr[i] = null;
                            }
                        }
                    }

                    //01-2. Note_AutoDestroy
                    firstNoteInstance.autoDestroyAction = (railIdx, noteInfo, isPassed) =>
                    {
                        //Pass without Press
                        if (isPassed) isTouchPadPressed[railIdx] = false;
                        //Pass >> remove from List
                        noteInstance_Rails[railIdx].Remove(noteInfo);
                    };
                    
                    //Instantiated되면 대기열에서 지워주기
                    noteInfo_Rails[i].RemoveAt(0);
                }
            }
        }
        #endregion

        //02-1.Note_pressCheck
        #region TouchPad - PressCheck (0,1,2,3,4,5 ~ a,s,d,j,k,l)
        //0,1,2,3,4,5 ~ a,s,d,j,k,l

        //keyDown
        if (Input.GetKeyDown(KeyCode.A))
        {
            isPressDown(0);
            touchpads[0].GetComponent<MeshRenderer>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            isPressDown(1);
            touchpads[1].GetComponent<MeshRenderer>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            isPressDown(2);
            touchpads[2].GetComponent<MeshRenderer>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            isPressDown(3);
            touchpads[3].GetComponent<MeshRenderer>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            isPressDown(4);
            touchpads[4].GetComponent<MeshRenderer>().enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            isPressDown(5);
            touchpads[5].GetComponent<MeshRenderer>().enabled = true;
        }
        //keyUp
        if (Input.GetKeyUp(KeyCode.A))
        {
            isPressUp(0);
            isTouchPadPressed[0] = false;
            touchpads[0].GetComponent<MeshRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            isPressUp(1);
            isTouchPadPressed[1] = false;
            touchpads[1].GetComponent<MeshRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            isPressUp(2);
            isTouchPadPressed[2] = false;
            touchpads[2].GetComponent<MeshRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            isPressUp(3);
            isTouchPadPressed[3] = false;
            touchpads[3].GetComponent<MeshRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            isPressUp(4);
            isTouchPadPressed[4] = false;
            touchpads[4].GetComponent<MeshRenderer>().enabled = false;
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            isPressUp(5);
            isTouchPadPressed[5] = false;
            touchpads[5].GetComponent<MeshRenderer>().enabled = false;
        }

        #endregion

        //02-2.Note_pressCheck + scoreCheck
        #region 02. Note_pressCheck

        // 02-2-1. KeyEvent check : keyDown = true
        for (int i = 0; i < isTouchPadPressed.Length; i++)
        {
            if (isTouchPadPressed[i] == true)
            {
                //무슨 경우지?
            }
        }

        // 02-2-2. KeyEvent check : before anyNote Instantiated
        for (int i = 0; i < noteInstance_Rails.Length; i++)
        {
            //Before any Note Instantiated 
            if (noteInstance_Rails[i].Count == 0) continue;

            //Note Not Pressed && TouchPad Not Pressed 
            if (noteInstance_Rails[i][0].noteInfo.type == (int)NoteType.LONG && noteInstance_Rails[i][0].noteInfo.isLongNoteStart == false)
            {
                if (isTouchPadPressed[i] == false)
                {
                    //score: Miss
                }
            }
        }

        #endregion
    }

    // 02-3. KeyEvent check : keyDown = true
    void isPressDown(int n)
    {
        //Song Start && After Note Instantiated
        if (noteInstance_Rails[n].Count > 0)
        {
            NoteInfo firstNoteInfo = noteInstance_Rails[n][0].noteInfo;
            float dist = Mathf.Abs(noteInstance_Rails[n][0].transform.position.y - touchpads[n].position.y);

            //scoreCheck by NoteCheck

            //01.SHORT Note scoreCheck

            if (firstNoteInfo.type == (int)NoteType.SHORT)
            {
                //01.excellentZone
                if (dist >= 0 && dist < excellentZone)
                {
                    //excellent
                    showScoreText(0);
                    EJScoreManager.instance.SCORE += excellentScore;
                }
                else if (dist >= excellentZone && dist < greatZone)
                {
                    //great
                    showScoreText(1);
                    EJScoreManager.instance.SCORE += greatScore;
                }
                else if (dist >= greatZone && dist < goodZone)
                {
                    //good
                    showScoreText(2);
                    EJScoreManager.instance.SCORE += goodScore;
                }
                else if (dist >= goodZone && dist < badZone)
                {
                    //bad
                    showScoreText(3);
                    EJScoreManager.instance.SCORE += badScore;
                }
                else
                {
                    //miss
                    showScoreText(4);
                    EJScoreManager.instance.SCORE += missScore;
                }
            }


            //02.longNote scoreCheck
            //KeyDown >> isTouchPadPress = true
            //press_start???
            if (firstNoteInfo.type == (int)NoteType.LONG && firstNoteInfo.isLongNoteStart == true)
            {
                //누르기 시작
                isTouchPadPressed[n] = true;
            }
            else
            {
                //성공???
                //왜지?
            }

            //isPassed: No Any KeyEvent 
            noteInstance_Rails[n][0].autoDestroy();
        }
    }

    // 02-3. KeyEvent check : keyUp = true
    void isPressUp(int n)
    {
        //Song Start && After Note Instantiated
        if (noteInstance_Rails[n].Count > 0)
        {
            //
            if (!(noteInstance_Rails[n][0].noteInfo.type == (int)NoteType.LONG && noteInstance_Rails[n][0].noteInfo.isLongNoteStart == false)) return;

            //02.LongNote scoreCheck
            float dist = Mathf.Abs(noteInstance_Rails[n][0].transform.position.y - touchpads[n].position.y);

            if (dist < 0.4f)
            {
                if (isTouchPadPressed[n])
                {
                    //PressDestroy
                    noteInstance_Rails[n][0].autoDestroy();
                }
            }
        }
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
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 1;
        info.type = (int)NoteType.SHORT;
        info.time = 2;
        info.isLongNoteStart = false;
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 2;
        info.type = (int)NoteType.SHORT;
        info.time = 3;
        info.isLongNoteStart = false;
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 3;
        info.type = (int)NoteType.SHORT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 4;
        info.type = (int)NoteType.SHORT;
        info.time = 5;
        info.isLongNoteStart = false;
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 5;
        info.type = (int)NoteType.SHORT;
        info.time = 6;
        info.isLongNoteStart = false;
        info.press_idx = 0;
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
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 1;
        info.type = (int)NoteType.LONG;
        info.time = 2;
        info.isLongNoteStart = false;
        info.press_idx = 0;
        allNoteInfo.Add(info);


        info.railIdx = 3;
        info.type = (int)NoteType.LONG;
        info.time = 3;
        info.isLongNoteStart = true;
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info.railIdx = 3;
        info.type = (int)NoteType.LONG;
        info.time = 5;
        info.isLongNoteStart = false;
        info.press_idx = 0;
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
        info.press_idx = 0;
        allNoteInfo.Add(info);

        info = new NoteInfo();
        info.railIdx = 2;
        info.type = (int)NoteType.DRAG_LEFT;
        info.time = 4;
        info.isLongNoteStart = false;
        info.press_idx = 0;
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


    void showScoreText(int n)
    {
        GameObject scoreText = Instantiate(scoreTexts[n], canvas.transform.position - Vector3.forward, Quaternion.identity);
        scoreText.transform.SetParent(canvas.transform);

        Destroy(scoreText, 0.5f);
    }

}
