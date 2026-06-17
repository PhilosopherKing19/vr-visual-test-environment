using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class NASATLXTask : MonoBehaviour
{
    // --- Questionnaire content ----------------------------------------------
    [SerializeField] private string[] questions;
    [SerializeField] private string[] headers;
    private int currentQuestionIndex;
    private int[] answers;
    private string inputBuffer;

    // --- Screen setup ---------------------------------------------------
    [SerializeField] private GameObject screenPrefab;
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float scale;
    private GameObject screen;
    private TMPro.TextMeshProUGUI questionText;
    private TMPro.TextMeshProUGUI inputText;

    // --- CSV logging ----------------------------------------------
    private string csvPath;

    // Builds the question screen with two text fields: one showing the
    // current question and one showing the digits typed so far. No
    // numerical scale is displayed alongside the question, since
    // participants state their rating verbally and the experimenter enters
    // it through the keyboard.
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

    // Shows the next question and clears the input buffer so the previous
    // answer's digits do not carry over.
    private void DisplayQuestion()
    {
        questionText.text = questions[currentQuestionIndex];
        inputText.text = "";
        inputBuffer = "";
    }

    // Stops play mode in the editor or quits the built application,
    // depending on which environment the task is running in.
    void EndTask()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Writes the collected answers as a single comma-separated line,
    // matching the header written in Start, and ends the task.
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
        questionText.color = Color.black;
        questionText.enableAutoSizing = true;
        inputText.color = Color.black;
        inputText.enableAutoSizing = true;
        DisplayQuestion();
    }

    // Builds up inputBuffer from individual digit keypresses, allows
    // deleting the last digit with backspace, and confirms the answer with
    // enter. The typed value is parsed and stored as is, with no check
    // against the 1-20 rating range communicated to participants verbally;
    // validating the entered value is intentionally left to the
    // experimenter rather than enforced here.
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
        screen.transform.localScale = new Vector3(scale, scale, 1f);
        HandleInput();
    }
}
