using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class NASATLXTask : MonoBehaviour
{
    [SerializeField] private string[] questions;
    [SerializeField] private string[] headers;
    private int currentQuestionIndex;
    private int[] answers;
    private string inputBuffer;

    //-----------------------------

    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float Scale;
    private GameObject screen;
    private TMPro.TextMeshProUGUI questionText;
    private TMPro.TextMeshProUGUI inputText;

    //-----------------------------
    private string csvPath;
    private void SetupScreen()
    {
        screen = Object.Instantiate(screenPrefab);
        Canvas screenCanvas = screen.GetComponentInChildren<Canvas>();

        TMPro.TextMeshProUGUI CreateText(string name, Vector2 anchorMin, Vector2 anchorMax, int fontsize)
        { 
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(screenCanvas.transform, false);
            TMPro.TextMeshProUGUI tmp = obj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = fontsize;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return tmp;
        }
        questionText = CreateText("QuestionText", new Vector2(0f, 0.5f), new Vector2(1f, 0.8f), 36);
        inputText = CreateText("InputText", new Vector2(0f, 0.2f), new Vector2(1f, 0.5f), 48);
    }

    private void DisplayQuestion()
    {
        questionText.text = questions[currentQuestionIndex];
        inputText.text = "";
        inputBuffer = "";
    }
    void EndTask()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    private void SaveToCSV()
    {
        string line = "";
        for (int i = 0; i < answers.Length; i++)
        {
            line += answers[i];
            if (i < answers.Length - 1) line += ",";
        }
        line += "\n";
        File.AppendAllText(csvPath, line);
        EndTask();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        answers = new int[questions.Length];
        string header = string.Join(",", headers) + "\n";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        csvPath = Application.persistentDataPath + "/nasa_tlx_" + timestamp + ".csv";
        File.WriteAllText(csvPath, header);
        SetupScreen();
        DisplayQuestion();
    }

    private void HandleInput()
    {
        if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame)
            inputBuffer += "0";

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            inputBuffer += "1";

        if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            inputBuffer += "2";

        if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            inputBuffer += "3";

        if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
            inputBuffer += "4";

        if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame)
            inputBuffer += "5";

        if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame)
            inputBuffer += "6";

        if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame)
            inputBuffer += "7";

        if (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame)
            inputBuffer += "8";

        if (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame)
            inputBuffer += "9";

        if (Keyboard.current.backspaceKey.wasPressedThisFrame && inputBuffer.Length > 0)
            inputBuffer = inputBuffer[..^1]; // last character is removed

        if (Keyboard.current.enterKey.wasPressedThisFrame && inputBuffer.Length > 0)
        {
            answers[currentQuestionIndex] = int.Parse(inputBuffer);
            currentQuestionIndex++;

            if (currentQuestionIndex >= questions.Length)
                SaveToCSV();
            else DisplayQuestion();
        }

        inputText.text = inputBuffer;

        
    }

    // Update is called once per frame
    void Update()
    {
        screen.transform.position = position;
        screen.transform.rotation = Quaternion.Euler(rotation);
        screen.transform.localScale = new Vector3(Scale, Scale, 1f);
        HandleInput();
    }
}
