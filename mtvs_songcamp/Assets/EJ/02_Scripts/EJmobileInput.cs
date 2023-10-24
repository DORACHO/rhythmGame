using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EJmobileInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    Touch touch;
    Vector3 dragStartPos;
    Vector3 draggingPos;
    Vector3 dragReleasedPos;
    LineRenderer lr;

    // Update is called once per frame
    void Update()
    {
        //OnSigleTouch();
        OnMultiTouch();
        //OnVibrate();

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                {
                    DragStart();
                    OnVibrate();
                }
                if (touch.phase == TouchPhase.Moved)
                {
                    Dragging();
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    DragRelease();
                    OnVibrate();
                }
            }
        }
    }

    void DragStart() 
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            dragStartPos = hitInfo.point;   
        }

        //dragStartPos = Camera.main.ScreenToWorldPoint(touch.position);
        //dragStartPos.z = 0;     //depth가 없는 2차원이니까?
        transform.position = dragStartPos;
        lr.SetPosition(0,dragStartPos);
    }

    void Dragging() 
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            draggingPos = hitInfo.point;
        }

        //Vector3 draggingPos = Camera.main.ScreenToWorldPoint(touch.position);
        //draggingPos.z = 0;
        transform.position = draggingPos;
        lr.SetPosition(1, draggingPos);
    }

    void DragRelease() 
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            dragReleasedPos = hitInfo.point;
        }


        //dragReleasedPos = Camera.main.ScreenToWorldPoint(touch.position);
        //dragReleasedPos.z = 0;
        transform.position = dragReleasedPos;   
        lr.SetPosition(2, dragReleasedPos);
    }

    private void OnVibrate()
    {
        Handheld.Vibrate();
    }

    private void OnSigleTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                
            }else if (touch.phase == TouchPhase.Ended)
            {

            }
        }
    }

    private void OnMultiTouch()
    {
        // touchCount > 0 : isTouched == true
        if (Input.touchCount > 0)
        {
            //touch Properties
            for(int i = 0; i< Input.touchCount; i++) 
            {
                Touch touch = Input.GetTouch(i);
                int index = touch.fingerId;
                Vector2 position = touch.position;
                TouchPhase phase = touch.phase;

                // touch Started (just 1 time)
                if (phase == TouchPhase.Began)
                {

                }
                // touching & moving
                else if (phase == TouchPhase.Moved)
                {
                    //dragNote check?? deltaPosition 체크하는 법
                }
                // touching & moving == false
                else if (phase == TouchPhase.Stationary)
                {
                    //longNote check
                }
                // released by User
                else if (phase == TouchPhase.Ended)
                {

                }
                // released by System
                else if (phase == TouchPhase.Canceled)
                {

                }

            }
        }
    }


}
