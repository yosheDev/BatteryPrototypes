using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class a2_r2_JunkDropSequence : MonoBehaviour
{
    private GameObject playerObj;
    [SerializeField] private GameObject clawObj;
    [SerializeField] private Animator clawAnimator;
    [SerializeField] private List<float> dropPoints;
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
        playerObj = GameObject.FindFirstObjectByType<BatteryController>().gameObject;

        onDrop += OnDropEvent;

        StartCoroutine(BeginSequence());
    }

    public IEnumerator BeginSequence()
    {
        // Wait until player is within world bounds for triggering.
        yield return new WaitForSeconds(2f);

        // Close door to block player in. Only do this once, make this not a part of the sequence looping.

        // Show claw and junk arrive.
        onDescentFinished += DescentFinished;
        ascendDescendRoutine = StartCoroutine(Descend(false, 1f));
        yield break;
    }

    public IEnumerator RestartSequence()
    {
        // Show claw and junk arrive.
        onDescentFinished += DescentFinished;
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
            Debug.Log("Sequence is over.");
        }
        else
        {
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
            yield return null;
        }
    }

    private IEnumerator Descend(bool ascend = false, float duration = 1f)
    {
        float descendDistance = 2f * (ascend ? -1f : 1f);
        Vector2 target = (Vector2)clawObj.transform.position - new Vector2(0f, descendDistance);
        Vector2 curVelocity = Vector2.zero;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            Debug.Log("Descending/Ascending");
            clawObj.transform.position = Vector2.SmoothDamp((Vector2)clawObj.transform.position, target, ref curVelocity, .5f);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("Descend/Ascend finished");
        onDescentFinished?.Invoke();
    }

    private IEnumerator MoveToDropPoint(float dropPoint)
    {
        Vector2 target = new Vector2(dropPoint, clawObj.transform.position.y);
        Vector2 curVelocity = Vector2.zero;

        while (Mathf.Abs(clawObj.transform.position.x - dropPoint) > .1f)
        {
            Debug.Log("Moving to drop point");
            clawObj.transform.position = Vector2.SmoothDamp((Vector2)clawObj.transform.position, target, ref curVelocity, 1f);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("At drop point.");
        yield return new WaitForSeconds(.5f);
        Debug.Log("Dropping Junk");

        onDrop?.Invoke();
    }
}
