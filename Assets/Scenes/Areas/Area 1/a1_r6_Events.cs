using UnityEngine;

public class a1_r6_Events : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (AreaManager.instance.checkpointRespawnCount > 0)
        {
            Debug.Log("Room is blasted open!");
        }
        else
        {
            Debug.Log("This is the first time being in the room.");
        }
    }
}
