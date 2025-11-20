using UnityEngine;

public class PlayerPivotLocation : MonoBehaviour
{
    [SerializeField] GameObject player;
    void FixedUpdate()
    {
        transform.position = player.transform.position;
    }
}
