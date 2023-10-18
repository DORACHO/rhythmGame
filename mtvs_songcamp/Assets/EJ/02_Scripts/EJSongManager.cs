using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Standards;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class EJSongManager : MonoBehaviour
{
    //stopwatch : 타이머
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    private const string OutputDeviceName = "Microsoft GS Wavetable Synth";
    private OutputDevice _outputDevice;
    public Playback _playback;

    public string sss;

    // Start is called before the first frame update
    void Start()
    {
        InitializeOutputDevice();
        var midiFile = MidiFile.Read("C:/Users/user/Downloads/" + sss + ".mid"); //CreateTestFile();
        InitializeFilePlayback(midiFile);
        StartPlayback();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Releasing playback and device...");

        if (_playback != null)
        {
            _playback.NotesPlaybackStarted -= OnNotesPlaybackStarted;
            _playback.NotesPlaybackFinished -= OnNotesPlaybackFinished;
            _playback.Dispose();
        }

        if (_outputDevice != null)
            _outputDevice.Dispose();

        Debug.Log("Playback and device released.");
    }

    private void InitializeOutputDevice()
    {
        Debug.Log($"Initializing output device [{OutputDeviceName}]...");

        var allOutputDevices = OutputDevice.GetAll();
        if (!allOutputDevices.Any(d => d.Name == OutputDeviceName))
        {
            var allDevicesList = string.Join(Environment.NewLine, allOutputDevices.Select(d => $"  {d.Name}"));
            Debug.Log($"There is no [{OutputDeviceName}] device presented in the system. Here the list of all device:{Environment.NewLine}{allDevicesList}");
            return;
        }

        _outputDevice = OutputDevice.GetByName(OutputDeviceName);

        Debug.Log($"Output device [{OutputDeviceName}] initialized.");

    }

    private MidiFile CreateTestFile()
    {
        Debug.Log("Creating test MIDI file...");

        var patternBuilder = new PatternBuilder()
            .SetNoteLength(MusicalTimeSpan.Eighth)
            .SetVelocity(SevenBitNumber.MaxValue)
            .ProgramChange(GeneralMidiProgram.Harpsichord);

        foreach (var noteNumber in SevenBitNumber.Values)
        {
            patternBuilder.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get(noteNumber));
        }

        var midiFile = patternBuilder.Build().ToFile(TempoMap.Default);

        Debug.Log("Test MIDI file created.");

        return midiFile;
    }

    private void InitializeFilePlayback(MidiFile midiFile)
    {
        Debug.Log("Initializing playback...");

        _playback = midiFile.GetPlayback(_outputDevice);
        _playback.Loop = false;
        _playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
        _playback.NotesPlaybackFinished += OnNotesPlaybackFinished;
        _playback.Speed = 1;

        Debug.Log("Playback initialized.");
    }


    private void StartPlayback()
    {
        Debug.Log("Starting playback...");
        stopwatch.Reset();
        stopwatch.Start();
        _playback.Start();
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {
        //LogNotes("Notes finished:", e);
        // print("finish : " + stopwatch.ElapsedMilliseconds + ", " + stopwatch.ElapsedTicks + ", " + e.Notes.ToArray()[0].Time);
    }

    //note의 time에 생성
    //length에 따라 note의 종류 구분 (short/ long)
    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        //LogNotes("Notes started:", e);
        print("start : " + stopwatch.ElapsedMilliseconds + ", " + e.Notes.ToArray()[0].Time + ", " + e.Notes.ToArray()[0].Length);
    }

    private void LogNotes(string title, NotesEventArgs e)
    {
        var message = new StringBuilder()
            .AppendLine(title)
            .AppendLine(string.Join(Environment.NewLine, e.Notes.Select(n => $"  {n}")))
            .ToString();
        //Debug.Log(message.Trim());
    }

}
