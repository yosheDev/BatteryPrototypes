using UnityEngine;

public class Corrosion : MonoBehaviour
{

    //private bool isTrigger = true; /// Later on may be useful if adding non-trigger methods here. Should make delegate stuff for stuff.

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (!isTrigger)
        //{
            if (collision.gameObject.GetComponent<Battery>())
            {
                CorrodeTarget(collision.gameObject.GetComponent<Battery>());
            }
        //}
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (isTrigger)
        //{
            if (collision.gameObject.GetComponent<Battery>())
            {
                CorrodeTarget(collision.gameObject.GetComponent<Battery>());
            }
        //}
    }
    void CorrodeTarget(Battery battery)
    {
        battery.Corrode();
    }
}
