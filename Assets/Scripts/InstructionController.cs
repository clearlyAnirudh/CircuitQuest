using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private Button startLessonButton;
    [SerializeField] private Button doneButton;
    [SerializeField] private Button quitLessonButton;

    [Header("Instruction Text")]
    [SerializeField] private TMP_Text stepTitle;
    [SerializeField] private TMP_Text instructionText;

    [Header("Component Holograms")]
    [SerializeField] private GameObject resistorHologram;
    [SerializeField] private GameObject tactileButtonHologram;
    [SerializeField] private GameObject ledHologram;

    [Header("Circuit Wires")]
    [SerializeField] private GameObject wirePlusResistor;
    [SerializeField] private GameObject wireLedMinus;

    [Header("Hole Guidance")]
    [SerializeField] private HoleOverlayController holeOverlay;

    private int currentStep = 0;
    private bool lessonActive = false;
    private bool uiInitialized = false;

    private void Awake()
    {
        // Give the spawned OverlayRoot a moment to exist alongside
        // the scene UI before attempting to bind the controls.
        Invoke(nameof(InitializeUI), 0.1f);
    }

    private void Start()
    {
        lessonActive = false;
        currentStep = 0;

        HideAllHolograms();

        if (holeOverlay != null)
            holeOverlay.ClearHoles();
    }

    private void InitializeUI()
    {
        FindSceneUI();

        if (instructionPanel == null ||
            startLessonButton == null ||
            doneButton == null ||
            quitLessonButton == null ||
            stepTitle == null ||
            instructionText == null)
        {
            Debug.LogError(
                "InstructionController: UI initialization failed. " +
                "One or more UI references could not be found."
            );

            return;
        }

        uiInitialized = true;

        // Force the application into normal mode.
        lessonActive = false;
        currentStep = 0;

        instructionPanel.SetActive(false);

        startLessonButton.gameObject.SetActive(true);
        doneButton.gameObject.SetActive(false);
        quitLessonButton.gameObject.SetActive(false);

        if (holeOverlay != null)
            holeOverlay.ClearHoles();

        // Prevent duplicate listeners if this method is ever called again.
        startLessonButton.onClick.RemoveListener(StartLesson);
        doneButton.onClick.RemoveListener(NextStep);
        quitLessonButton.onClick.RemoveListener(QuitLesson);

        startLessonButton.onClick.AddListener(StartLesson);
        doneButton.onClick.AddListener(NextStep);
        quitLessonButton.onClick.AddListener(QuitLesson);

        Debug.Log(
            "InstructionController: UI initialized successfully."
        );
    }

    private void FindSceneUI()
    {
        Transform[] allTransforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Include
        );

        foreach (Transform target in allTransforms)
        {
            switch (target.name)
            {
                case "InstructionPanel":
                    instructionPanel = target.gameObject;
                    break;

                case "StartLessonButton":
                    startLessonButton =
                        target.GetComponent<Button>();
                    break;

                case "DoneButton":
                    doneButton =
                        target.GetComponent<Button>();
                    break;

                case "QuitLessonButton":
                    quitLessonButton =
                        target.GetComponent<Button>();
                    break;

                case "StepTitleText":
                    stepTitle =
                        target.GetComponent<TMP_Text>();
                    break;

                case "InstructionText":
                    instructionText =
                        target.GetComponent<TMP_Text>();
                    break;
            }
        }

        Debug.Log(
            "InstructionController UI lookup:\n" +
            "InstructionPanel: " + (instructionPanel != null) + "\n" +
            "StartLessonButton: " + (startLessonButton != null) + "\n" +
            "DoneButton: " + (doneButton != null) + "\n" +
            "QuitLessonButton: " + (quitLessonButton != null) + "\n" +
            "StepTitleText: " + (stepTitle != null) + "\n" +
            "InstructionText: " + (instructionText != null)
        );
    }

    private void OnDestroy()
    {
        if (startLessonButton != null)
            startLessonButton.onClick.RemoveListener(StartLesson);

        if (doneButton != null)
            doneButton.onClick.RemoveListener(NextStep);

        if (quitLessonButton != null)
            quitLessonButton.onClick.RemoveListener(QuitLesson);
    }

    private void StartLesson()
    {
        if (!uiInitialized)
        {
            Debug.LogWarning(
                "InstructionController: UI is not initialized yet."
            );

            return;
        }

        lessonActive = true;
        currentStep = 0;

        HideAllHolograms();

        if (holeOverlay != null)
            holeOverlay.ClearHoles();

        // Hide Start.
        startLessonButton.gameObject.SetActive(false);

        // Show lesson panel.
        instructionPanel.SetActive(true);

        // Show Done.
        doneButton.gameObject.SetActive(true);

        // Keep Quit hidden until the lesson is complete.
        quitLessonButton.gameObject.SetActive(false);

        ShowStep(currentStep);
    }

    private void ShowStep(int step)
    {
        HideAllHolograms();

        switch (step)
        {
            case 0:
                ShowResistorStep();
                break;

            case 1:
                ShowButtonStep();
                break;

            case 2:
                ShowLedStep();
                break;

            case 3:
                ShowPositiveWireStep();
                break;

            case 4:
                ShowNegativeWireStep();
                break;

            default:
                FinishLesson();
                break;
        }
    }

    private void HideAllHolograms()
    {
        if (resistorHologram != null)
            resistorHologram.SetActive(false);

        if (tactileButtonHologram != null)
            tactileButtonHologram.SetActive(false);

        if (ledHologram != null)
            ledHologram.SetActive(false);
    }

    private void ShowResistorStep()
    {
        stepTitle.text = "STEP 1";

        instructionText.text =
            "Place the resistor.\n" +
            "Insert Pin A into B7 and Pin B into B20\n" +
            "Look for the glowing points";

        if (resistorHologram != null)
            resistorHologram.SetActive(true);

        if (holeOverlay != null)
            holeOverlay.ShowHoles("c7", "c15");
    }

    private void ShowButtonStep()
    {
        stepTitle.text = "STEP 2";

        instructionText.text =
            "Place the pushbutton.\n\n" +
            "Pin A -> E15\n" +
            "Pin B -> E19";

        if (tactileButtonHologram != null)
            tactileButtonHologram.SetActive(true);

        if (holeOverlay != null)
            holeOverlay.ShowHoles("e15", "e19");
    }

    private void ShowLedStep()
    {
        stepTitle.text = "STEP 3";

        instructionText.text =
            "Place the LED.\n\n" +
            "Anode -> B19\n" +
            "Cathode -> B20";

        if (ledHologram != null)
            ledHologram.SetActive(true);

        if (holeOverlay != null)
            holeOverlay.ShowHoles("b19", "b20");
    }

    private void ShowPositiveWireStep()
    {
        stepTitle.text = "STEP 4";

        instructionText.text =
            "Connect the positive rail\n" +
            "to the resistor row.";

        ShowWire(wirePlusResistor);

        if (holeOverlay != null)
            holeOverlay.ShowHoles("L+5", "a7");
    }

    private void ShowNegativeWireStep()
    {
        stepTitle.text = "STEP 5";

        instructionText.text =
            "Connect the LED cathode\n" +
            "to the negative rail.";

        ShowWire(wireLedMinus);

        if (holeOverlay != null)
            holeOverlay.ShowHoles("a20", "L-6");
    }

    private void ShowWire(GameObject wire)
    {
        if (wire != null)
            wire.SetActive(true);
    }

    private void NextStep()
    {
        if (!lessonActive)
            return;

        currentStep++;
        ShowStep(currentStep);
    }

    private void FinishLesson()
    {
        lessonActive = false;

        HideAllHolograms();

        if (holeOverlay != null)
            holeOverlay.ClearHoles();

        // Leave both physical circuit wires visible.
        if (wirePlusResistor != null)
            wirePlusResistor.SetActive(true);

        if (wireLedMinus != null)
            wireLedMinus.SetActive(true);

        stepTitle.text = "CIRCUIT COMPLETE";

        instructionText.text =
            "The PB + LED circuit is complete.";

        doneButton.gameObject.SetActive(false);

        // Only now show Quit Lesson.
        quitLessonButton.gameObject.SetActive(true);

        // Keep Start hidden while completed.
        startLessonButton.gameObject.SetActive(false);
    }

    private void QuitLesson()
    {
        lessonActive = false;
        currentStep = 0;

        HideAllHolograms();

        if (holeOverlay != null)
            holeOverlay.ClearHoles();

        // Keep the completed circuit visible.
        if (wirePlusResistor != null)
            wirePlusResistor.SetActive(true);

        if (wireLedMinus != null)
            wireLedMinus.SetActive(true);

        // Hide lesson UI.
        instructionPanel.SetActive(false);

        doneButton.gameObject.SetActive(false);
        quitLessonButton.gameObject.SetActive(false);

        // Return to normal mode.
        startLessonButton.gameObject.SetActive(true);
    }
}