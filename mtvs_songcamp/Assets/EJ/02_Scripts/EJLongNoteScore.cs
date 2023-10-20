using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJLongNoteScore : MonoBehaviour
{
    Transform touchPad;
    
    GameObject startNote;
    GameObject endNote;

    Transform startNoteTriggered;
    Transform endNoteTriggered;

    bool startNoteSuccess;
    bool linkNoteSuccess;
    bool endNoteSuccess;

    //따로 체크해서 세개가 다 true라면 점수를 체크한다.
    // start와 end의 점수를 평균내서 할당하는 시스템이 필요하다.
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool isKeyDown;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isKeyDown = true;
            startNoteTriggered = startNote.transform;

        }
        else if (Input.GetKey(KeyCode.Space))
        {
            
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isKeyDown= false;
            endNoteTriggered = endNote.transform;
        }

        
    }

    //start
    private void OnTriggerStay(Collider other)
    {
        if (isKeyDown)
        {
            print("success");

            //start와 triggerpad 위치 감지해서 점수 변경
            //end와 triggerpad 위치 감지해서 점수 변경
            // 점수 체크 

        }

        if (!isKeyDown)
        {
            print("fail");
        }
    }

    //
}
