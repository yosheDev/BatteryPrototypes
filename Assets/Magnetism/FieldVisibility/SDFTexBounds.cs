using UnityEngine;

public class SDFTexBounds : MonoBehaviour
{
    private MagneticDistanceFieldsManager sdfManager;
    [HideInInspector]public Bounds boundsWS;

    void Awake()
    {
        // Get bounds before disabling the rendering.
        this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        //this.gameObject.GetComponent<SpriteRenderer>().color = new Color (1f, 1f, 1f, 0f);
        
        boundsWS = GetComponent<SpriteRenderer>().bounds;
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;

        sdfManager = FindFirstObjectByType<MagneticDistanceFieldsManager>(); /// Only ever one of these at a time.
        if (sdfManager != null)
        {
            sdfManager.AddTexBounds(this);
        }
    }
}
