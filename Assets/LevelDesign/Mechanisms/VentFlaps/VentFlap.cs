using Magnet;
using UnityEngine;

public class VentFlap : MonoBehaviour, IInterfaceEvent
{
    Rigidbody2D rb;
    HingeJoint2D hingeJoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hingeJoint = GetComponent<HingeJoint2D>();

        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        rb.Sleep();
    }

    public void InterfaceEvent(string eventName)
    {
        switch(eventName)
        {
            case "Open":
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints2D.None;
                    rb.WakeUp();
                }
                break;
            default:
                break;
        }
    }
}
