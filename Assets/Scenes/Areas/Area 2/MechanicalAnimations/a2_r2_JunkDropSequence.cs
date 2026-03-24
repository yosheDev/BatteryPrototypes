using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class a2_r2_JunkDropSequence : MonoBehaviour
{
    private GameObject playerObj;
    [SerializeField] private GameObject clawObj;
    [SerializeField] private Vector2 clawXRange;
    [SerializeField] private GameObject barrierObj;
    [SerializeField] private Animator clawAnimator;
    [SerializeField] private List<float> dropPoints;
    [SerializeField] private List<GameObject> scrapPrefabs;
    [SerializeField] private Transform scrapTransform;
    GameObject scrapObj;

    private int dropIndex = 0;

    public delegate void OnDescentFinished();
    public event OnDescentFinished onDescentFinished;

    public delegate void OnDrop();
    public event OnDrop onDrop;

    private Coroutine trailRoutine;
    private Coroutine ascendDescendRoutine;

    private void OnDisable()
    {
        onDescentFinished -= DescentFinished;
        onDrop -= OnDropEvent;
    }
    private void Start()
    {
        playerObj = GameObject.FindAnyObjectByType<BatteryController>().gameObject;
        barrierObj.SetActive(false);
        onDrop += OnDropEvent;

        StartCoroutine(BeginSequence());
    }

    public IEnumerator BeginSequence()
    {
        // Wait until player is within world bounds for triggering.
        while (true)
        {
            if (Mathf.Abs(playerObj.transform.position.x - 16) < .1f)
            {
                break;
            }

            yield return null;
        }

        // Close door to block player in. Only do this once, make this not a part of the sequence looping.
        barrierObj.SetActive(true);

        // Show claw and junk arrive.
        onDescentFinished += DescentFinished;

        // Create scrap
        scrapObj = GameObject.Instantiate(scrapPrefabs[dropIndex], scrapTransform);
        scrapObj.GetComponent<Rigidbody2D>().gravityScale = 0f;
        scrapObj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        ascendDescendRoutine = StartCoroutine(Descend(false, 1f));
        yield break;
    }

    public IEnumerator RestartSequence()
    {
        // Show claw and junk arrive.
        onDescentFinished += DescentFinished;

        // Create scrap
        scrapObj = GameObject.Instantiate(scrapPrefabs[dropIndex], scrapTransform);
        scrapObj.GetComponent<Rigidbody2D>().gravityScale = 0f;
        scrapObj.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        ascendDescendRoutine = StartCoroutine(Descend(false, 1f));
        yield break;
    }

    public void DescentFinished()
    {
        ascendDescendRoutine = null;
        StartCoroutine(SequenceResumeAfterDescent());
    }
    public IEnumerator SequenceResumeAfterDescent()
    {
        onDescentFinished -= DescentFinished;

        // Trail the player
        trailRoutine = StartCoroutine(TrailPlayerX());
        yield return new WaitForSeconds(3f);

        // Stop trailing the player
        StopCoroutine(trailRoutine);
        yield return new WaitForSeconds(1f);

        // Move to the actual drop location.
        trailRoutine = StartCoroutine(MoveToDropPoint(dropPoints[dropIndex]));
        yield break;
    }
    public void OnDropEvent()
    {
        StartCoroutine(SequenceResumeAfterDrop());
    }
    public IEnumerator SequenceResumeAfterDrop()
    {
        // Delay, and then do it again.
        yield return new WaitForSeconds(1f);

        // Ascend back up.
        onDescentFinished += AscentFinished;
        ascendDescendRoutine = StartCoroutine(Descend(true, 1f));
    }
    public void AscentFinished()
    {
        StartCoroutine(SequenceResumeAtEnd());
        onDescentFinished -= AscentFinished;
    }
    public IEnumerator SequenceResumeAtEnd()
    {
        yield return new WaitForSeconds(1f);

        // If there is another drop point, repeat the sequence.
        dropIndex++;
        if (dropIndex >= dropPoints.Count)
        {
            //Debug.Log("Sequence is over.");
        }
        else
        {
            clawAnimator.SetTrigger("Reset");
            StartCoroutine(RestartSequence());
        }

        yield break;
    }
    private IEnumerator TrailPlayerX()
    {
        Vector2 curVelocity = Vector2.zero;
        while (true)
        {
            clawObj.transform.position = Vector2.SmoothDamp((Vector2)clawObj.transform.position, new Vector2(playerObj.transform.position.x, clawObj.transform.position.y), ref curVelocity, .5f);
            clawObj.transform.position = new Vector2(Mathf.Clamp(clawObj.transform.position.x, clawXRange.x, clawXRange.y), clawObj.transform.position.y);
            yield return null;
        }
    }

    private IEnumerator Descend(bool ascend = false, float duration = 1f)
    {
        float descendDistance = 12f * (ascend ? -1f : 1f);
        Vector2 target = (Vector2)clawObj.transform.position - new Vector2(0f, descendDistance);
        Vector2 curVelocity = Vector2.zero;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            //Debug.Log("Descending/Ascending");
            clawObj.transform.position = Vector2.SmoothDamp((Vector2)clawObj.transform.position, target, ref curVelocity, .5f);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        //Debug.Log("Descend/Ascend finished");
        onDescentFinished?.Invoke();
    }

    private IEnumerator MoveToDropPoint(float dropPoint)
    {
        //Vector2 target = new Vector2(dropPoint, clawObj.transform.position.y);
        Vector2 target = new Vector2(playerObj.transform.position.x, clawObj.transform.position.y);
        Vector2 curVelocity = Vector2.zero;

        while (Mathf.Abs(clawObj.transform.position.x - Mathf.Clamp(target.x, clawXRange.x, clawXRange.y)) > .1f)
        {
            //Debug.Log("Moving to drop point");
            clawObj.transform.position = Vector2.SmoothDamp((Vector2)clawObj.transform.position, target, ref curVelocity, 1f);
            clawObj.transform.position = new Vector2(Mathf.Clamp(clawObj.transform.position.x, clawXRange.x, clawXRange.y), clawObj.transform.position.y);
            yield return new WaitForFixedUpdate();
        }

        //Debug.Log("At drop point.");
        yield return new WaitForSeconds(.5f);
        //Debug.Log("Dropping Junk");
        DropScrap();

        onDrop?.Invoke();
    }

    public void DropScrap()
    {
        if (scrapObj != null)
        {
            scrapObj.transform.SetParent(null);
            scrapObj.GetComponent<Rigidbody2D>().gravityScale = 1f;
            //scrapObj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            scrapObj.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

            scrapObj = null;

            clawAnimator.SetTrigger("Drop");
        }
    }
}
