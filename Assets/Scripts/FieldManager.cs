using TMPro;
using UnityEngine;
public enum Team
{
    Red,
    Green
}
public class FieldManager : MonoBehaviour
{
    [SerializeField] int[] teamScores = new int[2];

    [SerializeField] SoccerAgent[] agents = new SoccerAgent[2];
    [SerializeField] Transform ball;

    [SerializeField] TextMeshProUGUI scoresText;
    [SerializeField] TextMeshProUGUI episodeText;
    public Transform statsParent;

    Rigidbody2D ballRb;

    int episodeCounter;
    private void Start()
    {
        ballRb = ball.GetComponent<Rigidbody2D>();

        UpdateTexts();
    }

    public void ResetField()
    {
        ball.localPosition = Vector2.zero;
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
    }

    public void OnGoalScored(Team scoringTeam)
    {
        int winnerIndex = scoringTeam == Team.Red ? 0 : 1;
        int loserIndex = 1 - winnerIndex;

        teamScores[winnerIndex]++;
        agents[winnerIndex].OnGoalScored(true);
        agents[loserIndex].OnGoalScored(false);

        episodeCounter++;
        UpdateTexts();
    }       

    void UpdateTexts()
    {
        scoresText.text = $"{teamScores[0]} | {teamScores[1]}";
        episodeText.text = $"Episode: {episodeCounter}";
    }
}
