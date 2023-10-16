using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterFrame : MonoBehaviour
{
    AudioSource myAudio;
    bool musicStart = false;

    // Start is called before the first frame update
    void Start()
    {
        myAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!musicStart)
        {
            if (other.CompareTag("Note"))
            {
                myAudio.Play();
                musicStart = true;  
            }
        }
    }
}
