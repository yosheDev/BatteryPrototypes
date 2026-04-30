using Magnet;
using Unity.VisualScripting;
using UnityEngine;

public class ContactBomb : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject explosionPrefab;
    public bool explodeOnAnyHighVelocityContact = false;
    [SerializeField] private float explodeVelocityThreshold = 10f;
    private float lastFrameVelocity = 0f;
    private Vector2 lastFramePos = Vector2.zero;

    [SerializeField] private Rigidbody2D rb;

    private void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void LateUpdate()
    {
        lastFramePos = transform.position;
        lastFrameVelocity = rb.linearVelocity.magnitude;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BatteryController>() != null || collision.gameObject.GetComponent<Corrosion>() != null)
        {
            Explode();
        }
        else
        {
            if (lastFrameVelocity >= explodeVelocityThreshold || ((Vector2)transform.position - lastFramePos).magnitude > explodeVelocityThreshold)
            {
                Explode();
            }
        }
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(this.gameObject);
    }
    
    #region IDamageable
    public void Damage(DamageTypes damageType)
    {
        Explode();
    }

    public bool IsAffectedByDamageType(DamageTypes damageType)
    {
        switch(damageType)
        {
            case DamageTypes.BluntForce:
                return true;
                break;
            case DamageTypes.Explosives:
                return true;
                break;
            case DamageTypes.Corrosion:
                return false;
                break;
            default:
                return false;
                break;
        }
    }
    #endregion
}
