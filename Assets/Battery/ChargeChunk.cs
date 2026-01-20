using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class ChargeChunk : MonoBehaviour
{
    [SerializeField] private byte chargeAmount = 50;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Battery>() != null)
        {
            Collider2D col = GetComponent<Collider2D>();
            col.enabled = false;

            collision.gameObject.GetComponent<Battery>().AddPercent(chargeAmount);
            StartCoroutine(DestroyChunk());
        }
    }

    private IEnumerator DestroyChunk()
    {
        yield return new WaitForSeconds(.1f);
        Destroy(this.gameObject);
    }
}
