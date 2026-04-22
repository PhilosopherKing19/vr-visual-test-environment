using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.IO;
using System.Net;

public class ComparativeSearchTask : MonoBehaviour
{
    private enum LayoutType { Linear, Grid}

    private VarScreenManager screenManager;
    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private int activeScreenIndex;
    [SerializeField] private LayoutType layoutType;
    [SerializeField] private int screenCount;
    [SerializeField] private int rowCount;
    [SerializeField] private int columnCount;
    private List<GameObject> screens;


    
    [SerializeField] private Sprite[] GeometricSprites;
    [SerializeField] private int objectsPerScreen;
    private Color[] objectColors = new Color[] 
    {
        new Color(0.2f, 0.2f, 0.2f), // dunkelgrau
        new Color(0.5f, 0.5f, 0.5f), // mittelgrau
        new Color(0.8f, 0.8f, 0.8f)  // hellgrau
    };
    private Sprite[] objectShapes;
    private List<Canvas> canvas = new List<Canvas>();

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
                objectShapes = new Sprite[] { landoltC, landoltC, landoltC, landoltC, landoltC, landoltC, landoltC, landoltC };
                break;
            case StimuliSet.SloaneLetters:
                objectShapes = sloanLetterSprites;
                break;
            case StimuliSet.TumblingE:
                objectShapes = new Sprite[] { tumblingE, tumblingE, tumblingE, tumblingE, };
                break;

        }
    }

    private void GenerateObjects() {

        ClearScreens();

        for (int i = 0; i < screens.Count; i++)
            canvas.Add(screens[i].GetComponentInChildren<Canvas>());

        for (int i = 0;i < canvas.Count; i++)
            for (int j = 0; j < objectsPerScreen; j++)
            {
                GameObject obj = new GameObject("Object");
                UnityEngine.UI.Image img = obj.AddComponent<UnityEngine.UI.Image>();
                RectTransform rect = obj.GetComponent<RectTransform>();

                obj.transform.SetParent(canvas[i].transform, false);
                //Debug.Log("Object created on canvas: " + canvas[i].name);

                img.sprite = objectShapes[baseObjects[j].GetShapeIndex()];
                ApplyRotation(img, baseObjects[j].GetShapeIndex());

                img.color = objectColors[baseObjects[j].GetColorIndex()];
                rect.anchoredPosition = baseObjects[j].GetPosition();
                rect.sizeDelta = new Vector2(50f, 50f);

                if (i == mismatchScreenIndex && j == mismatchObjectIndex)
                    switch (currentMismatchType)
                    {
                        case MismatchType.Color:
                            int newColorIndex;
                            do newColorIndex = Random.Range(0, objectColors.Length);
                            while (newColorIndex == baseObjects[j].GetColorIndex());
                            img.color = objectColors[newColorIndex];
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

                        case MismatchType.Missing:
                            continue;

                        default:
                            break;
                    }
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
                new Vector2(Random.Range(-400f, 400f), Random.Range(-300f, 300f)))
                );
        }

        hasMismatch = Random.value > 0.5f;
        
       
        if (hasMismatch){
            mismatchScreenIndex = Random.Range(0, screens.Count);
            mismatchObjectIndex = Random.Range(0, objectsPerScreen);
        }
        
        else mismatchScreenIndex = -1;
    }

    private void EvaluateResponse(int answer)
    {
        bool correct;
        if (hasMismatch) correct = answer == mismatchScreenIndex +1;
        else correct = answer == 0;
        Debug.Log(correct ? "Correct!" : "Wrong!");    
        responseTime = Time.time-trialStartTime;
        SaveToCSV(responseTime, correct, answer);
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
        {
            foreach (Transform child in c.transform)
            {
                if (child.gameObject.name == "Object")
                    Destroy(child.gameObject);
            }
        }
        canvas.Clear();
    }

    private void SaveToCSV(float ResponseTime,bool correct,int playerAnswer)
    {
        string line = $"{trialNumber},{responseTime},{hasMismatch},{mismatchScreenIndex},{playerAnswer},{correct},{currentMismatchType}\n";
        File.AppendAllText(csvPath, line);
    }
    void Start()  // Start is called once before the first execution of Update after the MonoBehaviour is created
    {

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        csvPath = Application.persistentDataPath + "/comparative_search_" + timestamp + ".csv";
        File.WriteAllText(csvPath, "TrialNumber,ResponseTime,HasMismatch,MismatchScreenIndex,PlayerAnswer,Correct,MismatchType\n");
        
        //print(csvPath);

        SetupStimuliSet();

        screenManager = new VarScreenManager(screenPrefab);
        if (layoutType == LayoutType.Linear) { screens = screenManager.GenerateLinearScreens(screenCount); }
        else screens = screenManager.GenerateGridScreens(rowCount, columnCount);
        
        //objectShapes = new Sprite[] { circleSprite, squareSprite, triangleSprite };
        GenerateTrial();
        GenerateObjects();
        
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

}

public class ScreenObject
{
    private int shapeIndex;
    private int colorIndex;
    private Vector2 position;

    public ScreenObject(int shapeIndex, int colorIndex, Vector2 position)
    {
        this.shapeIndex = shapeIndex;
        this.colorIndex = colorIndex;
        this.position = position;
    }

    public int GetShapeIndex() { return this.shapeIndex; }
    public int GetColorIndex() {  return this.colorIndex; }

    public Vector2 GetPosition() { return this.position; }
}
