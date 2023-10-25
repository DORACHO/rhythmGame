using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJTouchManager3 : MonoBehaviour
{
    Touch touch;

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

    float pressScore = 1;

    //변경해야 하는 변수들
    GameObject note;
    GameObject touchPad;
    float dist;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        #region 01. Note_Instantiate & Destroy
        //생성하는 부분 넣어주기
        #endregion

        #region 02. scoreCheck
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                touch = Input.GetTouch(i);
                dist = MathF.Abs(note.transform.position.y - touchPad.transform.position.y);

                if (touch.phase == TouchPhase.Began)
                {
                    //short Note score check (0~5)
                    ScoreCheck_SHORT();
                    //long Note success check (0,1)
                    SuccessEnter_LONG();
                    //drag Note success check (0,1)
                    SuccessEnter_DRAG();
                    
                }
                if (touch.phase == TouchPhase.Moved)
                {  
                    //long, drag Notes score ++ 
                    PressingScore();
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    //long Note score check(0~5)
                    SuccessExit_LONG();
                    //drag Note success check by deltaPosition, ended index
                    SuccessExit_DRAG();
                }
            }
        }
        #endregion

    }

    private void ScoreCheck_SHORT()
    {
        //01.excellentZone
        if (dist >= 0 && dist < excellentZone)
        {
            //excellent
            //showScoreText(0);
            EJScoreManager.instance.SCORE += excellentScore;
        }
        else if (dist >= excellentZone && dist < greatZone)
        {
            //great
            //showScoreText(1);
            EJScoreManager.instance.SCORE += greatScore;
        }
        else if (dist >= greatZone && dist < goodZone)
        {
            //good
            //showScoreText(2);
            EJScoreManager.instance.SCORE += goodScore;
        }
        else if (dist >= goodZone && dist < badZone)
        {
            //bad
            //showScoreText(3);
            EJScoreManager.instance.SCORE += badScore;
        }
        else
        {
            //miss
            //showScoreText(4);
            EJScoreManager.instance.SCORE += missScore;
        }
    }

    private void SuccessEnter_LONG()
    {
        if (dist < badZone)
        {
            //success
        }else
        {
            //miss
        }
    }

    private void SuccessExit_LONG()
    {
        if (dist > badZone)
        {
            //miss
        }
    }

    private void SuccessEnter_DRAG()
    {
        if (dist < badZone)
        {
            //success
        }else
        {
            //miss
        }
    }

    private void SuccessExit_DRAG()
    {
        if (dist < badZone)
        {
            //success

        }else
        {
            //miss
        }
    }


    private void PressingScore()
    {
        EJScoreManager.instance.SCORE += pressScore;
    }






}
