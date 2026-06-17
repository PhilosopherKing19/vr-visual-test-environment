using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class VisualAcuityTest : MonoBehaviour
{
    private enum StimuliSet
    {
        LandoltC,
        GeoShapes,
        TumblingE,
        SloanLetters
    }

    // --- Screen setup ---------------------------------------------------
    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float scale;

    // --- Task configuration ----------------------------------------------
    [SerializeField] private StimuliSet currentStimuliSet;
    [SerializeField] private float finishingSize;
    [SerializeField] private int totalTrials;
    [UnityEngine.Range(10f, 400f)]
    [SerializeField] private float initialSize = 200f;
    [SerializeField] private bool endOnFinishingSize;
    [SerializeField] private int correctThreshold;
    [SerializeField] private int incorrectThreshold;

    // --- Stimuli sprites ----------------------------------------------
    [SerializeField] private Sprite landoltC;
    [SerializeField] private Sprite tumblingE;
    [SerializeField] private Sprite[] sloanLetterSprites;
    [SerializeField] private Sprite[] geometricSprites;

    private readonly float[] landoltCRotations = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private readonly float[] tumblingERotations = { 0f, 90f, 180f, 270f };
    private Sprite[] currentSprites;

    // --- Staircase state ----------------------------------------------
    private int correctStreak;
    private int incorrectStreak;
    private float currentSize;
    private float sizeStep;
    private int trialCount;

    // --- Scene references ----------------------------------------------
    private GameObject screen;
    private Canvas screenCanvas;
    private UnityEngine.UI.Image stimulusImage;
    private int currentStimulusIndex;

    private GameObject buttonCanvas;
    private List<GameObject> answerButtons;

    // --- Trial timing ----------------------------------------------
    private float trialStartTime;
    private float responseTime;

    // --- CSV logging ----------------------------------------------
    private string csvPath;
    private int trialNumber;

    // --- Calibration logging (px-to-world-unit measurement) -------------
    // TEMP - used to derive the px-to-metre conversion factor reported in
    // the thesis; kept for reproducibility, can be removed in future use.
    private bool calibrationLogged;

    // Builds the response buttons for Sloan Letters and Geometric Shapes,
    // since these sets cannot be answered through a single directional
    // keypress like Landolt C or Tumbling E. One button is created per
    // sprite in the current set and arranged in a centered horizontal row.
    private void SetupAnswerButtons()
    {
        GameObject canvasObj = new GameObject("ButtonCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        buttonCanvas = canvasObj;

        float totalWidth = (currentSprites.Length - 1) * 110f;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < currentSprites.Length; i++)
        {
            GameObject btnObj = new GameObject("AnswerButton_" + i);
            btnObj.AddComponent<RectTransform>();
            btnObj.transform.SetParent(buttonCanvas.transform, false);

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 100f);
            rect.anchoredPosition = new Vector2(startX + i * 110f, 0f);

            UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
            img.sprite = currentSprites[i];
            img.color = Color.black;

            Button button = btnObj.AddComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => EvaluateResponse(index));
        }
    }

    // Fills currentSprites with the sprite set selected in the Inspector.
    // Landolt C and Tumbling E only need a single sprite repeated several
    // times, since their variation comes from rotation rather than
    // different sprites.
    private void SetupStimuliSet()
    {
        switch (currentStimuliSet)
        {
            case StimuliSet.LandoltC:
                currentSprites = Enumerable.Repeat(landoltC, 8).ToArray();
                break;

            case StimuliSet.GeoShapes:
                currentSprites = geometricSprites;
                break;

            case StimuliSet.TumblingE:
                currentSprites = Enumerable.Repeat(tumblingE, 4).ToArray();
                break;

            case StimuliSet.SloanLetters:
                currentSprites = sloanLetterSprites;
                break;
        }
    }

    // Instantiates the screen prefab and creates the stimulus image as a
    // child of its canvas. The stimulus sprite and size are set later in
    // DisplayStimulus for each trial.
    private void SetupScreen()
    {
        screen = Object.Instantiate(screenPrefab);
        screenCanvas = screen.GetComponentInChildren<Canvas>();

        GameObject obj = new GameObject("Stimulus");
        UnityEngine.UI.Image img = obj.AddComponent<UnityEngine.UI.Image>();
        obj.transform.SetParent(screenCanvas.transform, false);

        stimulusImage = img;
    }

    // Rotates the stimulus image to the orientation corresponding to
    // shapeIndex. Only Landolt C and Tumbling E use rotation to encode the
    // correct answer; the other sets rely on distinct sprites instead.
    private void ApplyRotation(UnityEngine.UI.Image img, int shapeIndex)
    {
        if (currentStimuliSet == StimuliSet.LandoltC)
            img.transform.rotation = Quaternion.Euler(0, 0, landoltCRotations[shapeIndex]);

        else if (currentStimuliSet == StimuliSet.TumblingE)
            img.transform.rotation = Quaternion.Euler(0, 0, tumblingERotations[shapeIndex]);
    }

    // Picks a random stimulus, applies it at the current staircase size and
    // rotation, and marks the start of the trial's response window.
    private void DisplayStimulus()
    {
        trialStartTime = Time.time;
        int index = Random.Range(0, currentSprites.Length);
        currentStimulusIndex = index;
        stimulusImage.sprite = currentSprites[index];
        stimulusImage.color = Color.black;
        stimulusImage.rectTransform.sizeDelta = new Vector2(currentSize, currentSize);
        ApplyRotation(stimulusImage, index);
    }

    // Core staircase logic, called whenever a response is registered.
    // Three consecutive correct responses shrink the stimulus by one step,
    // while a single incorrect response grows it back by one step. After
    // logging the trial, the task either ends or moves on to the next
    // stimulus, depending on the configured termination condition.
    private void EvaluateResponse(int answer)
    {
        bool correct = answer == currentStimulusIndex;
        if (correct)
        {
            Debug.Log("Correct!");
            SaveToCSV(answer, correct);
            trialCount++;
            correctStreak++;
            incorrectStreak = 0;
            if (correctStreak >= correctThreshold)
            {
                currentSize -= sizeStep;
                correctStreak = 0;
            }
        }
        else
        {
            Debug.Log("Wrong!");
            SaveToCSV(answer, correct);
            trialCount++;
            incorrectStreak++;
            correctStreak = 0;
            if (incorrectStreak >= incorrectThreshold)
            {
                currentSize += sizeStep;
                currentSize = Mathf.Min(currentSize, initialSize);
                incorrectStreak = 0;
            }
        }

        responseTime = Time.time - trialStartTime;

        if (endOnFinishingSize && currentSize <= finishingSize)
        {
            Debug.Log("Task finished - Min. size reached");
            EndTask();
        }
        else if (!endOnFinishingSize && trialCount >= totalTrials)
        {
            Debug.Log("Task finished - Max. trials reached");
            EndTask();
        }
        else
        {
            DisplayStimulus();
        }
    }

    // Stops play mode in the editor or quits the built application,
    // depending on which environment the task is running in.
    private void EndTask()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Maps numpad and arrow key input to the four Tumbling E directions.
    private void HandleInputTumblingE()
    {
        if (Keyboard.current.numpad4Key.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame) EvaluateResponse(0);

        else if (Keyboard.current.numpad8Key.wasPressedThisFrame ||
                 Keyboard.current.upArrowKey.wasPressedThisFrame) EvaluateResponse(1);

        else if (Keyboard.current.numpad6Key.wasPressedThisFrame ||
                 Keyboard.current.rightArrowKey.wasPressedThisFrame) EvaluateResponse(2);

        else if (Keyboard.current.numpad2Key.wasPressedThisFrame ||
                 Keyboard.current.downArrowKey.wasPressedThisFrame) EvaluateResponse(3);
    }

    // Maps numpad input to the eight Landolt C gap orientations. The
    // numpad layout mirrors the eight compass directions, so each key sits
    // in roughly the same direction as the gap it represents.
    private void HandleInputLandoltC()
    {
        if (Keyboard.current.numpad8Key.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame) EvaluateResponse(0);

        else if (Keyboard.current.numpad9Key.wasPressedThisFrame) EvaluateResponse(1);

        else if (Keyboard.current.numpad6Key.wasPressedThisFrame ||
                 Keyboard.current.rightArrowKey.wasPressedThisFrame) EvaluateResponse(2);

        else if (Keyboard.current.numpad3Key.wasPressedThisFrame) EvaluateResponse(3);

        else if (Keyboard.current.numpad2Key.wasPressedThisFrame ||
                 Keyboard.current.downArrowKey.wasPressedThisFrame) EvaluateResponse(4);

        else if (Keyboard.current.numpad1Key.wasPressedThisFrame) EvaluateResponse(5);

        else if (Keyboard.current.numpad4Key.wasPressedThisFrame ||
                 Keyboard.current.leftArrowKey.wasPressedThisFrame) EvaluateResponse(6);

        else if (Keyboard.current.numpad7Key.wasPressedThisFrame) EvaluateResponse(7);
    }

    // Appends one line of trial data to the CSV file, matching the header
    // written in SetupCSV.
    private void SaveToCSV(int playerAnswer, bool correct)
    {
        string line = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0},{1},{2},{3},{4},{5},{6}\n",
            trialNumber, currentStimuliSet, playerAnswer, correct, currentSize, correctStreak, responseTime);
        File.AppendAllText(csvPath, line);
    }

    // Creates a new, timestamped CSV file for this session and writes the
    // header row.
    private void SetupCSV()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        csvPath = Application.persistentDataPath + "/visual_acuity_" + timestamp + ".csv";
        File.WriteAllText(csvPath, "TrialNumber,StimuliSet,PlayerAnswer,Correct,CurrentSize,CorrectStreak,ResponseTime\n");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupCSV();
        SetupStimuliSet();
        SetupScreen();
        if (currentStimuliSet == StimuliSet.SloanLetters || currentStimuliSet == StimuliSet.GeoShapes)
            SetupAnswerButtons();
        sizeStep = (initialSize - finishingSize) / totalTrials;
        currentSize = initialSize;
        DisplayStimulus();
    }

    // Update is called once per frame
    void Update()
    {
        screen.transform.position = position;
        screen.transform.rotation = Quaternion.Euler(rotation);
        screen.transform.localScale = new Vector3(scale, scale, 1f);

        // TEMP - for unit conversion (px to world units), logged once per
        // session; kept for reproducibility of the thesis's calibration
        // measurement.
        if (!calibrationLogged)
        {
            Vector3[] corners = new Vector3[4];
            stimulusImage.rectTransform.GetWorldCorners(corners);
            float worldSize = Vector3.Distance(corners[0], corners[1]);
            float distance = Vector3.Distance(Camera.main.transform.position,
                                               stimulusImage.rectTransform.position);
            Debug.Log($"[ACUITY-CAL] CurrentSize={currentSize:F2}px | worldSize={worldSize:F4}m | distance={distance:F4}m");
            calibrationLogged = true;
        }

        if (currentStimuliSet == StimuliSet.TumblingE) HandleInputTumblingE();
        else if (currentStimuliSet == StimuliSet.LandoltC) HandleInputLandoltC();
    }
}
