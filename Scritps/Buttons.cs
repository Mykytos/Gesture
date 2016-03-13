using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour {

    public void StartGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Main");
        CaptureRecognize.maxTime = 30;
        CaptureRecognize._score = 0;
    }
}
