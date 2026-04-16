using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class MatchingTaskController : MonoBehaviour
{

    [SerializeField] private Vector3 Screen1;
    [SerializeField] private Vector3 Screen2;
    [SerializeField] private Vector3 Screen3;
    [SerializeField] private GameObject screenPrefab;

    [SerializeField] private Sprite landoltRingSprite;

    private ScreenManager1 screenManager;
    private List<GameObject> screens;

    private List<Vector3> positions = new List<Vector3>();

    private string[] sloanLetters = { "D", "H", "K", "N", "R", "S", "V", "Z" };
    private float[] landoltCRings = { 0, 45, 90, 135, 180, 225, 270, 315 };

    private int sloanLetterIndex;
    private int landoltCIndex;
    private bool isMatch;

    private Canvas canvas1;
    private Canvas canvas2;
    private Canvas canvas3; 

    private TextMeshProUGUI letterText;
    private UnityEngine.UI.Image ringImage;
    private TextMeshProUGUI tableText;
    private GameObject tableContainer;
    private GameObject letterRow;
    private GameObject ringRow;



    private void StartTrial()
    {
        sloanLetterIndex = Random.Range(0, sloanLetters.Length);
        landoltCIndex = Random.Range(0, landoltCRings.Length);

        isMatch = Random.value > 0.5f;

        if (isMatch)
        {
            landoltCIndex = sloanLetterIndex;
        }
        else
        {
            do
            {
                landoltCIndex = Random.Range(0, landoltCRings.Length);
            } while (landoltCIndex == sloanLetterIndex);
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
        {
            Debug.Log("Richtig!");
        }
        else
        {
            Debug.Log("Falsch!");
        }
        StartTrial();
    }

    private void SetupScreens()
    {
        // Screen 1
        GameObject letterObj = new GameObject("LetterText");
        letterObj.transform.SetParent(canvas1.transform, false);
        letterText = letterObj.AddComponent<TextMeshProUGUI>();

        RectTransform rect = letterObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;


        //Screen2
        GameObject ringObj = new GameObject("ringImage");
        ringObj.transform.SetParent(canvas2.transform, false);
        ringImage = ringObj.AddComponent<UnityEngine.UI.Image>();

        rect = ringObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.offsetMin=Vector2.zero;
        rect.offsetMax=Vector2.zero;
        rect.sizeDelta = new Vector2(100, 100);
        rect.anchoredPosition = Vector2.zero;

        //Screen3
        GameObject tabelObj = new GameObject("tableText");

        

        tabelObj.transform.SetParent(canvas3.transform, false);
        tableText = tabelObj.AddComponent<TextMeshProUGUI>();



        rect = tabelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        tableContainer = new GameObject("TableContainer");
        tableContainer.AddComponent<RectTransform>();
        tableContainer.transform.SetParent(canvas3.transform, false);
        
        rect = tableContainer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        


        GameObject letterRowObj = new GameObject("LetterRow");
        letterRowObj.AddComponent<RectTransform>();
        letterRowObj.transform.SetParent(tableContainer.transform, false);
        

        rect = letterRowObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GridLayoutGroup grid = letterRowObj.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 8;
        grid.cellSize = new Vector2(100, 100);
        grid.padding.bottom = 200;
        grid.childAlignment = TextAnchor.MiddleCenter;

        letterRow = letterRowObj;

        GameObject ringRowObj = new GameObject("RingRow");
        ringRowObj.AddComponent<RectTransform>();
        ringRowObj.transform.SetParent(tableContainer.transform, false);
       

        rect = ringRowObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect .anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        grid = ringRowObj.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 8;
        grid.cellSize = new Vector2(100, 100);
        grid.padding.top = 200;
        grid.childAlignment = TextAnchor.MiddleCenter;
        ringRow = ringRowObj;
    }

    private void DisplayTrial()
    {
        foreach (Transform child in letterRow.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in ringRow.transform)
        {
            Destroy(child.gameObject);
        }
        //-----
        letterText.text = sloanLetters[sloanLetterIndex];

        ringImage.sprite=landoltRingSprite;
        ringImage.transform.rotation = Quaternion.Euler(0,0, landoltCRings[landoltCIndex]);

        int[] shuffled = ShuffleIndices();
        // line with letters
        foreach( int i in shuffled)
        {
            GameObject letterCell = new GameObject("LetterCell");
            letterCell.transform.SetParent(letterRow.transform, false);
            TextMeshProUGUI cellText = letterCell.AddComponent<TextMeshProUGUI>();
            cellText.text = sloanLetters[i];
            cellText.fontSize = 4 * 36;
        }
        // line with Landolt-C-Rings
        foreach(int i in shuffled)
        {
            GameObject ringCell = new GameObject("RingCell");
            ringCell.transform.SetParent(ringRow.transform, false);
            UnityEngine.UI.Image cellImg = ringCell.AddComponent<UnityEngine.UI.Image>();
            cellImg.sprite = landoltRingSprite;
            cellImg.transform.rotation = Quaternion.Euler(0, 0, landoltCRings[i]);
        }

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


    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        positions.Add(Screen1);
        positions.Add(Screen2);
        positions.Add(Screen3);
        screenManager = new ScreenManager1(screenPrefab, new Vector3(0f, 180f, 0f));
        screens = screenManager.GenerateScreens(positions);

        canvas1 = screens[0].GetComponentInChildren<Canvas>();
        canvas2 = screens[1].GetComponentInChildren<Canvas>();
        canvas3 = screens[2].GetComponentInChildren<Canvas>();

        SetupScreens();
        Debug.Log("LetterRow parent: " + letterRow.transform.parent.name);
        Debug.Log("RingRow parent: " + ringRow.transform.parent.name);
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
