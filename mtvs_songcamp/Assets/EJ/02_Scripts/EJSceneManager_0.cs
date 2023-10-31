using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EJSceneManager_0 : MonoBehaviour
{
    SceneManager sceneMgr;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.GetSceneByBuildIndex(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(1);
        }
    }
}
