using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GestureRecognizer;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CaptureRecognize : MonoBehaviour {

    public bool isEnabled = true;
    public bool forceCopy = false;
    public bool useProtractor = false;
    public string libraryToLoad = "shapes";

    public Image imageView;
    public Text timer;
    public Text score;
    public static float maxTime = 30;
    float timeLeft = maxTime;

    public Button restart;

    public static int _score;

    public float distanceBetweenPoints = 10f;
	public int minimumPointsToRecognize = 10;
    public Material lineMaterial;
    public float startThickness = 0.25f;
    public float endThickness = 0.05f;
    public Color startColor = new Color(0, 0.67f, 1f);
    public Color endColor = new Color(0.48f, 0.83f, 1f);

    public GestureLimitType gestureLimitType = GestureLimitType.None;
    public RectTransform gestureLimitRectBounds;
    Rect gestureLimitRect;
    Canvas parentCanvas;
    RuntimePlatform platform;

    LineRenderer gestureRenderer;

    Vector3 virtualKeyPosition = Vector2.zero;

    Vector2 point;

    List<Vector2> points = new List<Vector2>();

    int vertexCount = 0;

    GestureLibrary gl;
    Gesture gesture;
    Result result;

    public delegate void GameEvent(Result r);
    public static event GameEvent OnRecognition;

    // Get the platform and apply attributes to line renderer.
    void Awake() {
        platform = Application.platform;
        gestureRenderer = gameObject.AddComponent<LineRenderer>();
        gestureRenderer.SetVertexCount(0);
        gestureRenderer.material = lineMaterial;
        gestureRenderer.SetColors(startColor, endColor);
        gestureRenderer.SetWidth(startThickness, endThickness);

        Sprite[] sprites = Resources.LoadAll<Sprite>("Figures");
        imageView.sprite = sprites[Random.Range(0, sprites.Length)];
    }

    // Load the library.
    void Start() {
        gl = new GestureLibrary(libraryToLoad, forceCopy);

        if (gestureLimitType == GestureLimitType.RectBoundsClamp) {
            parentCanvas = gestureLimitRectBounds.GetComponentInParent<Canvas>();
            gestureLimitRect = RectTransformUtility.PixelAdjustRect(gestureLimitRectBounds, parentCanvas);
            gestureLimitRect.position += new Vector2(gestureLimitRectBounds.position.x, gestureLimitRectBounds.position.y);
        }
    }

    // Track user input and fire OnRecognition event when necessary.
    void Update() {
        // Track user input if GestureRecognition is enabled.
        if (isEnabled) {
            // If it is a touch device, get the touch position
            // if it is not, get the mouse position
            if (Utility.IsTouchDevice()) {
                if (Input.touchCount > 0) {
                    virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
                }
            } else {
                if (Input.GetMouseButton(0)) {
                    virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
                }
            }
            // It is not necessary to track the touch from this point on,
            // because it is already registered, and GetMouseButton event 
            // also fires on touch devices
            if (Input.GetMouseButton(0)) {

                switch (gestureLimitType) {

                    case GestureLimitType.None:
                        RegisterPoint();
                        break;

                    case GestureLimitType.RectBoundsIgnore:
                        if (RectTransformUtility.RectangleContainsScreenPoint(gestureLimitRectBounds, virtualKeyPosition, null)) {
                            RegisterPoint();
                        }
                        break;

                    case GestureLimitType.RectBoundsClamp:
                        virtualKeyPosition = Utility.ClampPointToRect(virtualKeyPosition, gestureLimitRect);
                        RegisterPoint();
                        break;
                }

            }
            // Capture the gesture, recognize it, fire the recognition event,
            // and clear the gesture from the screen.
            if (Input.GetMouseButtonUp(0)) {

                if (points.Count > minimumPointsToRecognize) {
                    gesture = new Gesture(points);
                    result = gesture.Recognize(gl, useProtractor);

                    if (OnRecognition != null) {
                        OnRecognition(result);
                    }

                    if (result.Name == imageView.sprite.name)
                    {
                        Debug.Log("Next Level");
                        maxTime -= 2f;
                        _score += 1;
                        SceneManager.LoadScene("Main");
                    }

                }

                ClearGesture();
            }

            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                timer.text = "Time left: " + Mathf.Round(timeLeft).ToString();
            }
            else
            {
                imageView.gameObject.SetActive(false);
                timer.gameObject.SetActive(false);
                restart.gameObject.SetActive(true);
                score.text = "Score: " + _score.ToString();
                score.gameObject.SetActive(true);
            }
        }

    }
    /// <summary>
    /// Register this point only if the point list is empty or current point
    /// is far enough than the last point. This ensures that the gesture looks
    /// good on the screen. Moreover, it is good to not overpopulate the screen
    /// with so much points.
    /// </summary>
    void RegisterPoint() {
        point = new Vector2(virtualKeyPosition.x, -virtualKeyPosition.y);

        if (points.Count == 0 || (points.Count > 0 && Vector2.Distance(point, points[points.Count - 1]) > distanceBetweenPoints)) {
            points.Add(point);

            gestureRenderer.SetVertexCount(++vertexCount);
            gestureRenderer.SetPosition(vertexCount - 1, Utility.WorldCoordinateForGesturePoint(virtualKeyPosition));
        }
    }
    /// <summary>
    /// Remove the gesture from the screen.
    /// </summary>
    void ClearGesture() {
        points.Clear();
        gestureRenderer.SetVertexCount(0);
        vertexCount = 0;
    }

}
