using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EJSceneManager_5 : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClickComposeBtn()
    {
        SceneManager.LoadScene(7);
    }

    public void ClickPlayBtn()
    {
        SceneManager.LoadScene(8);
    }

    public void ClickConcertBtn()
    {
        SceneManager.LoadScene(10);
    }

    public void ClickChatbotBtn()
    {
        SceneManager.LoadScene(6);
    }
}
