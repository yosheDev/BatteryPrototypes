using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class RayHazardEmitter : MonoBehaviour
{
    [SerializeField] private Vector2 direction = new Vector2();
    [SerializeField] private float raySpeed = 3f;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask traceLayerMask;
   

    private Vector3 targetLocation;
    private Coroutine emitRoutine;
    private void Start()
    {
        direction = direction.normalized;

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.material.SetFloat("_Speed", raySpeed);
        lineRenderer.enabled = false;
    }

    public void EmitEnd()
    {
        StopCoroutine(emitRoutine);
        emitRoutine = null;
        lineRenderer.enabled = false;
    }

    public void EmitBegin()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.enabled = true;
        
        emitRoutine = StartCoroutine(EmitRay());
    }

    private IEnumerator EmitRay()
    {
        while (true)
        {
            RaycastHit2D furthestHit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, traceLayerMask);

            if (furthestHit.collider != null)
            {
                targetLocation = furthestHit.point;
                targetLocation.z = transform.position.z;
            }
            else
            {
                targetLocation = transform.position + ((Vector3)direction * 99999f);
            }

            // If beam should now be shorter than the current position.
            if ((transform.position - targetLocation).magnitude <= (transform.position - lineRenderer.GetPosition(1)).magnitude)
            {
                lineRenderer.SetPosition(1, targetLocation);
            }

            else
            {
                Vector3 advancePos = lineRenderer.GetPosition(1) + ((Vector3)direction * raySpeed * .01f);
                lineRenderer.SetPosition(1, advancePos);
            }
            //Debug.Log(lineRenderer.GetPosition(1));
            yield return new WaitForEndOfFrame();
        }
    }
}
