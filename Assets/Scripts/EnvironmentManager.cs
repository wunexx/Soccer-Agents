using TMPro;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("-------Agents-------")]
    [SerializeField] GameObject[] redTeamAgents;
    [SerializeField] GameObject[] greenTeamAgents;

    [Header("-------UI-------")]
    [SerializeField] Transform statsParent;
    [SerializeField] TextMeshProUGUI episodeText;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("-------Ball-------")]
    [SerializeField] Rigidbody2D ball;

    [Header("-------Goals-------")]
    [SerializeField] Transform[] teamGoals;

    [Header("---Field Measurements---")]
    public float fieldHalfWidth;
    public float fieldHalfHeight;

    int[] scores = new int[2];

    public void OnGoalScored(Team team)
    {
        scores[(int)team]++;

        GameObject[] winnerTeam = (int)team == 0 ? redTeamAgents : greenTeamAgents;

        ResetBall();

        ApplyRewards(winnerTeam, (int)team == 0 ? greenTeamAgents : redTeamAgents);

    }
     
    void ApplyRewards(GameObject[] winnerTeam, GameObject[] loserTeam)
    {
        foreach(var agent in winnerTeam)
        {
            SoccerAgent soccerAgent = agent.GetComponent<SoccerAgent>();

            soccerAgent.OnWin();
            soccerAgent.EndEpisode();
        }

        foreach(var agent in loserTeam)
        {
            SoccerAgent soccerAgent = agent.GetComponent<SoccerAgent>();

            soccerAgent.OnLose();
            soccerAgent.EndEpisode();
        }
    }

    public void ResetBall()
    {
        ball.linearVelocity = Vector2.zero;
        ball.angularVelocity = 0;
        ball.transform.localPosition = Vector2.zero;
        ball.MoveRotation(0);
    }

    public Transform GetStatsParent()
    {
        return statsParent;
    }

    public GameObject[] GetEnemies(Team team)
    {
        GameObject[] enemies = (int)team == 0 ? greenTeamAgents : redTeamAgents;

        return enemies;
    }
    
    public void UpdateUI(int episode)
    {
        scoreText.text = $"{scores[0]} | {scores[1]}";
        episodeText.text = $"Episode: {episode}";
    }

    public Rigidbody2D GetBallRB()
    {
        return ball;
    }

    public Transform GetGoal(Team team)
    {
        return (int)team == 0 ? teamGoals[1] : teamGoals[0];
    }
}