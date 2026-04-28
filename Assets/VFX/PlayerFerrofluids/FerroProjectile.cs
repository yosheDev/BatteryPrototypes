using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FerroProjectile : MonoBehaviour
{
    private bool inPlay = false;
    private float speedMult = 1f;

    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private Light2D light;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem shootParticles;

    public void Shoot(Vector3 pos, Quaternion rot, Vector3 scale, float speed = 1f)
    {
        transform.position = pos;
        transform.rotation = rot;
        transform.localScale = scale;

        speedMult = speed;
        shootParticles.Play();
        SetState(true);
    }

    void FixedUpdate()
    {
        if (inPlay)
        {
            rb.position = rb.position + (Vector2)transform.up * 50f * Time.fixedDeltaTime * speedMult;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (inPlay)
        {
            //Debug.Log("Collided with " + collision.gameObject);
            SetState(false);
        }
    }

    public bool IsInPlay()
    {
        return inPlay;
    }

    private void SetState(bool active)
    {
        rb.simulated = active;
        inPlay = active;
        renderer.enabled = active;
        light.enabled = active;
    }
}
