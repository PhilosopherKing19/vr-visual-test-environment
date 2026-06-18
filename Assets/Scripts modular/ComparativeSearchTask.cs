using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;


public class ComparativeSearchTask : MonoBehaviour
{
    // --- Screen setup ---------------------------------------------------
    private ScreenManager1 screenManager1;
    [SerializeField] private List<Vector3> screenPositions;
    [SerializeField] private List<Vector3> screenRotations;
    [SerializeField] private float globalScale = 1;
    [SerializeField] private GameObject screenPrefab;
    private List<GameObject> screens;

    // --- Object generation ----------------------------------------------
    [SerializeField] private Sprite[] geometricSprites;
    [SerializeField] private int objectsPerScreen;
    private Color[] objectColors = new Color[]
    {
        new(0.2f, 0.2f, 0.2f), // dunkelgrau
        new(0.5f, 0.5f, 0.5f), // mittelgrau
        new(0.8f, 0.8f, 0.8f)  // hellgrau
    };
    private Sprite[] objectShapes;
    private List<Canvas> canvases = new();
    private List<ScreenObject> baseObjects;

    // --- Mismatch configuration ----------------------------------------------
    private bool hasMismatch;
    private int mismatchScreenIndex;
    private enum MismatchType { Color, Shape, Position, Missing };
    private int mismatchObjectIndex;
    [SerializeField] private MismatchType currentMismatchType;

    // --- Logging ----------------------------------------------
    private int trialNumber;
    private float trialStartTime;
    private float responseTime;
    private TrialLogger logger;

    // --- Stimuli selection ----------------------------------------------
    private enum StimuliSet
    {
        GeometricShapes,
        LandoltC,
        SloanLetters,
        TumblingE
    }
    [SerializeField] private Sprite landoltC;
    // The Landolt C orientation table is now shared via StimulusRotations.
    [SerializeField] private Sprite tumblingE;
    private readonly float[] tumblingERotations = { 0f, 90f, 180f, 270f };
    [SerializeField] private Sprite[] sloanLetterSprites;

    [SerializeField] private StimuliSet currentStimuliSet;

    // --- Random grayscale ----------------------------------------------
    [SerializeField] private bool randomGrayscale;

    // --- Distribution type ----------------------------------------------
    private enum Distribution
    {
        Random,
        Row,
        Column
    }

    [SerializeField] private Distribution distribution;
    [SerializeField] private float stimuliSpacing;


    // Rotates the stimulus image to the orientation corresponding to
    // shapeIndex. Only Landolt C and Tumbling E use rotation to encode the
    // displayed orientation; the other sets rely on distinct sprites
    // instead and are left unrotated.
    private void ApplyRotation(UnityEngine.UI.Image img, int shapeIndex)
    {
        if (currentStimuliSet == StimuliSet.LandoltC)
            img.transform.rotation = Quaternion.Euler(0, 0, StimulusRotations.LandoltC[shapeIndex]);

        else if (currentStimuliSet == StimuliSet.TumblingE)
            img.transform.rotation = Quaternion.Euler(0, 0, tumblingERotations[shapeIndex]);
    }

    // Fills objectShapes with the sprite set selected in the Inspector.
    // Landolt C and Tumbling E only need a single sprite repeated several
    // times, since their variation comes from rotation rather than
    // different sprites.
    private void SetupStimuliSet()
    {
        switch (currentStimuliSet)
        {
            case StimuliSet.GeometricShapes:
                objectShapes = geometricSprites;
                break;
            case StimuliSet.LandoltC:
                objectShapes = Enumerable.Repeat(landoltC, 8).ToArray();
                break;
            case StimuliSet.SloanLetters:
                objectShapes = sloanLetterSprites;
                break;
            case StimuliSet.TumblingE:
                objectShapes = Enumerable.Repeat(tumblingE, 4).ToArray();
                break;
        }
    }

    // Instantiates the objects for the current trial on every screen,
    // using the reference set in baseObjects so all screens show the same
    // objects except for the mismatch object on the mismatch screen. If
    // random grayscale is enabled, a fresh gray value is assigned to each
    // object before it is drawn.
    private void GenerateObjects()
    {
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
            canvases.Add(screens[i].GetComponentInChildren<Canvas>());

        for (int i = 0; i < canvases.Count; i++)
            for (int j = 0; j < objectsPerScreen; j++)
            {
                obj = new GameObject("Object");
                img = obj.AddComponent<UnityEngine.UI.Image>();
                rect = obj.GetComponent<RectTransform>();

                obj.transform.SetParent(canvases[i].transform, false);

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

    // Modifies the mismatch object's image or position according to the
    // configured mismatch type. The Missing type is not handled here, since
    // it is resolved earlier in GenerateObjects by skipping the object
    // entirely.
    private void ApplyMismatchToStimuli(UnityEngine.UI.Image img, RectTransform rect, int j)
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

    // Sets up a new trial: generates the reference object set shared by
    // all screens, then decides whether this trial has a mismatch and, if
    // so, which screen and object will carry it.
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

        if (hasMismatch)
        {
            mismatchScreenIndex = Random.Range(0, screens.Count);
            mismatchObjectIndex = Random.Range(0, objectsPerScreen);
        }
        else mismatchScreenIndex = -1;
    }

    // Computes the position of object index within a screen, depending on
    // the configured distribution. Random positions are rejection-sampled
    // against the objects placed so far to avoid overlap; Row and Column
    // positions are placed on an evenly spaced line centered on the screen.
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
                if (Vector2.Distance(newPos, baseObjects[i].GetPosition()) < stimuliSpacing)
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

    // Resizes each screen's canvas and background to fit objectsPerScreen
    // objects along the configured axis, so that no objects fall outside
    // the screen boundary regardless of how many objects are configured.
    // Random distribution does not need resizing, since objects are placed
    // within the screen's existing bounds.
    private void AdjustScreenSize()
    {
        Canvas c;
        UnityEngine.UI.Image bg;
        RectTransform rect;
        RectTransform bgRect;

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
        }
    }

    // Checks the player's response against the mismatch condition of the
    // current trial. Answer 0 is reserved for "no difference"; answers
    // 1..screens.Count correspond to screens 0..screens.Count-1, which is
    // why the mismatch screen index is compared against answer - 1.
    private void EvaluateResponse(int answer)
    {
        bool correct;
        if (hasMismatch) correct = answer == mismatchScreenIndex + 1;
        else correct = answer == 0;

        Debug.Log(correct ? "Correct!" : "Wrong!");
        responseTime = Time.time - trialStartTime;
        SaveToCSV(correct, answer);

        trialNumber++;

        GenerateTrial();
        GenerateObjects();
    }

    // Maps number row and numpad keys 0-9 to the corresponding response,
    // covering one button per screen plus the "no difference" button.
    private void HandleInput()
    {
        if (DigitInput.TryGetDigit(out int digit))
            EvaluateResponse(digit);
    }

    // Destroys all previously generated objects across all screens and
    // clears the canvas list, so GenerateObjects can rebuild it from
    // scratch for the next trial.
    private void ClearScreens()
    {
        foreach (Canvas c in canvases)
            foreach (Transform child in c.transform)
                if (child.gameObject.name == "Object")
                    Destroy(child.gameObject);

        canvases.Clear();
    }

    // Appends one line of trial data through the shared TrialLogger, matching
    // the column order passed to it in Start.
    private void SaveToCSV(bool correct, int playerAnswer)
    {
        logger.WriteRow(trialNumber, responseTime, hasMismatch, mismatchScreenIndex, playerAnswer, correct, currentMismatchType);
    }

    void Start()  // Start is called once before the first execution of Update after the MonoBehaviour is created
    {
        logger = new TrialLogger("comparative_search",
            new[] { "TrialNumber", "ResponseTime", "HasMismatch", "MismatchScreenIndex", "PlayerAnswer", "Correct", "MismatchType" });

        SetupStimuliSet();

        screenManager1 = new ScreenManager1(screenPrefab, new Vector3(0f, 0f, 0f));
        screens = screenManager1.GenerateScreens(screenPositions);

        GenerateTrial();
        GenerateObjects();
        AdjustScreenSize();
    }

    // Updates every screen's position, depth-based scale, and rotation
    // each frame from the configured Inspector values.
    void UpdatePositions()
    {
        screenManager1.UpdateTransforms(screenPositions, screenRotations, globalScale);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        UpdatePositions();
    }
}

// Stores the shape, color, position, and (optionally) grayscale value of a
// single object on a screen, used as both the reference object shared
// across screens and the basis for mismatch modifications.
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
    public int GetColorIndex() { return this.colorIndex; }

    public void SetGrayValue(float gray) { this.grayValue = gray; }

    public float GetGrayValue() { return this.grayValue; }
    public Vector2 GetPosition() { return this.position; }
}
