//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

////midi library
////using Melanchall.DryWetMidi.Core;
////using Melanchall.DryWetMidi.Interaction;
////using System.IO;
////using UnityEngine.Networking;

//public class SongManager : MonoBehaviour
//{
//    public static SongManager instance;
//    public AudioSource audioSource;
//    public float songDelayInSeconds;
//    public int inputDelayInMilliseconds;

//    public string fileLocation;
//    public float noteTime;
//    public float noteSpawnY;
//    public float noteTapY;

//    public List<double> timeStamps;

//    public float noteDespawnY
//    {
//        get
//        {
//            return noteTapY - (noteSpawnY - noteTapY);
//        }
//    }

//    //public static MidiFile midiFie;   // midi library 생성 시 사용할 수 있는 자료형

//    private void Awake()
//    {
//        instance = this;    
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
//        if (Application.streamingAssetsPath.StartsWith("http://") || Application.streamingAssetsPath + "/" + fileLocation)
//        {
//            StartCoroutine(ReadFromWebsite());
//        }else
//        {
//            StartCoroutine(ReadFromFile());
//        }
//    }

//    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
//    {
//        foreach (var note in array)
//        {
//            if (note.NoteName == noteRestriction)
//            {
//                note.Time
//            }
//        }
//    }

//    private IEnumerator ReadFromFile()
//    {
        
//    }

//    private IEnumerator ReadFromWebsite()
//    {
//        using (UnityWebRequest www = UnityWebRequest.Get(Application))
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
