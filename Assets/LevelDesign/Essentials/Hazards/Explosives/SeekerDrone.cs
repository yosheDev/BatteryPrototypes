using FunctionLibrary;
using Magnet;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class SeekerDrone : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AIDestinationSetter destinationSetter;
    [SerializeField] private AIPath aiPath;

    [SerializeField] private bool setPlayerAsTargetImmediately = true;
    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (destinationSetter == null)
        {
            destinationSetter = GetComponent<AIDestinationSetter>();
        }

        destinationSetter.target = AreaManager.instance.playerController.transform;

        if (aiPath == null)
        {
            aiPath = GetComponent<AIPath>();
        }
    }

    private void FixedUpdate()
    {
        if (destinationSetter.target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, destinationSetter.target.position);

            aiPath.maxSpeed = FunctionLibraryF.MapRangeClamped(10f, 15f, 1f, 3f, distanceToTarget);
        }
    }
}
