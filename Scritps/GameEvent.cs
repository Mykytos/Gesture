using UnityEngine;
using System.Collections;
using GestureRecognizer;

public class GameEvent : MonoBehaviour {

    void OnRecognition(Result r)
    {
        Debug.Log("Gesture is " + r.Name + " and scored: " + r.Score);
    }

    void OnEnable()
    {
        CaptureRecognize.OnRecognition += OnRecognition;
    }

    void OnDisable()
    {
        CaptureRecognize.OnRecognition -= OnRecognition;
    }

    void OnDestroy()
    {
        CaptureRecognize.OnRecognition -= OnRecognition;
    }
}
