using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJBtn : MonoBehaviour
{
    public GameObject input1, input2, input3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickBtn_Musician()
    {
        input1.gameObject.SetActive(false);
        input2.gameObject.SetActive(true);
        input3.gameObject.SetActive(false);
    }

    public void ClickBtn_Listener()
    {
        input1.gameObject.SetActive(false);
        input2.gameObject.SetActive(false);
        input3.gameObject.SetActive(true);
    }

    public void ClickBtn_Back()
    {
        input1.gameObject.SetActive(true);
        input2.gameObject.SetActive(false);
        input3.gameObject.SetActive(false);
    }
}
