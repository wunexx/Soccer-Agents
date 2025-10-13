using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public enum Team
{
    Red = 0, Green = 1
}


[RequireComponent(typeof(Rigidbody2D))]
public class SoccerAgent : Agent
{
    [Header("-------Movement-------")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float rotationSpeed = 200f;

    [Header("-------Kick-------")]
    [SerializeField] float kickForce = 10f;
    [SerializeField] float kickCooldown = 1f;

    [Header("-------Team-------")]
    [SerializeField] Team team;

    [Header("-------Team Data-------")]
    [SerializeField] string[] teamLayers;
    [SerializeField] Sprite[] teamSprites;
    [SerializeField] Sprite[] teamCircleSprites;

    [Header("-------Rewards-------")]

    [Header("Multipliers")]
    [SerializeField] float ballDistanceMultiplierReward = 0.01f;
    [SerializeField] float movingTowardsBallMultiplierReward = 0.01f;

    [Header("Kick")]
    [SerializeField] float kickReward = 5f;
    [SerializeField] float falseKickReward = -1f; //kick with no ball

    [Header("Time")]
    [SerializeField] float idleReward = -0.01f;
    [SerializeField] float timeoutReward = -10f;

    [Header("Win/Lose")]
    [SerializeField] float loseReward = -5f;
    [SerializeField] float winReward = 10f;

    [Header("-------References-------")]
    [SerializeField] GameObject statsPrefab;

    [SerializeField] RayPerceptionSensorComponent2D rayPerceptionSensorComponent2D;
    [SerializeField] BehaviorParameters behaviorParameters;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer circleSpriteRenderer;
 
    [SerializeField] GameObject kickTriggerObject;

    Transform enemyGoal;

    TextMeshProUGUI nameText;
    TextMeshProUGUI statsText;

    EnvironmentManager environmentManager;
    Rigidbody2D rb;
    KickTrigger kickTrigger;
    GameObject[] enemies;

    float previousBallToGoalDistance;

    float kickTimer = 0;

    Vector2 initialPos;
    float initalRotation;

    float episodeMaxReward = 0;
    float allTimeMaxReward = 0;

    int episodeCounter;

    public override void Initialize()
    {
        environmentManager = GetComponentInParent<EnvironmentManager>();
        rb = GetComponent<Rigidbody2D>();
        kickTrigger = transform.GetComponentInChildren<KickTrigger>();
        enemyGoal = environmentManager.GetGoal(team);

        initialPos = transform.localPosition;
        initalRotation = rb.rotation;

        enemies = environmentManager.GetEnemies(team);

        int observationVectorSize = (enemies.Length * 6) + 12;
        behaviorParameters.BrainParameters.VectorObservationSize = observationVectorSize;

        GameObject statsObj = Instantiate(statsPrefab, Vector2.zero, Quaternion.identity, environmentManager.GetStatsParent());

        nameText = statsObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        statsText = statsObj.transform.Find("StatsText").GetComponent<TextMeshProUGUI>();
    }
    public override void OnEpisodeBegin()
    {
        episodeCounter++;
        environmentManager.UpdateUI(episodeCounter);
        episodeMaxReward = 0;
        ResetKickCooldown();

        environmentManager.ResetBall();

        transform.localPosition = initialPos;
        rb.MoveRotation(initalRotation);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;

        previousBallToGoalDistance = Vector2.Distance(environmentManager.GetBallRB().position, enemyGoal.position);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        float invWidth = 1f / Mathf.Max(0.0001f, environmentManager.fieldHalfWidth);
        float invHeight = 1f / Mathf.Max(0.0001f, environmentManager.fieldHalfHeight);
        float invAgentSpeed = 1f / Mathf.Max(0.0001f, moveSpeed);
        float invBallSpeed = 1f / Mathf.Max(0.0001f, kickForce);


        sensor.AddObservation(new Vector2(
            rb.linearVelocity.x * invAgentSpeed,
            rb.linearVelocity.y * invAgentSpeed));

        float normalizedRot = Mathf.Sin(rb.rotation * Mathf.Deg2Rad);
        sensor.AddObservation(normalizedRot);

        float normalizedKickTimer = Mathf.Clamp01(kickTimer / Mathf.Max(0.0001f, kickCooldown));
        sensor.AddObservation(normalizedKickTimer);

        //sensor.AddObservation(kickCooldown);

        Vector2 ballRelPos = environmentManager.GetBallRB().position - (Vector2)transform.position;
        Vector2 ballRelPosNorm = new Vector2(ballRelPos.x * invWidth, ballRelPos.y * invHeight);
        sensor.AddObservation(ballRelPosNorm); // 2

        Vector2 ballVel = environmentManager.GetBallRB().linearVelocity;
        Vector2 ballVelNorm = new Vector2(ballVel.x * invBallSpeed, ballVel.y * invBallSpeed);
        sensor.AddObservation(ballVelNorm); // 2

        Vector2 goalRel = (Vector2)enemyGoal.position - (Vector2)transform.position;
        Vector2 goalRelNorm = new Vector2(goalRel.x * invWidth, goalRel.y * invHeight);
        sensor.AddObservation(goalRelNorm); // 2

        float angleToGoal = Vector2.SignedAngle(transform.up, goalRel) / 180f;
        sensor.AddObservation(angleToGoal);

        sensor.AddObservation(kickTrigger.HasBall());

        foreach (var enemy in enemies)
        {
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            SoccerAgent enemySoccerAgent = enemy.GetComponent<SoccerAgent>();

            sensor.AddObservation(new Vector2(
               enemyRb.linearVelocity.x * invAgentSpeed,
               enemyRb.linearVelocity.y * invAgentSpeed));

            float normalizedEnemyRot = Mathf.Sin(enemyRb.rotation * Mathf.Deg2Rad);
            sensor.AddObservation(normalizedEnemyRot);

            Vector2 enemyRel = (Vector2)enemyRb.position - (Vector2)transform.position;
            Vector2 enemyRelNorm = new Vector2(enemyRel.x * invWidth, enemyRel.y * invHeight);
            sensor.AddObservation(enemyRelNorm); // 2

            float normalizedEnemyKickTimer = Mathf.Clamp01(enemySoccerAgent.kickTimer / Mathf.Max(0.0001f, enemySoccerAgent.kickCooldown));
            sensor.AddObservation(normalizedEnemyKickTimer);

            //sensor.AddObservation(enemySoccerAgent.kickCooldown);
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        var moveInput = PlayerController.Instance.GetMovementInput();

        bool kickInput = PlayerController.Instance.IsKickPressed();

        discreteActions[0] = 0;

        continuousActions[0] = moveInput.y;
        continuousActions[1] = -moveInput.x;

        if (kickInput)
            discreteActions[0] = 1;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        kickTimer += Time.fixedDeltaTime;
        AddReward(idleReward);

        float moveInput = actions.ContinuousActions[0];
        float turnInput = actions.ContinuousActions[1];

        MoveAgent(moveInput, turnInput);

        int kick = actions.DiscreteActions[0];
        if (kick == 1)
        {
            bool kicked = kickTrigger.TryKick();

            AddReward(kicked ? kickReward : falseKickReward);
        }

        float distance = Vector2.Distance(environmentManager.GetBallRB().position, enemyGoal.position);
        float distanceDelta = previousBallToGoalDistance - distance;
        AddReward(ballDistanceMultiplierReward * distanceDelta);
        previousBallToGoalDistance = distance;

        Vector2 ballDir = (environmentManager.GetBallRB().position - (Vector2)transform.position).normalized;
        float forwardDot = Vector2.Dot(transform.up, ballDir);
        AddReward(movingTowardsBallMultiplierReward * forwardDot);

        if (StepCount >= MaxStep)
        {
            AddReward(timeoutReward);
            EndEpisode();
        }

        UpdateUI();
    }

    public bool CanKick() { return kickTimer >= kickCooldown; }

    public void ResetKickCooldown() => kickTimer = 0;

    void MoveAgent(float moveInput, float turnInput)
    {
        rb.linearVelocity = transform.up * moveInput * moveSpeed;
        rb.MoveRotation(rb.rotation + turnInput * rotationSpeed * Time.fixedDeltaTime);
    }
    public float GetKickForce() { return kickForce; }
    public void OnWin() => AddReward(winReward);
    public void OnLose() => AddReward(loseReward);

    void UpdateUI()
    {
        float reward = GetCumulativeReward();

        if (reward > allTimeMaxReward) allTimeMaxReward = reward;
        if (reward > episodeMaxReward) episodeMaxReward = reward;

        Color textColor = (int)team == 0 ? Color.red : Color.green;

        nameText.color = textColor;
        nameText.text = $"{gameObject.name} ({team} Team)";

        statsText.text = $"Reward: {reward} \nEp. Max Reward: {episodeMaxReward} \nAll Time Max Reward: {allTimeMaxReward} \nStep: {StepCount}";
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyTeamSettings(team);
    }
#endif
    void ApplyTeamSettings(Team team)
    {
        if (!rayPerceptionSensorComponent2D || !spriteRenderer || !behaviorParameters) return;

        behaviorParameters.TeamId = (int)team;

        if (teamSprites != null && (int)team < teamSprites.Length)
            spriteRenderer.sprite = teamSprites[(int)team];

        if (teamCircleSprites != null && (int)team < teamCircleSprites.Length)
            circleSpriteRenderer.sprite = teamCircleSprites[(int)team];

        if (teamLayers != null && (int)team < teamLayers.Length)
        {
            int excludeLayer = LayerMask.NameToLayer(teamLayers[(int)team]);

            gameObject.layer = excludeLayer;
            kickTriggerObject.layer = excludeLayer;

            int newMask = Physics2D.AllLayers & ~(1 << excludeLayer);

            rayPerceptionSensorComponent2D.RayLayerMask = newMask;
        }
    }
}
