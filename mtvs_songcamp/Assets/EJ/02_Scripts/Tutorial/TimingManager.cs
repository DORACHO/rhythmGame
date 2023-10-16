using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{
    //생성된 노트를 담을 List
    public List<GameObject> boxNoteList = new List<GameObject>();

    [SerializeField] Transform Center = null;
    [SerializeField] Transform[] timingRect = null;
    Vector3[] timingBoxes = null;

    // Start is called before the first frame update
    void Start()
    {
        timingBoxes = new Vector3[timingRect.Length];
        
        for (int i = 0; i < timingRect.Length; i++)
        {
            //timingBoxes[i].Set(Center.localPosition.x - timingRect[i].transform.position)
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
