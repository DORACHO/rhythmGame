using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notemanager : MonoBehaviour
{
    public int bpm = 0;
    double currentTime = 0d;    //float보다 오차가 작다.

    [SerializeField] Transform ftNoteAppear = null;     //note가 생성될 위치
    [SerializeField] GameObject goNote = null;          //note Prefab

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= 60d / bpm) //1분
        {
            GameObject t_note = Instantiate(goNote, ftNoteAppear.position, Quaternion.identity);
            t_note.transform.SetParent(this.transform);
            currentTime -= 60d / bpm;   
            //currentTime = 0 초기화하지 않고 빼주는 이유
            //currentTime = 0.51005551~만큼의 오차가 손실되기 때문이다.
            //박자가 다르기 때문에
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Note"))
        {
            Destroy(other.gameObject);
        }
    }
}
