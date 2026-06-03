using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(AimConstraint))]
public class AimAtPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ConstraintSource playerSource = new ConstraintSource();
        playerSource.sourceTransform = GameObject.FindAnyObjectByType<BatteryController>().transform;
        playerSource.weight = 1f;
        GetComponent<AimConstraint>().AddSource(playerSource);
    }
}
