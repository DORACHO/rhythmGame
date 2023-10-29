using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongPlay : MonoBehaviour
{
    public AudioSource flop;
    bool isPlaying;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Note"))
        {
            if (!isPlaying)
            {
                isPlaying = true;
                flop.Play();
            }
        }
    }
}
