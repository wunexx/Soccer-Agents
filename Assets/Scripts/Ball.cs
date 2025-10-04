using UnityEngine;

public class Ball : MonoBehaviour
{
    FieldManager fieldManager;

    private void Start()
    {
        fieldManager = transform.parent.GetComponent<FieldManager>();

        if (!fieldManager) Debug.LogWarning("No Field Manager Found!!!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal 1"))
        {
            fieldManager.OnGoalScored(Team.Green);
        }
        else if (collision.CompareTag("Goal 2"))
        {
            fieldManager.OnGoalScored(Team.Red);
        }
    }
}
