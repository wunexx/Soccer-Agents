using UnityEngine;

public class KickTrigger : MonoBehaviour
{
    [HideInInspector] public bool canKick = false;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            canKick = true;
            spriteRenderer.color = new Color(0, 255, 0, 0.3f);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            canKick = false;
            spriteRenderer.color = new Color(255, 0, 0, 0.3f);
        }
    }
}
