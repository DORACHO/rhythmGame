using Melanchall.DryWetMidi.Multimedia;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;

public class EJLongNoteTouchTest : MonoBehaviour
{
    //startNote: keydown
    //connectNote : key
    //endNote: keyup

    GameObject startNote, connectNote, endNote;

    bool isStartNoteSuccessed;
    bool isConnectNoteSuccessed;
    bool isEndNoteSuccessed;

    //keydown시점에 startNote 위치가 어디였는지
    //keyup 시점에 endNote 위치가 어디였는지
    
    public GameObject touchpadmodel;
    EJTouchPad touchpad;

    //bool isKeyDown = true;
    Stopwatch stopwatch = new Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        handState = HandState.None;
        touchpad = touchpadmodel.GetComponent<EJTouchPad>();
    }

    // Update is called once per frame
    void Update()
    {
        #region longNoteCheck01
        if (touchpad.isTriggered)
        {
            stopwatch.Start();

            //트리거 되고 눌리면 점수체크, 도중에 떼지면 miss 뜨기
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                stopwatch.Stop();
                long keydownTime = stopwatch.ElapsedMilliseconds;
                print(keydownTime);
                print("keydown");
                //isKeyDown = true;
                stopwatch.Start();
            }
            else if (Input.GetKey(KeyCode.Alpha0))
            {
                print("key");
            }
            else if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                stopwatch.Stop();
                long keyupTime = stopwatch.ElapsedMilliseconds;
                print("keyup");
                print(keyupTime);
                //isKeyDown = false;
            }            
        }
        #endregion

        //01. 시도1: key event 기준으로 note가 지나가는 위치 체크
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //KeyDown된 시점에 startNote와 touchpad의 거리를 구한다.
            float startP = Vector3.Distance(startNote.transform.position, touchpad.transform.position);

            //startNote가 판정 범위 내에 들어왔다면 success판정
            if (startP < 0.5f)
            {
                isStartNoteSuccessed = true;
            }
        }else if (Input.GetKey(KeyCode.Alpha2) && isStartNoteSuccessed)
        {
            //startNote가 제때 눌렸다면 connectNote체크를 해라.
            float connectP = Vector3.Distance(connectNote.transform.position, touchpad.transform.position);

            float connectLength = endNote.transform.position.y - startNote.transform.position.y;

            if (Mathf.Abs(connectP) < connectLength)
            {
                isConnectNoteSuccessed = true;
            }
        }else if (Input.GetKeyUp(KeyCode.Alpha2) && isConnectNoteSuccessed)
        {
            float endP = Vector3.Distance(endNote.transform.position, touchpad.transform.position);
            
            if (endP < 0.5)
            {
                isConnectNoteSuccessed = true;
            }
        }

       
    }

    //02. 시도2: note 지나가는 위치 기준으로 key event 체크

    enum HandState
    {
        None,
        Pressed,
        Pressing,
        Released
    }

    HandState handState;
    int score;

    private void OnTriggerStay(Collider other)
    {        
        //handState 체크  (Update로 옮기기)                       
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            handState = HandState.Pressed; 
        }
        else if (Input.GetKey(KeyCode.Alpha0))
        {
            handState = HandState.Pressing;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha0)) 
        {
            handState = HandState.Released;
        }

        //note의 종류에 따라 키이벤트 구분 후 점수 판정
        if (other.CompareTag("startNote"))
        {
            if (handState == HandState.Pressed)
            {
                //거리 체크 후 점수 판정
                isStartNoteSuccessed = true;
            }else
            {
                //miss
            }
        }
        
        if (other.CompareTag("connectNote"))
        {
            //startNote가 눌렸고 Pressing 이라면 점수 체크
            if (handState == HandState.Pressing && isStartNoteSuccessed)
            {
                //점수 증가

                if(true/*connectNote와 touchPad의 거리가 0이 되면*/)
                {
                    isConnectNoteSuccessed = true;
                }

            }
            else
            {
                //miss
            }
        }

        if (other.CompareTag("endNote"))
        {
            if (handState == HandState.Released && isConnectNoteSuccessed)
            {
                //거리 체크 후 점수 판정
            }
        }
    }

}
