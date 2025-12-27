using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

// Hold `~` to show the Developer Menu.
public class DeveloperMenu : MonoBehaviour
{
    public static DeveloperMenu Instance { get; private set; } // singleton

    private Canvas canvas;
    private GameObject panel;
    private Text titleText;
    private Text fpsText;
    private Text timerText;
    private Button pauseButton;
    private Button reloadButton;
    private Button stopwatchButton;
    private Button stopwatchResetButton;

    [SerializeField] private Transform debugTarget;
    private Text infoText;

    private string lastKeyPressed = "(none)"; // default key pressed text
    private KeyCode[] allKeys;

    private float deltaTime = 0.0f;
    private bool isPaused = false;

    // Timers
    private float inGameTime = 0f; // accumulates unscaled time since this component was active

    private bool stopwatchRunning = false;
    private float stopwatchTime = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // enforce singleton

        // cache all KeyCodes for dev key detection
        allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));

        CreateUI();
        canvas.enabled = false;
    }

    void Update()
    {
        // unscaled isolates inGameTime from Pause 
        if (!isPaused)
            inGameTime += Time.unscaledDeltaTime;

        // stopwatch
        if (stopwatchRunning)
            stopwatchTime += Time.unscaledDeltaTime;

        // detect last key pressed
        foreach (var k in allKeys)
        {
            if (Input.GetKeyDown(k))
            {
                lastKeyPressed = k.ToString();
                break;
            }
        }

        bool show = Input.GetKey(KeyCode.BackQuote); // `~` key
        if (canvas != null) canvas.enabled = show;

        if (show)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / Mathf.Max(0.0001f, deltaTime);
            if (fpsText != null) fpsText.text = string.Format("FPS: {0:0.}", fps);
            if (pauseButton != null) pauseButton.GetComponentInChildren<Text>().text = isPaused ? "Resume (Space)" : "Pause (Space)";

            if (timerText != null)
            {
                string running = stopwatchRunning ? "Running" : "Stopped";
                timerText.text = $"Game Time: {FormatTime(inGameTime)}\nStopwatch: {FormatTime(stopwatchTime)} ({running})";
            }

            if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
            if (Input.GetKeyDown(KeyCode.R)) ReloadScene();
            if (Input.GetKeyDown(KeyCode.T)) ToggleStopwatch();
            if (Input.GetKeyDown(KeyCode.Y)) ResetStopwatch();

            Transform target = debugTarget;
            if (target == null)
            {
                var targetObject = GameObject.FindWithTag("Player");
                if (targetObject != null) target = targetObject.transform;
                if (target == null && Camera.main != null) target = Camera.main.transform;
            }

            string info = $"Last Key: {lastKeyPressed}\n";
            if (target != null)
            {
                Vector3 worldPos = target.position;
                info += $"Target: {target.name}\nWorldPos: {worldPos.x:0.###}, {worldPos.y:0.###}, {worldPos.z:0.###}";
                try
                {
                    var gridManager = GridManager.Instance;
                    if (gridManager != null)
                    {
                        var node = gridManager.NodeFromWorldPos(worldPos);
                        if (node != null)
                        {
                            var sc = node.gridSector.sectorCoordinate;
                            info += $"\nSector: {sc.x}, {sc.y}\nNode Global: {node.globalX}, {node.globalY}\nNode Local: {node.localX}, {node.localY}";
                        }
                        else info += "\nNode: null";
                    }
                    else info += "\nGridManager: null";
                }
                catch
                {
                    info += "\nGridManager: unavailable";
                }
            }
            else
            {
                info += "No target found (tag 'Player')";
            }

            if (infoText != null) infoText.text = info;
        }
    }

    void CreateUI()
    {
        GameObject canvasGO = new GameObject("DevMenuCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // bring to front
        canvasGO.AddComponent<CanvasScaler>();

        CanvasScaler canvasScaler = canvasGO.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1280, 720);
        canvasScaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();


        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        // set the panel to the center of the screen
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(520, 300);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 8;

        // Title
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panel.transform, false);
        titleText = titleGO.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = Color.cyan;
        titleText.text = "DEVELOPER MENU";
        var titleRt = titleText.GetComponent<RectTransform>();
        titleRt.sizeDelta = new Vector2(480, 30);

        // FPS 
        GameObject textGO = new GameObject("FPSText");
        textGO.transform.SetParent(panel.transform, false);
        fpsText = textGO.AddComponent<Text>();
        fpsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fpsText.fontSize = 16;
        fpsText.alignment = TextAnchor.UpperLeft;
        fpsText.color = Color.white;
        fpsText.text = "FPS: -";
        var fpsRt = fpsText.GetComponent<RectTransform>();
        fpsRt.sizeDelta = new Vector2(480, 24);

        // Timer
        GameObject timerGO = new GameObject("TimerText");
        timerGO.transform.SetParent(panel.transform, false);
        timerText = timerGO.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = 14;
        timerText.alignment = TextAnchor.UpperLeft;
        timerText.color = Color.white;
        timerText.text = "Game Time: 00:00:00\nStopwatch: 00:00:00 (Stopped)";
        var timerRt = timerText.GetComponent<RectTransform>();
        timerRt.sizeDelta = new Vector2(480, 44);

        // Info
        GameObject infoGO = new GameObject("InfoText");
        infoGO.transform.SetParent(panel.transform, false);
        infoText = infoGO.AddComponent<Text>();
        infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        infoText.fontSize = 14;
        infoText.alignment = TextAnchor.UpperLeft;
        infoText.color = Color.white;
        infoText.text = "";
        var infoRt = infoText.GetComponent<RectTransform>();
        infoRt.sizeDelta = new Vector2(480, 120);

        // Buttons
        GameObject buttonsGO = new GameObject("Buttons");
        buttonsGO.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup hLayout = buttonsGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 8;

        pauseButton = CreateButton("Pause", TogglePause);
        pauseButton.transform.SetParent(buttonsGO.transform, false);

        stopwatchButton = CreateButton("Start Stopwatch", ToggleStopwatch);
        stopwatchButton.transform.SetParent(buttonsGO.transform, false);

        stopwatchResetButton = CreateButton("Reset Stopwatch", ResetStopwatch);
        stopwatchResetButton.transform.SetParent(buttonsGO.transform, false);

        reloadButton = CreateButton("Reload", ReloadScene);
        reloadButton.transform.SetParent(buttonsGO.transform, false);

        // Hint
        GameObject hintGO = new GameObject("Hints");
        hintGO.transform.SetParent(panel.transform, false);
        Text hintText = hintGO.AddComponent<Text>();
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 14;
        hintText.alignment = TextAnchor.UpperLeft;
        hintText.color = Color.white;
        hintText.text = "Hold `~` to show. Space: Pause, R: Reload, T: Start/Stop Stopwatch, Y: Reset Stopwatch";
        var hintRect = hintText.GetComponent<RectTransform>();
        hintRect.sizeDelta = new Vector2(480, 22);
    }

    private Button CreateButton(string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGameObject = new GameObject("Button_" + label);
        Button button = buttonGameObject.AddComponent<Button>();
        Image img = buttonGameObject.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.12f);
        RectTransform rectTransform = buttonGameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(140, 38);

        GameObject txtGameObject = new GameObject("Text");
        txtGameObject.transform.SetParent(buttonGameObject.transform, false);
        Text text = txtGameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 15;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        RectTransform textRect = txtGameObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        button.onClick.AddListener(action);
        return button;
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void ToggleStopwatch()
    {
        stopwatchRunning = !stopwatchRunning;
        UpdateStopwatchButtonText();
    }

    private void ResetStopwatch()
    {
        stopwatchRunning = false;
        stopwatchTime = 0f;
        UpdateStopwatchButtonText();
    }

    private void UpdateStopwatchButtonText()
    {
        if (stopwatchButton != null)
        {
            var txt = stopwatchButton.GetComponentInChildren<Text>();
            if (txt != null) txt.text = stopwatchRunning ? "Stop Stopwatch" : "Start Stopwatch";
        }
    }

    private string FormatTime(float seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // public API for developer menu
    public void StartStopwatch() { stopwatchRunning = true; UpdateStopwatchButtonText(); }
    public void StopStopwatch() { stopwatchRunning = false; UpdateStopwatchButtonText(); }
    public void ResetStopwatchPublic() { ResetStopwatch(); }
}
