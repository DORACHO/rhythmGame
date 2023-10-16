using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJnoteMaker : MonoBehaviour
{
    public Transform[] noteFactories;
    public GameObject[] notes;

    public int bpm = 72;
    double currentTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        // bpm(beats per minute): 분당 몇 비트의 템포로 연주되는가.
        // 60 / bpm : 한 비트 당 걸리는 시간?


        if (currentTime >= 3)
        {
            GameObject note = Instantiate(notes[0], noteFactories[0].position + Vector3.forward * -0.5f, Quaternion.identity);

            note.transform.forward = notes[0].transform.forward;    
            note.transform.SetParent(noteFactories[0].transform);

            currentTime -= 3;
        }
    }
}
