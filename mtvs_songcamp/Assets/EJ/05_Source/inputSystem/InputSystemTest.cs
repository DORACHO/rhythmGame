using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTest: MonoBehaviour
{
    private PlayerInput playerInput;

    private InputActionMap inputActionMap;

    private InputAction touch_began;
    private InputAction touch_moved;
    private InputAction touch_ended;

   /* public*/ Canvas canvas;
   /* public*/ TextMeshProUGUI scoreText;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        touch_began = playerInput.actions.FindAction("Touch_began");
        print("1" + touch_began);
        touch_moved = playerInput.actions.FindAction("Touch_moved");
        print("2" + touch_moved);
        touch_ended = playerInput.actions.FindAction("Touch_ended");
        print("3" + touch_ended);   
    }

    private void OnEnable()
    {
        touch_began.Enable();
        touch_moved.Enable();   
        touch_ended.Enable();

        touch_began.performed += TouchBegan;
        touch_moved.performed += TouchMoved;
        touch_ended.performed += TouchEnded;
    }

    private void Update()
    {
        if (touch_moved.inProgress)
        {
            TouchProgress();
        }
    }

    private void OnDisable()
    {
        touch_began.performed -= TouchBegan;
        touch_moved.performed -= TouchMoved;
        touch_ended.performed -= TouchEnded;
    }

    void TouchProgress()
    {
        print("touching");
        //scoreText.text = "touching";
    }

    private void TouchBegan(InputAction.CallbackContext context)
    {
        print("touchBegan");
        //scoreText.text = "touchBegan";
    }

    private void TouchMoved(InputAction.CallbackContext context)
    {
        print("touchMoved");
        //scoreText.text = "touchMoved";
    }

    private void TouchEnded(InputAction.CallbackContext context)
    {
        print("touchEnded");
        //scoreText.text = "touchEnded";
    }
}
