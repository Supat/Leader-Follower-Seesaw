using UnityEngine;

[CreateAssetMenu(fileName = "ExperimentConfig", menuName = "Seesaw/Experiment Config", order = 0)]
public class ExperimentConfig : ScriptableObject
{
    [Header("Trial Structure")]
    [Tooltip("Number of trials per block. After this many trials, LeadingPlayerID flips and a new block begins.")]
    public int numberOfTrials = 5;

    [Header("Target Placement")]
    [Tooltip("Inclusive lower bound for the random integer Y shift applied to targets each trial.")]
    public int targetShiftMin = -3;

    [Tooltip("Exclusive upper bound for the random integer Y shift applied to targets each trial.")]
    public int targetShiftMax = 4;

    [Tooltip("Minimum vertical distance between a target and a player. Candidates closer than this are rejected.")]
    public float minTargetPlayerDistance = 1.0f;

    [Tooltip("Max retries in MoveTargets before giving up and accepting the last candidate. A warning is logged on cap-hit.")]
    public int placementMaxAttempts = 20;

    [Header("Player Perturbation")]
    [Tooltip("Lowest random multiplier applied to player movement at the start of each trial.")]
    public float perturbationMin = 0.5f;

    [Tooltip("Highest random multiplier applied to player movement at the start of each trial.")]
    public float perturbationMax = 2.0f;
}
