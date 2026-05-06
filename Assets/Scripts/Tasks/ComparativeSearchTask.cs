using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using UnityEngine.UIElements;
using System.Linq;


public class ComparativeSearchTask : MonoBehaviour
{
    //private enum LayoutType { Linear, Grid}

    //private VarScreenManager screenManager;
    private ScreenManager1 screenManager1;
    [SerializeField] private List<Vector3> screenPositions;
    [SerializeField] private GameObject screenPrefab;
    //[SerializeField] private LayoutType layoutType;
    //[SerializeField] private int screenCount;
    //[SerializeField] private int rowCount;
    //[SerializeField] private int columnCount;
    private List<GameObject> screens;
    //[SerializeField] private float screenSpacing;
    //[SerializeField] private float verticalSpacing;
    

    
    [SerializeField] private Sprite[] GeometricSprites;
    [SerializeField] private int objectsPerScreen;
    private Color[] objectColors = new Color[] 
    {
        new(0.2f, 0.2f, 0.2f), // dunkelgrau
        new(0.5f, 0.5f, 0.5f), // mittelgrau
        new(0.8f, 0.8f, 0.8f)  // hellgrau
    };
    private Sprite[] objectShapes;
    private List<Canvas> canvas = new();

    private List<ScreenObject> baseObjects;

    private bool hasMismatch;
    private int mismatchScreenIndex;
    private enum MismatchType { Color, Shape, Position, Missing};
    private int mismatchObjectIndex;
    [SerializeField] private MismatchType currentMismatchType;


    //Logging
    private int trialNumber;
    private float trialStartTime;
    private float responseTime;
    private string csvPath;

    // stimuli auswahl
    private enum StimuliSet
    {
        GeometricShapes,
        LandoltC,
        SloaneLetters,
        TumblingE
    }
    [SerializeField] private Sprite landoltC;
    private float[] landoltCRotations = {0f,45f,90f,135f,180f,225f,270,315f };
    [SerializeField] private Sprite tumblingE;
    private float[] tumblingERotations = { 0f, 90f, 180f, 270f };
    [SerializeField] private Sprite[] sloanLetterSprites;

    [SerializeField]private StimuliSet currentStimuliSet;

    // Random Grayscale
    [SerializeField] private bool randomGrayscale;

    //Distribution Type

    private enum Distribution
    {
        Random,
        Row,
        Column
    }

    [SerializeField] private Distribution distribution;
    [SerializeField] private float stimuliSpacing;
    

    
    
    private void ApplyRotation(UnityEngine.UI.Image img, int shapeIndex)
    {
        if (currentStimuliSet == StimuliSet.LandoltC)
            img.transform.rotation = Quaternion.Euler(0, 0, landoltCRotations[shapeIndex]);

        else if (currentStimuliSet == StimuliSet.TumblingE)
            img.transform.rotation = Quaternion.Euler(0, 0, tumblingERotations[shapeIndex]);
    }
    private void SetupStimuliSet()
    {
        switch (currentStimuliSet)
        {
            case StimuliSet.GeometricShapes:
                objectShapes = GeometricSprites; 
                break;
            case StimuliSet.LandoltC:
                objectShapes = Enumerable.Repeat(landoltC,8).ToArray();
                break;
            case StimuliSet.SloaneLetters:
                objectShapes = sloanLetterSprites;
                break;
            case StimuliSet.TumblingE:
                objectShapes = Enumerable.Repeat(tumblingE,4).ToArray();
                break;

        }
    }

    private void GenerateObjects() {
        
        
        float gray;
        Color grayScale;
        
        GameObject obj;
        UnityEngine.UI.Image img;
        RectTransform rect;

        ClearScreens();


        if (randomGrayscale)
            for (int i = 0; i < baseObjects.Count; i++)
            {
                gray = Random.Range(0.2f, 0.8f);
                baseObjects[i].SetGrayValue(gray);
            }

        for (int i = 0; i < screens.Count; i++)
            canvas.Add(screens[i].GetComponentInChildren<Canvas>());

        for (int i = 0;i < canvas.Count; i++)
            for (int j = 0; j < objectsPerScreen; j++)
            {
                obj = new GameObject("Object");
                img = obj.AddComponent<UnityEngine.UI.Image>();
                rect = obj.GetComponent<RectTransform>();

                obj.transform.SetParent(canvas[i].transform, false);
                
                img.sprite = objectShapes[baseObjects[j].GetShapeIndex()];
                ApplyRotation(img, baseObjects[j].GetShapeIndex());
                rect.anchoredPosition = baseObjects[j].GetPosition();
                rect.sizeDelta = new Vector2(50f, 50f);

                if (randomGrayscale)
                {
                    float baseGray = baseObjects[j].GetGrayValue();
                    grayScale = new Color(baseGray, baseGray, baseGray);
                    img.color = grayScale;
                } 
                else img.color = objectColors[baseObjects[j].GetColorIndex()];

                

                if (i == mismatchScreenIndex && j == mismatchObjectIndex)
                {
                    if (currentMismatchType == MismatchType.Missing) continue;
                    ApplyMismatchToStimuli(img, rect, j);
                }
                    
            }
        
    }

    private void ApplyMismatchToStimuli(UnityEngine.UI.Image img,RectTransform rect, int j)
    {
        switch (currentMismatchType)
        {
            case MismatchType.Color:
                int newColorIndex;
                do newColorIndex = Random.Range(0, objectColors.Length);
                while (newColorIndex == baseObjects[j].GetColorIndex());

                if (randomGrayscale)
                {
                    float newGray;
                    do newGray = Random.Range(0.2f, 0.8f);
                    while (Mathf.Approximately(newGray, baseObjects[j].GetGrayValue()));
                    img.color = new Color(newGray, newGray, newGray);
                }
                else img.color = objectColors[newColorIndex];

                break;

            case MismatchType.Shape:
                int newShapesIndex;
                do newShapesIndex = Random.Range(0, objectShapes.Length);
                while (newShapesIndex == baseObjects[j].GetShapeIndex());
                img.sprite = objectShapes[newShapesIndex];
                ApplyRotation(img, baseObjects[j].GetShapeIndex());
                break;

            case MismatchType.Position:
                rect.anchoredPosition = baseObjects[j].GetPosition() + new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f));
                break;

            default:
                break;
        }
    }

    private void GenerateTrial() 
    {
        trialStartTime = Time.time;
        baseObjects = new List<ScreenObject>();
        for (int i = 0; i < objectsPerScreen; i++) 
        {
            baseObjects.Add(new ScreenObject(
                Random.Range(0, objectShapes.Length),
                Random.Range(0, objectColors.Length),
                CalculateStimuliPosition(i))
                );
        }

        hasMismatch = Random.value > 0.5f;
        
       
        if (hasMismatch){
            mismatchScreenIndex = Random.Range(0, screens.Count);
            mismatchObjectIndex = Random.Range(0, objectsPerScreen);
        }
        
        else mismatchScreenIndex = -1;
    }

    private Vector2 CalculateStimuliPosition(int index)
    { 
        float calculateCenter()
        {
            float totalHW = (objectsPerScreen - 1) * stimuliSpacing; // Total Height\Width
            float startXY = totalHW / 2f; // Startposition of X \ Y
            return startXY;
        }

        bool IsTooClose(Vector2 newPos, int currentIndex)
        {
            for (int i = 0; i < currentIndex; i++)
                if(Vector2.Distance(newPos, baseObjects[i].GetPosition()) < stimuliSpacing)
                    return true;
            return false;
        }
        switch (distribution)
        {

            case Distribution.Random:
                Vector2 newPos;
                do newPos = new Vector2(Random.Range(-400f, 400f), Random.Range(-300f, 300f));
                while (IsTooClose(newPos, index));
                
                return newPos;
            
            case Distribution.Column:
                
                return new(0f, calculateCenter() - index * stimuliSpacing);

            case Distribution.Row:
                
                return new(calculateCenter() - index * stimuliSpacing, 0f);

            default:
                return new Vector2(Random.Range(-400f, 400f), Random.Range(-300f, 300f));
        }

       
        
    }

    private void AdjustScreenSize()
    {
        
        Canvas c;
        UnityEngine.UI.Image bg;
        RectTransform rect;
        RectTransform bgRect;
        float offset;

        float ScreenSize() { return objectsPerScreen * stimuliSpacing; }
        void SetRect(int i) 
        {
            c = screens[i].GetComponentInChildren<Canvas>();
            bg = screens[i].GetComponentInChildren<UnityEngine.UI.Image>();
            rect = c.GetComponent<RectTransform>();
            bgRect = bg.GetComponent<RectTransform>();
        }

        for (int i = 0; i < screens.Count; i++)
        {
            switch (distribution)
            {
                case Distribution.Column:
                    SetRect(i);
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, ScreenSize());
                    bgRect.sizeDelta = new Vector2(bgRect.sizeDelta.x, ScreenSize());
                    break;

                case Distribution.Row:
                    SetRect(i);
                    rect.sizeDelta = new Vector2(ScreenSize(), rect.sizeDelta.y);
                    bgRect.sizeDelta = new Vector2(ScreenSize(), rect.sizeDelta.y);
                    break;

                default:
                    break;
            }

            //offset = i * (ScreenSize() + screenSpacing) * 0.001f;
            /*if(layoutType == LayoutType.Linear)
            screens[i].transform.position = new Vector3(offset,
                                                        screens[i].transform.position.y,
                                                        screens[i].transform.position.z);*/

            
        }

    }
    private void EvaluateResponse(int answer)
    {
        bool correct;
        if (hasMismatch) correct = answer == mismatchScreenIndex +1;
        else correct = answer == 0;

        Debug.Log(correct ? "Correct!" : "Wrong!");
        responseTime = Time.time - trialStartTime;
        SaveToCSV(correct, answer);
        
        trialNumber++;
        
        GenerateTrial();
        GenerateObjects();

    }

    private void HandleInput()
    {
        
        if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame)
            EvaluateResponse(0);
            
        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            EvaluateResponse(1);

        if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            EvaluateResponse(2);

        if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            EvaluateResponse(3);

        if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
            EvaluateResponse(4);

        if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame)
            EvaluateResponse(5);

        if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame)
            EvaluateResponse(6);

        if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame)
            EvaluateResponse(7);

        if (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame)
            EvaluateResponse(8);

        if (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame)
            EvaluateResponse(9);



    }
       
    private void ClearScreens()
    {
        foreach (Canvas c in canvas)
            foreach (Transform child in c.transform)
                if (child.gameObject.name == "Object")
                    Destroy(child.gameObject);

        canvas.Clear();
    }

    private void SaveToCSV(bool correct, int playerAnswer)
    {
        string line = $"{trialNumber},{responseTime},{hasMismatch},{mismatchScreenIndex},{playerAnswer},{correct},{currentMismatchType}\n";
        File.AppendAllText(csvPath, line);
    }
    void Start()  // Start is called once before the first execution of Update after the MonoBehaviour is created
    {

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        csvPath = Application.persistentDataPath + "/comparative_search_" + timestamp + ".csv";
        File.WriteAllText(csvPath, "TrialNumber,ResponseTime,,HasMismatch,MismatchScreenIndex,PlayerAnswer,Correct,MismatchType\n");

        SetupStimuliSet();

        //screenManager = new VarScreenManager(screenPrefab);
        //if (layoutType == LayoutType.Linear) screens = screenManager.GenerateLinearScreens(screenCount); 
        //else screens = screenManager.GenerateGridScreens(rowCount, columnCount);

        screenManager1 = new ScreenManager1(screenPrefab,new Vector3(0f,0f,0f));
        screens = screenManager1.GenerateScreens(screenPositions);

        GenerateTrial();
        GenerateObjects();
        AdjustScreenSize();
        
        
    }
    void UpdatePositions()
    {
        for (int i = 0; i < screens.Count; i++)
        {
            screens[i].transform.position = screenPositions[i];
            float scale = Mathf.Max(0.1f,Mathf.Abs(screenPositions[i].z));
            screens[i].transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
    // Update is called once per frame
    void Update()
    {
        HandleInput();
        UpdatePositions();
    }
}

public class ScreenObject
{
    private int shapeIndex;
    private int colorIndex;
    private Vector2 position;
    private float grayValue;

    public ScreenObject(int shapeIndex, int colorIndex, Vector2 position)
    {
        this.shapeIndex = shapeIndex;
        this.colorIndex = colorIndex;
        this.position = position;
    }

    public int GetShapeIndex() { return this.shapeIndex; }
    public int GetColorIndex() {  return this.colorIndex; }

    public void SetGrayValue(float gray){ this.grayValue = gray; }
    

    public float GetGrayValue() { return this.grayValue; }
    public Vector2 GetPosition() { return this.position; }
}
