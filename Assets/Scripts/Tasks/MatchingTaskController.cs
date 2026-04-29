using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using System.Runtime.CompilerServices;
using System.IO;


public class MatchingTaskController : MonoBehaviour
{

    [SerializeField] private Vector3 Screen1;
    [SerializeField] private Vector3 Screen2;
    [SerializeField] private Vector3 Screen3;
    [SerializeField] private GameObject screenPrefab;

   // [SerializeField] private Sprite landoltRingSprite;

    private ScreenManager1 screenManager;
    private List<GameObject> screens;

    private List<Vector3> positions = new List<Vector3>();
    private float[] landoltCRings = { 0, 45, 90, 135, 180, 225, 270, 315 };

    private int screenOneIndex;
    private int screenTwoIndex;
    private bool isMatch;

    private Canvas canvas1;
    private Canvas canvas2;
    private Canvas canvas3; 

    private UnityEngine.UI.Image screenOneImage;
    private UnityEngine.UI.Image screenTwoImage;
    //private TextMeshProUGUI tableText;
    private GameObject tableContainer;
    private GameObject letterRow;
    private GameObject ringRow;

    //--------------------------------
    private enum StimuliSet
    {
        Shapes,
        LandoltC,
        TumblingE,
        SloanLetters
    }

    [SerializeField] private StimuliSet screenOneStimuliSet;
    private Sprite[] screenOneSprites;

    [SerializeField] private StimuliSet screenTwoStimuliSet;
    private Sprite[] screenTwoSprites;

    [SerializeField] private Sprite landoltCSprite;
    [SerializeField] private Sprite tumblingESprite;

    [SerializeField] private Sprite[] sloanLetterSprites;
    [SerializeField] private Sprite[] shapeSprites;

    [SerializeField] private bool randomGrayscale;

    private int[] shuffled;
    
    private Color[] color =
    {
        new(0.2f, 0.2f, 0.2f), // dunkelgrau
        new(0.5f, 0.5f, 0.5f), // mittelgrau
        new(0.8f, 0.8f, 0.8f)  // hellgrau
    };

    [SerializeField] private bool matchByColor;
    private int screenOneColorIndex;
    private int screenTwoColorIndex;

    private string csvPath;
    private int trialNumber;
    private float trialStartTime;

    private void SetupStimuliSet()
    {
        switch (screenOneStimuliSet)
        {
            case StimuliSet.Shapes:
                screenOneSprites = shapeSprites;
                break;
            case StimuliSet .LandoltC:
                screenOneSprites = Enumerable.Repeat(landoltCSprite, 8).ToArray();
                break;
            case StimuliSet.TumblingE:
                screenOneSprites = Enumerable.Repeat(tumblingESprite, 8).ToArray();
                break;
            case StimuliSet .SloanLetters:
                screenOneSprites = sloanLetterSprites;
                break;
        }

        switch (screenTwoStimuliSet)
        {
            case StimuliSet.Shapes:
                screenTwoSprites = shapeSprites;
                break;
            case StimuliSet.LandoltC:
                screenTwoSprites = Enumerable.Repeat(landoltCSprite, 8).ToArray();
                break;
            case StimuliSet.TumblingE:
                screenTwoSprites = Enumerable.Repeat(tumblingESprite, 8).ToArray();
                break;
            case StimuliSet.SloanLetters:
                screenTwoSprites = sloanLetterSprites;
                break;
        }
    }
    private void StartTrial()
    {
        void SetRandomGrayTones()
        {
            if (randomGrayscale)
            {
                color = new Color[8];
                float gray;
                for (int i = 0; i < color.Length; i++)
                {
                    gray = Random.Range(0.2f, 0.8f);
                    color[i] = new Color(gray, gray, gray);
                }
            }
        }

        trialStartTime = Time.time;
        SetRandomGrayTones();
        shuffled = ShuffleIndices();
        screenOneIndex = shuffled[Random.Range(0, shuffled.Length)];
        screenTwoIndex = shuffled[Random.Range(0, shuffled.Length)];
        

        isMatch = Random.value > 0.5f;
        if (matchByColor)
        {
            screenOneColorIndex = Random.Range(0,color.Length);

            if (isMatch)
                screenTwoColorIndex = screenOneColorIndex;

            else do screenTwoColorIndex = Random.Range(0, color.Length);
                while (screenTwoColorIndex == screenOneColorIndex);
        }
        else
        {
            if (isMatch)
                screenTwoIndex = screenOneIndex;

            else do screenTwoIndex = shuffled[Random.Range(0, shuffled.Length)];
                while (screenTwoIndex == screenOneIndex);
        }
        DisplayTrial();
    }

    private void HandleInput()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame) EvaluateResponse(true);
        else if (Keyboard.current.nKey.wasPressedThisFrame) { EvaluateResponse(false); } 
    }

    private void EvaluateResponse(bool playerAnswer)
    {
        if (playerAnswer == isMatch)
            Debug.Log("Richtig!");
        else
            Debug.Log("Falsch!");

        float responseTime = Time.time - trialStartTime;
        SaveToCSV((playerAnswer == isMatch), playerAnswer, responseTime);
        trialNumber++;
        StartTrial();
    }

    private void SetupScreens()
    {
        UnityEngine.UI.Image CreateScreenImage(Canvas canvas)
        {
            GameObject obj = new("LetterText");
            obj.transform.SetParent(canvas.transform, false);
            UnityEngine.UI.Image img = obj.AddComponent<UnityEngine.UI.Image>();

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100, 100);
            rect.anchoredPosition = Vector2.zero;

            return img;
        }

        
        // Screen 1
        screenOneImage = CreateScreenImage(canvas1);
        
        //Screen2
        screenTwoImage = CreateScreenImage(canvas2);

        //Screen3

        void SetFullRect(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        GameObject tabelObj = new("tableText");
        tabelObj.transform.SetParent(canvas3.transform, false);
        tabelObj.AddComponent<RectTransform>();
        SetFullRect(tabelObj.GetComponent<RectTransform>());
        //---------------------------------------------------

        tableContainer = new GameObject("TableContainer");
        tableContainer.AddComponent<RectTransform>();
        tableContainer.transform.SetParent(canvas3.transform, false);
        SetFullRect(tableContainer.GetComponent<RectTransform>());
       // ----------------------------------------------------------
      
        GameObject CreateGridRow(string name, Transform parent, int padding, bool isTop)
        {
            GameObject rowObj = new(name);
            rowObj.AddComponent<RectTransform>();
            rowObj.transform.SetParent(parent,false);
            SetFullRect(rowObj.GetComponent<RectTransform>());

            GridLayoutGroup grid = rowObj.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;
            grid.cellSize = new Vector2(100, 100);
            grid.childAlignment = TextAnchor.MiddleCenter;
            if (isTop) grid.padding.top = padding;
            else grid.padding.bottom = padding;

            return rowObj;

        }

        letterRow = CreateGridRow("LetterRow",tableContainer.transform, 200, false);
        ringRow = CreateGridRow("RingRow", tableContainer.transform, 200, true);
    }

    private void ApplyRotation(UnityEngine.UI.Image img, int index, StimuliSet stimuliSet)
    {
        switch (stimuliSet)
        {
            case StimuliSet.LandoltC:
            case StimuliSet.TumblingE:
                img.transform.rotation = Quaternion.Euler(0, 0, landoltCRings[index]);
                break;
        }
    }
    private void DisplayTrial()
    {
        void SetColor(UnityEngine.UI.Image img)
        {
                int i = Random.Range(0, color.Length);
                img.color = color[i];
        }

        foreach (Transform child in letterRow.transform)
           Destroy(child.gameObject);
        
        foreach (Transform child in ringRow.transform)
            Destroy(child.gameObject);

        //-----------------------------------------------------------------
        void CreateCell(string name, Transform parent, Sprite[] sprites, StimuliSet stimuliSet, int index)
        {
            GameObject cell = new(name);
            cell.transform.SetParent(parent, false);
            UnityEngine.UI.Image img = cell.AddComponent<UnityEngine.UI.Image>();
            img.sprite = sprites[index];

            SetColor(img);
            ApplyRotation(img, index, stimuliSet);                                  
        }

        //-----screenOne
        screenOneImage.sprite = screenOneSprites[screenOneIndex];
        if (matchByColor) screenOneImage.color = color[screenOneColorIndex];
        else SetColor(screenOneImage);
        ApplyRotation(screenOneImage, screenOneIndex, screenOneStimuliSet);
        
        //-----screenTwo
        screenTwoImage.sprite = screenTwoSprites[screenTwoIndex];
        if (matchByColor) screenTwoImage.color = color[screenTwoColorIndex];
        else SetColor(screenTwoImage);
        ApplyRotation(screenTwoImage, screenTwoIndex, screenTwoStimuliSet);

        // filling the Table
        foreach ( int i in shuffled)
            CreateCell("LetterCell", letterRow.transform, screenOneSprites, screenOneStimuliSet, i);

        foreach (int i in shuffled)
            CreateCell("RingCell", ringRow.transform, screenTwoSprites, screenTwoStimuliSet, i);

    }

    private int[] ShuffleIndices()
    {
        int[] indices = { 0, 1, 2, 3, 4, 5, 6, 7 };
        for(int i = indices.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            // swap

            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }
        return indices;
    }

    private void SaveToCSV(bool correct, bool playerAnswer, float responseTime)
    {
        string line = $"{trialNumber},{responseTime},{isMatch},{playerAnswer},{correct},{matchByColor}\n";
        File.AppendAllText(csvPath, line);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        csvPath = Application.persistentDataPath + "/matching_task_" + timestamp + ".csv";
        File.WriteAllText(csvPath, "TrialNumber,ResponseTime,,IsMatch,PlayerAnswer,Correct,MatchByColor\n");

        trialStartTime = Time.time;
        positions.Add(Screen1);
        positions.Add(Screen2);
        positions.Add(Screen3);
        screenManager = new ScreenManager1(screenPrefab, new Vector3(0f, 180f, 0f));
        screens = screenManager.GenerateScreens(positions);

        canvas1 = screens[0].GetComponentInChildren<Canvas>();
        canvas2 = screens[1].GetComponentInChildren<Canvas>();
        canvas3 = screens[2].GetComponentInChildren<Canvas>();
        SetupStimuliSet();
        SetupScreens();
        StartTrial();
    }

    // Update is called once per frame
    void Update()
    {
        screens[0].transform.position = Screen1;
        float scale1 = Screen1.z < 0 ? -1 * Screen1.z : Screen1.z;
        screens[0].transform.localScale = new Vector3 (scale1, scale1, 1f);
        
        screens[1].transform.position = Screen2;
        float scale2 = Screen2.z < 0 ? -1 * Screen2.z : Screen2.z;
        screens[1].transform.localScale = new Vector3(scale2, scale2, 1f);

        screens[2].transform.position = Screen3;
        float scale3 = Screen3.z < 0 ? -1 * Screen3.z : Screen3.z;
        screens[2].transform.localScale = new Vector3(scale3, scale3, 1f);

        HandleInput();

    }
}
