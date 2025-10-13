using UnityEngine;

public class KickTrigger : MonoBehaviour
{
    [SerializeField] float kickPlaceholderOpacity = 0.3f;

    SpriteRenderer spriteRenderer;
    SoccerAgent soccerAgent;
    GameObject ball;
    private void Start()
    {
        soccerAgent = GetComponentInParent<SoccerAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public bool TryKick()
    {
        if (!soccerAgent.CanKick())
        {
            Debug.Log("Cooldown not ready");
            return false;
        }

        soccerAgent.ResetKickCooldown();

        if (!ball)
        {
            Debug.Log("No ball to kick");
            return false;
        }

        Debug.Log("Kicking!");

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();

        Vector2 direction = (ball.transform.position - transform.position).normalized;

        rb.AddForce(direction * soccerAgent.GetKickForce(), ForceMode2D.Impulse);

        return true;
    }

    //for agent obs
    public int HasBall() { return ball == null || !soccerAgent.CanKick() ? 0 : 1; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            ball = collision.gameObject;
            spriteRenderer.color = new Color(0, 1f, 0, kickPlaceholderOpacity);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            ball = null;
            spriteRenderer.color = new Color(1f, 0, 0, kickPlaceholderOpacity);
        }
    }
}
