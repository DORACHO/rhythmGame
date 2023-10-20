using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Melanchall.DryWetMidi.Interaction;

public class EJTouchpad_LongNote : MonoBehaviour
{
    enum HandState
    {
        None,
        Pressed,
        Pressing,
        Released
    }

    HandState handState = HandState.Released;

    public TextMeshProUGUI scoreText;
    float score;

    Transform startNotePos;
    Transform endNotePos;
    Transform connectNotePos;

    float startNoteD;
    float connectNoteD;
    float endNoteD;

    bool isStartNoteSuccessed;
    bool isConnectNoteSuccessed;
    bool isEndNoteSuccessed;

    public Canvas canvas;
    public GameObject missText;

    public Material missMat;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();
        //handState 체크                   
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            handState = HandState.Pressed;
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
           // handState = HandState.Pressing;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            handState = HandState.Released;
            n = 0;
        }

        if(n > 0)
        {
            score += Time.deltaTime;
            scoreText.text = score.ToString();
        }

    }

    int n = 0;
    private void OnTriggerStay(Collider other)
    {
        //note의 종류에 따라 handState체크 후 점수 판정

        if (other.CompareTag("startNote") && n == 0)
        {
            if (handState == HandState.Pressed)
            {
                print("1");

                startNotePos = other.transform;
                //거리 판정은 나중에
                startNoteD = Vector3.Distance(startNotePos.position, transform.position);

                isStartNoteSuccessed = true;
                score++;
                n++;
            }   
           
            //else if (handState == HandState.Pressing)
            //{
            //    print("2");
            //    score += Time.deltaTime;
            //    scoreText.text = score.ToString();
            //}
        }
     
        if (other.CompareTag("linkNote") && n == 0)
        {
            missCheck();

            //print("2.5");
            //if (isStartNoteSuccessed)
            //{
            //    if (handState == HandState.Pressing)
            //    {
            //        print("3");
            //        score += Time.deltaTime;
            //        scoreText.text = score.ToString();
            //    }
            //    else
            //    {
            //        print("4");
            //        missCheck();
            //    }
            //}
        }
       

        //if (other.CompareTag("endNote"))
        //{
        //    if (isConnectNoteSuccessed)
        //    {
        //        if (handState == HandState.Pressing)
        //        {
        //            print("5");
        //            score += Time.deltaTime;
        //        }
        //        else if (handState == HandState.Released)
        //        {
        //            print("success");
        //        }
        //    }
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("startNote") && n == 0)
        {
            missCheck();
            other.gameObject.GetComponent<MeshRenderer>().material = missMat;
        }

        if (other.CompareTag("linkNote") && n == 0)
        {
            missCheck();
        }
        if (other.CompareTag("endNote"))
        {
            handState = HandState.Released;
            n = 0;
        }

        return;
        //startNote가 눌리지 않고 지나갔다면
        if (!isStartNoteSuccessed)
        {
            if (other.CompareTag("startNote"))
            {
                missCheck();
            }
        }
        else
        {
            //linkNote가 다 지나가도록 눌리고 있었다면 success체크
            if (other.CompareTag("linkNote") && handState == HandState.Pressing)
            {
                isConnectNoteSuccessed = true;
            }
            //linkNote가 눌리지 않고 지나갔다면
            else
            {
                missCheck();
                
                //여기 체크
                other.gameObject.GetComponent<MeshRenderer>().material = missMat;
            }

        }
    }

    void missCheck()
    {
        GameObject miss = Instantiate(missText, canvas.transform.position - Vector3.forward, Quaternion.identity);
        miss.transform.SetParent(canvas.transform);

        

        Destroy(miss, 3);
    }
}
