using UnityEngine;

public class RoboticArmTrack : RuntimeAnimator
{
    [SerializeField] private GameObject scrapPrefab;
    [SerializeField] private Transform scrapSpawnTransform;
    GameObject scrapObj;

    protected override void Start()
    {
        base.Start();   
    }

    public void OnAnimationStarted()
    {
        // TO DO: Use object pooling for the scraps if needed in future.

        scrapObj = GameObject.Instantiate(scrapPrefab, scrapSpawnTransform);
        scrapObj.GetComponent<Rigidbody2D>().gravityScale = 0f;
        scrapObj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    public void DropScrap()
    {
        if (scrapObj != null)
        {
            scrapObj.transform.SetParent(null);
            scrapObj.GetComponent<Rigidbody2D>().gravityScale = 1f;
            scrapObj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

            scrapObj = null;
        }
    }
}
