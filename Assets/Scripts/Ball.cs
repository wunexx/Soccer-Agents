using UnityEngine;

public class Ball : MonoBehaviour
{
    EnvironmentManager environmentManager;

    private void Start()
    {
        environmentManager = GetComponentInParent<EnvironmentManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("RedGoal"))
        {
            environmentManager.OnGoalScored(Team.Green);
        }
        else if (collision.gameObject.CompareTag("GreenGoal"))
        {
            environmentManager.OnGoalScored(Team.Red);
        }
    }
}
