using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    private ScreenManager1 screenManager;
    private GameObject screen;
    private TMPro.TextMeshProUGUI questionText;
    private TMPro.TextMeshProUGUI inputText;

    // --- CSV logging ----------------------------------------------
    private TrialLogger logger;

    // Builds the question screen with two text fields: one showing the
    // current question and one showing the digits typed so far. No
    // numerical scale is displayed alongside the question, since
    // participants state their rating verbally and the experimenter enters
    // it through the keyboard. The screen itself is instantiated by the
    // ScreenManager in Start.
    private void SetupScreen()
    {
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

    // Writes the collected answers as a single row through the shared
    // TrialLogger, matching the headers passed to it in Start, and ends the
    // task.
    private void SaveToCSV()
    {
        logger.WriteRow(System.Array.ConvertAll(answers, a => (object)a));
        TaskRunner.Exit();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        answers = new int[questions.Length];
        logger = new TrialLogger("nasa_tlx", headers);
        screenManager = new ScreenManager1(screenPrefab, rotation);
        screen = screenManager.GenerateScreens(new List<Vector3> { position })[0];
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
        if (DigitInput.TryGetDigit(out int digit))
            inputBuffer += digit;

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
        screenManager.UpdateTransform(position, rotation, scale);
        HandleInput();
    }
}
