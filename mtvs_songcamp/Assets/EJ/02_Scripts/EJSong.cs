using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJSong : MonoBehaviour
{

    public GameObject img1, img2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SongClick()
    {
        img1.gameObject.SetActive(false);
        img2.gameObject.SetActive(true);   
    }
}
