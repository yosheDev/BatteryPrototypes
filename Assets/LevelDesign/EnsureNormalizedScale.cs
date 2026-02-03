using UnityEngine;

public class EnsureNormalizedScale : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

}
