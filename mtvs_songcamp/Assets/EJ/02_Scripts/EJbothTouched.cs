using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EJbothTouched : MonoBehaviour
{
    public GameObject[] touchpads;
    Touch touch;
    TouchPhase touchPhase;

    int fingerID;
    int[] fingerIDs;

    Vector2 deltaPos;

    public Canvas canvas;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI touchText;

    // Start is called before the first frame update
    void Start()
    {

    }

    
    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hitInfo;

        if (Input.touchCount > 0)
        {
            touchText.text = "touchCount´Â + ${touchCount}";
            for (int i = 0; i < Input.touchCount; i++)
            {
                touch = Input.GetTouch(i);

                if (Physics.Raycast(ray, out hitInfo))
                {
                    for (int j = 0; j < touchpads.Length; j++)
                    {
                        if (hitInfo.transform.gameObject == touchpads[j].gameObject)
                        {
                            scoreText.text += "0";
                        }
                    }

                }
            }
        }
    }
}
