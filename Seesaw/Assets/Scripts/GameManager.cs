using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using SeesawHelper;

public abstract class GameManager : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public GameObject target1;
    public GameObject target2;
    public GameObject ball;
    public Canvas messageCanvas;
    public TextMeshProUGUI messageTMPText;
    public Camera gameCamera;
    public int playerID = 0;

    [Tooltip("ScriptableObject holding all experiment tunables. Required — assign an ExperimentConfig asset.")]
    public ExperimentConfig config;

    protected float targetPositionShift = 5.0f;
    public float TargetPositionShift
    {
        get { return targetPositionShift; }
        set { targetPositionShift = value; }
    }

    protected Vector3 player1OriginalPos;
    protected Vector3 player2OriginalPos;
    protected Vector3 ballOriginalPos;

    protected int leadingPlayerID = 1;
    public int LeadingPlayerID
    {
        get { return leadingPlayerID; }
        set { leadingPlayerID = value; }
    }

    protected FixedSizedQueue<int> leadingPlayerHistory = new();
    protected int trialCount = 0;
    protected int blockCount = 0;
    protected int playScore = 0;
    public int PlayScore
    {
        get { return playScore; }
        set { playScore = value; }
    }

    protected int pauseState = 0;
    public int PauseState
    {
        get { return pauseState; }
    }

    private bool trialResolved = false;

    // Cached component references
    protected BallBehavior ballBehavior;
    protected Rigidbody ballRigidbody;
    protected TargetBehavior target1Behavior;
    protected TargetBehavior target2Behavior;
    protected BoxCollider target1Collider;
    protected BoxCollider target2Collider;
    protected MeshRenderer target1Renderer;
    protected MeshRenderer target2Renderer;
    protected PlayerController player1Controller;
    protected PlayerController player2Controller;

    protected virtual void Awake()
    {
        leadingPlayerHistory.Limit = 2;
    }

    protected virtual void Start()
    {
        if (config == null)
        {
            Debug.LogError($"{GetType().Name}: ExperimentConfig is not assigned. Create one via Assets → Create → Seesaw → Experiment Config and assign it in the Inspector.", this);
            enabled = false;
            return;
        }

        CacheComponents();
        player1OriginalPos = player1.transform.position;
        player2OriginalPos = player2.transform.position;
        ballOriginalPos = ball.transform.position;

        messageCanvas.enabled = false;
        leadingPlayerHistory.Enqueue(leadingPlayerID);
        InitializeStage();
    }

    protected void CacheComponents()
    {
        ballBehavior = ball.GetComponent<BallBehavior>();
        ballRigidbody = ball.GetComponent<Rigidbody>();
        target1Behavior = target1.GetComponent<TargetBehavior>();
        target2Behavior = target2.GetComponent<TargetBehavior>();
        target1Collider = target1.GetComponent<BoxCollider>();
        target2Collider = target2.GetComponent<BoxCollider>();
        target1Renderer = target1.GetComponent<MeshRenderer>();
        target2Renderer = target2.GetComponent<MeshRenderer>();
        player1Controller = player1.GetComponent<PlayerController>();
        player2Controller = player2.GetComponent<PlayerController>();
    }

    protected virtual void Update()
    {
        HandleUnpauseInput();
        CheckMissionOutcome();
        CheckBlockAdvance();

        if (Input.GetKey(KeyCode.Escape))
            SceneManager.LoadScene("MainMenuScene");
    }

    private void HandleUnpauseInput()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame)
        {
            UnpauseGame();
            return;
        }

        if (Input.GetKey(KeyCode.Space))
            UnpauseGame();
    }

    private void CheckMissionOutcome()
    {
        if (trialResolved)
            return;

        // Mission Success — takes priority if both outcomes trigger in the same frame
        if (target1Behavior.IsHit && target2Behavior.IsHit)
        {
            trialResolved = true;
            IncreaseScore();
            AdvanceTrial();
        }
        else if (ballBehavior.IsHit)
        {
            trialResolved = true;
            AdvanceTrial();
            InitializeStage();
            PauseGame();
        }
    }

    private void CheckBlockAdvance()
    {
        if (trialCount >= config.numberOfTrials)
            AdvanceBlock();
    }

    protected void InitializeStage()
    {
        trialResolved = false;

        DisableTargets();
        HideTargets();
        MoveTargets();

        ballBehavior.IsHit = false;

        player1.transform.position = player1OriginalPos;
        player2.transform.position = player2OriginalPos;
        ball.transform.position = ballOriginalPos;
        if (!ballRigidbody.isKinematic)
        {
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

        EnableTargets();
        CheckAndShowTarget();
    }

    protected void MoveTargets()
    {
        float previousShift = targetPositionShift;
        int attempts = 0;

        do
        {
            targetPositionShift = Random.Range(config.targetShiftMin, config.targetShiftMax);
            attempts++;
        }
        while ((targetPositionShift == previousShift
                || targetPositionShift == 0
                || WouldOverlapPlayer(targetPositionShift)
                || WouldBeOffScreen(targetPositionShift))
               && attempts < config.placementMaxAttempts);

        if (attempts >= config.placementMaxAttempts)
            Debug.LogWarning("MoveTargets: hit retry cap; using last candidate shift " + targetPositionShift);

        target1Behavior.IsHit = false;
        target2Behavior.IsHit = false;

        target1.transform.position = new Vector3(player1OriginalPos.x - 0.5f, player1OriginalPos.y + targetPositionShift, player1OriginalPos.z);
        target2.transform.position = new Vector3(player2OriginalPos.x + 0.5f, player2OriginalPos.y + targetPositionShift, player2OriginalPos.z);
    }

    private bool WouldOverlapPlayer(float candidateShift)
    {
        float target1Y = player1OriginalPos.y + candidateShift;
        float target2Y = player2OriginalPos.y + candidateShift;

        return Mathf.Abs(player1.transform.position.y - target1Y) < config.minTargetPlayerDistance
            || Mathf.Abs(player2.transform.position.y - target2Y) < config.minTargetPlayerDistance;
    }

    private bool WouldBeOffScreen(float candidateShift)
    {
        Camera cam = gameCamera != null ? gameCamera : Camera.main;
        if (cam == null)
            return false;

        Vector3 target1Pos = new Vector3(player1OriginalPos.x - 0.5f, player1OriginalPos.y + candidateShift, player1OriginalPos.z);
        Vector3 target2Pos = new Vector3(player2OriginalPos.x + 0.5f, player2OriginalPos.y + candidateShift, player2OriginalPos.z);

        return IsOffScreen(cam, target1Pos) || IsOffScreen(cam, target2Pos);
    }

    private static bool IsOffScreen(Camera cam, Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        return vp.z <= 0 || vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1;
    }

    protected void DisableTargets()
    {
        target1Collider.enabled = false;
        target2Collider.enabled = false;
    }

    protected void EnableTargets()
    {
        target1Collider.enabled = true;
        target2Collider.enabled = true;
    }

    protected void HideTargets()
    {
        target1Renderer.enabled = false;
        target2Renderer.enabled = false;
    }

    protected void AdvanceTrial()
    {
        trialResolved = false;

        DisableTargets();
        HideTargets();
        MoveTargets();

        player1Controller.Perturbation = Random.Range(config.perturbationMin, config.perturbationMax);
        player2Controller.Perturbation = Random.Range(config.perturbationMin, config.perturbationMax);

        trialCount++;

        EnableTargets();
        CheckAndShowTarget();
    }

    protected void AdvanceBlock()
    {
        DisableTargets();
        HideTargets();
        MoveTargets();

        leadingPlayerID *= -1;
        trialCount = 0;
        blockCount++;

        EnableTargets();
        CheckAndShowTarget();
    }

    protected void IncreaseScore()
    {
        playScore++;
    }

    public virtual void PauseGame()
    {
        pauseState = 1;
        player1Controller.IsFreeze = true;
        player2Controller.IsFreeze = true;

        ballRigidbody.useGravity = false;
        ballRigidbody.isKinematic = true;

        messageTMPText.text = "Press spacebar to continue...";
        messageCanvas.enabled = true;
    }

    public virtual void UnpauseGame()
    {
        pauseState = 0;
        messageCanvas.enabled = false;

        player1Controller.IsFreeze = false;
        player2Controller.IsFreeze = false;

        ballRigidbody.useGravity = true;
        ballRigidbody.isKinematic = false;
    }

    protected void CheckAndShowTarget()
    {
        if (leadingPlayerID == playerID)
        {
            if (playerID == 1)
                target1Renderer.enabled = true;
            else if (playerID == -1)
                target2Renderer.enabled = true;
        }
    }
}
