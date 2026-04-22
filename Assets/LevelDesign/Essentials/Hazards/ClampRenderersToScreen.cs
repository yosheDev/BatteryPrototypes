using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using Magnet;

public class ClampRenderersToScreen : MonoBehaviour, IInterfaceEvent
{
    private enum ScreenClampMode
    {
        None,
        Top,
        Bottom,
        Vertical,
        Left,
        Right,
        Horizontal,
        All
    }

    [SerializeField] private List<Renderer> renderers;
    [SerializeField] private int largestSpriteIndex = 0;
    [SerializeField] private ScreenClampMode mode;
    [SerializeField] private Vector2 screenClampPadding = new Vector2();
    [SerializeField] private bool hideAtStart = true;
    private Coroutine flashRoutine;
    private Coroutine intervalRoutine;

    Vector3 initialPosition;
    //Vector2 screenBounds;
    Vector2 screenBoundsX;
    Vector2 screenBoundsY;
    float spriteWidth;
    float spriteHeight;

    private void Start()
    {
        initialPosition = transform.position;
        if (renderers.Count <= 0)
        {
            Debug.LogError("Renderers List in " + this + " on " + this.gameObject + " is empty.");
        }
        if (hideAtStart)
        {
            SetRendererVisibility(false);
        }
    }
    private void FixedUpdate()
    {
        // Get screen bounds based on camera
        Camera mainCamera = Camera.main;
        //screenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z - gameObject.transform.position.z)));
        screenBoundsX = new Vector2(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(mainCamera.transform.position.z - gameObject.transform.position.z))).x, mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z - gameObject.transform.position.z))).x);
        screenBoundsY = new Vector2(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(mainCamera.transform.position.z - gameObject.transform.position.z))).y, mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z - gameObject.transform.position.z))).y);
        //Debug.Log("Screen Bounds:" + screenBounds);
        // Get sprite size
        spriteWidth = renderers[largestSpriteIndex].bounds.size.x / 2;
        spriteHeight = renderers[largestSpriteIndex].bounds.size.y / 2;

        // Constain this position to specific sides of the screen.
        Vector2 newTrans = (Vector2)transform.position;

        if (mode == ScreenClampMode.Left || mode == ScreenClampMode.Right || mode == ScreenClampMode.All || mode == ScreenClampMode.Horizontal)
        {
            newTrans.x = Mathf.Clamp(initialPosition.x, screenBoundsX.x + spriteWidth + screenClampPadding.x, screenBoundsX.y - spriteWidth - screenClampPadding.x);
        }

        if (mode == ScreenClampMode.Top || mode == ScreenClampMode.Bottom || mode == ScreenClampMode.All || mode == ScreenClampMode.Vertical)
        {
            newTrans.y = Mathf.Clamp(initialPosition.y, screenBoundsY.x + spriteHeight + screenClampPadding.y, screenBoundsY.y - spriteHeight - screenClampPadding.y);
        }

        transform.position = new Vector3(newTrans.x, newTrans.y, initialPosition.z);
    }

    public void StopFlashIndicator()
    {
        StopAllCoroutines();
        SetRendererVisibility(false);
    }
    public void FlashIndicator(float duration = 4f, float interval = .25f, float flashDuration = .15f)
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        flashRoutine = StartCoroutine(FlashIndicatorRoutine(duration, interval, flashDuration));
    }
    private IEnumerator FlashIndicatorRoutine(float duration = 4f, float interval = .25f, float flashDuration = .15f)
    {
        // Duration = How long the total flashing should last.
        // Interval = How often the indicator flashes.
        // flashDuration = How long the indicator is visible for for each interval flash. Should never be > interval.

        // Ensure variables are acceptable.
        duration = Mathf.Clamp(duration, 0.01f, float.MaxValue);
        interval = Mathf.Clamp(interval, 0.01f, float.MaxValue);
        flashDuration = Mathf.Clamp(flashDuration, .01f, interval - .01f);

        // Start up Interval Routine
        if (intervalRoutine != null)
        {
            StopCoroutine(intervalRoutine);
            intervalRoutine = null;
        }
        intervalRoutine = StartCoroutine(IntervalRoutine(interval, flashDuration));

        // Flash timer
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            yield return new WaitForEndOfFrame();
            timeElapsed += Time.deltaTime;
        }

        // Timer has ended, so stop routines.
        StopCoroutine(intervalRoutine);
        intervalRoutine = null;
        GetComponent<Renderer>().enabled = false;

        yield break;
    }

    private IEnumerator IntervalRoutine(float interval, float flashDuration)
    {
        while (true)
        {
            SetRendererVisibility(true);
            yield return new WaitForSeconds(flashDuration);
            SetRendererVisibility(false);
            yield return new WaitForSeconds(interval - flashDuration);
        }
    }

    public void InterfaceEvent(string eventName)
    {
        switch (eventName)
        {
            case "Enable":
                SetRendererVisibility(true);
                break;
            case "Disable":
                SetRendererVisibility(false);
                break;
            default:
                break;
        }
    }

    private void SetRendererVisibility(bool visible)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }
}
