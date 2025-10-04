using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections;
using TMPro;

public class SoccerAgent : Agent
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 180f;

    [Header("Kick")]
    [SerializeField] float kickForce;

    [Header("Field")]
    [SerializeField] float fieldWidth = 20f;
    [SerializeField] float fieldHeight = 10f;

    [Header("Other")]
    [SerializeField] Team team;
    [SerializeField] Transform opponent;
    [SerializeField] Transform opponentsGoal;
    [SerializeField] KickTrigger kickTrigger;
    [SerializeField] GameObject statsPrefab;

    TextMeshProUGUI labelText;
    TextMeshProUGUI rewardText;

    Rigidbody2D ball;
    FieldManager fieldManager;
    [HideInInspector] public Rigidbody2D rb;

    float maxReward;

    float previousBallDistance;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!rb) Debug.LogWarning("No RigidBody2D Found!!!");

        ball = transform.parent.Find("Ball").GetComponent<Rigidbody2D>();
        if (!ball) Debug.LogWarning("No Ball Found!!!");

        fieldManager = transform.parent.GetComponent<FieldManager>();
        if (!fieldManager) Debug.LogWarning("No Field Manager Found!!!");

        GameObject statsObj = Instantiate(statsPrefab, Vector2.zero, Quaternion.identity, fieldManager.statsParent);
        labelText = statsObj.transform.Find("LabelText").GetComponent<TextMeshProUGUI>();
        rewardText = statsObj.transform.Find("RewardText").GetComponent<TextMeshProUGUI>();
    }
    public override void OnEpisodeBegin()
    {
        fieldManager.ResetField();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.rotation = 0f;

        float posOffset = team == Team.Red ? -10 : 10;
        transform.localPosition = new Vector2(posOffset, 0f);

        Vector2 toCenter = (Vector2)Vector2.zero - (Vector2)transform.localPosition;
        transform.up = toCenter.normalized;


        previousBallDistance = Vector2.Distance(ball.position, opponentsGoal.position);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x / (fieldWidth / 2f));
        sensor.AddObservation(transform.localPosition.y / (fieldHeight / 2f));

        sensor.AddObservation(ball.transform.localPosition.x / (fieldWidth / 2f));
        sensor.AddObservation(ball.transform.localPosition.y / (fieldHeight / 2f));

        sensor.AddObservation(opponent.localPosition.x / (fieldWidth / 2f));
        sensor.AddObservation(opponent.localPosition.y / (fieldHeight / 2f));

        sensor.AddObservation(transform.up);

        sensor.AddObservation(ball.linearVelocity / kickForce);
        sensor.AddObservation(rb.linearVelocity / moveSpeed);

        Rigidbody2D oppRb = opponent.GetComponent<Rigidbody2D>();
        sensor.AddObservation(oppRb ? oppRb.linearVelocity / moveSpeed : Vector2.zero);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;

        Vector2 moveInput = PlayerController.Instance.GetMovementInput();

        if (moveInput.y > 0)
            discreteActionsOut[0] = 1;
        else if (moveInput.x < 0)
            discreteActionsOut[0] = 2;
        else if (moveInput.x > 0)
            discreteActionsOut[0] = 3;

        if (PlayerController.Instance.IsKickPressed())
            discreteActionsOut[1] = 1;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);

        float currentDistance = Vector2.Distance(ball.transform.localPosition, opponentsGoal.localPosition);
        float deltaDistance = previousBallDistance - currentDistance;
        previousBallDistance = currentDistance;
        AddReward(deltaDistance * 0.01f);

        AddReward(-0.001f);

        UpdateStats();
    }

    void MoveAgent(ActionSegment<int> actions)
    {
        var act = actions[0];

        float rotation;
        switch(act)
        {
            case 1:
                rb.MovePosition(rb.position + (Vector2)transform.up * moveSpeed * Time.deltaTime);
                break;
            case 2:
                rotation = rb.rotation + rotationSpeed * Time.deltaTime;
                rb.MoveRotation(rotation);
                break;
            case 3:
                rotation = rb.rotation - rotationSpeed * Time.deltaTime;
                rb.MoveRotation(rotation);
                break;
        }

        var kickAct = actions[1];
        if (kickAct == 1)
            TryKickBall();
    }

    void TryKickBall()
    {
        //Debug.Log("Trying to kick the ball....");
        if (kickTrigger.canKick)
        {
            Vector2 direction = ball.transform.localPosition - transform.localPosition;
            ball.AddForce(direction.normalized * kickForce, ForceMode2D.Impulse);
            //Debug.Log($"Success! Force: {direction.normalized * kickForce}");
        }
    }

    void UpdateStats()
    {
        float reward = GetCumulativeReward();

        if (reward > maxReward) maxReward = reward;

        Color textColor = team == Team.Red ? Color.red : Color.green;

        labelText.text = $"{gameObject.name} ({team} Team)";
        labelText.color = textColor;

        rewardText.text = $"Reward: {reward} | Max Reward: {maxReward} | Current Step: {StepCount}";
    }

    public void OnGoalScored(bool isWinner)
    {
        AddReward(isWinner ? 1f : -1f);
        //Debug.Log($"Scored a goal! Is winner: {isWinner}! Name: {gameObject.name}");

        StartCoroutine(EndEpisodeNextFrame());
    }

    private IEnumerator EndEpisodeNextFrame()
    {
        yield return null;
        EndEpisode();
    }
}
