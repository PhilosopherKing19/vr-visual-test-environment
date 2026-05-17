using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;

public class VisualAcuityTest : MonoBehaviour
{

    [SerializeField] private GameObject screenPrefab;
    private enum StimuliSet
    {
        LandoltC,
        GeoShapes,
        TumblingE,
        SloaneLetters
    }

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float Scale;

    [SerializeField] private StimuliSet currenStimuliSet;
    [SerializeField] private float finishingSize;
    [SerializeField] private int totalTrials;
    [UnityEngine.Range(10f, 400f)]
    [SerializeField] private float initialSize = 200f;
    [SerializeField] private bool endOnFinishingSize;

    private int correctStreak;
    private int incorrectStreak;
    private float currentSize;
    private float sizeStep;
    private int trialCount;


    [SerializeField] private int correctThreshold;
    [SerializeField] private int incorrectThreshold;

    [SerializeField] private Sprite landoltC;
    private float[] landoltCRotations = { 0f, 45f, 90f, 135f, 180f, 225f, 270, 315f };
    
    [SerializeField] private Sprite tumblingE;
    private float[] tumblingERotations = { 0f, 90f, 180f, 270f };
    
    [SerializeField] private Sprite[] sloanLetterSprites;

    [SerializeField] private Sprite[] geometricSprites;

    private Sprite[] currentSprites;


    
    //--------------------------------------------------------------------

    private GameObject screen;

    private Canvas screenCanvas;

    private UnityEngine.UI.Image stimulusImage;

    private int currentStimulusIndex;
    //---------------------------------------------------------------------
    private string csvPath;
    private int trialNumber;
    //---------------------------------------------------------------------
    
    private GameObject buttonCanvas;

    private List<GameObject> answerButtons;

    private float trialStartTime;
    private float responseTime;
    private void SetupAnswerButtons()
    {
        UnityEngine.UI.Image img;
        Button button;
        GameObject btnObj;
        GameObject canvasObj = new GameObject("ButtonCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        buttonCanvas = canvasObj;

        for (int i = 0; i < currentSprites.Length; i++)
        {
            btnObj = new GameObject("AnswerButton_" + i);
            btnObj.AddComponent<RectTransform>();
            btnObj.transform.SetParent(buttonCanvas.transform, false);
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 100f);
            rect.anchoredPosition = new Vector2(i * 110f, 0f);

            img = btnObj.AddComponent<UnityEngine.UI.Image>();
            img.sprite = currentSprites[i];
            img.color = Color.black;
            button = btnObj.AddComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => EvaluateResponse(index));

            float totalWidth = (currentSprites.Length - 1) * 110f;
            float startX = -totalWidth / 2f;
            rect.anchoredPosition = new Vector2(startX + i * 110f, 0f);
        }
    }

    private void SetupStimuliSet()
    {
        switch(currenStimuliSet) 
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

            case StimuliSet.SloaneLetters:
                currentSprites = sloanLetterSprites;
                break;
        }
    }

    private void SetupScreen()
    {
        screen = Object.Instantiate(screenPrefab);
        screenCanvas = screen.GetComponentInChildren<Canvas>();

        GameObject obj;
        UnityEngine.UI.Image img;
        RectTransform rect;

        obj = new GameObject("Stimulus");
        img = obj.AddComponent<UnityEngine.UI.Image>();
        rect = obj.GetComponent<RectTransform>();
        obj.transform.SetParent(screenCanvas.transform, false);

        stimulusImage = img;
    }
    private void ApplyRotation(UnityEngine.UI.Image img, int shapeIndex)
    {
        if (currenStimuliSet == StimuliSet.LandoltC)
            img.transform.rotation = Quaternion.Euler(0, 0, landoltCRotations[shapeIndex]);

        else if (currenStimuliSet == StimuliSet.TumblingE)
            img.transform.rotation = Quaternion.Euler(0, 0, tumblingERotations[shapeIndex]);
    }
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

    private void EvaluateResponse(int answer)
    {   
        void EndTask()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        bool correct = answer == currentStimulusIndex;
        if (correct)
        {
            Debug.Log("Correct!");
            SaveToCSV(answer,correct);
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
            if(incorrectStreak >= incorrectThreshold) 
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

        else DisplayStimulus();
    }

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

    private void SaveToCSV(int playerAnswer, bool correct)
    {
        string line = $"{trialNumber},{currenStimuliSet},{playerAnswer},{correct},{currentSize},{correctStreak},{responseTime}\n";
        File.AppendAllText(csvPath, line);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void SetupCSV()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        csvPath = Application.persistentDataPath + "/visual_acuity_" + timestamp + ".csv";
        File.WriteAllText(csvPath, "TrialNumber,StimuliSet,PlayerAnswer,Correct,CurrentSize,CorrectStreak,ResponseTime\n");
    }
    void Start()
    {
        SetupCSV();
        SetupStimuliSet();
        SetupScreen();
        if (currenStimuliSet == StimuliSet.SloaneLetters || currenStimuliSet == StimuliSet.GeoShapes)
            SetupAnswerButtons();
        sizeStep = (initialSize - finishingSize)/totalTrials;
        currentSize = initialSize;
        DisplayStimulus();
    }

    // Update is called once per frame
    void Update()
    {
        screen.transform.position = position;
        screen.transform.rotation = Quaternion.Euler(rotation);
        screen.transform.localScale = new Vector3(Scale, Scale, 1f);
        if(currenStimuliSet == StimuliSet.TumblingE) HandleInputTumblingE();
        else if(currenStimuliSet == StimuliSet.LandoltC) HandleInputLandoltC();
    }
}
