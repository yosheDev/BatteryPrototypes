using FunctionLibrary;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ElectricNode : MonoBehaviour
{
    private enum ShockState
    {
        None,
        Telegraph,
        Shock
    }

    [HideInInspector] public HashSet<ElectricNode> withinRangeNodes = new HashSet<ElectricNode>();      /// Nodes that are within the connection range of this node.
    [HideInInspector] public HashSet<ElectricNode> connectedNodes = new HashSet<ElectricNode>();        /// Hashset is populated with nodes that a chain already exists between. Used in preventing duplicate chains.
    private Dictionary<ElectricNode, LineRenderer> elecLines = new Dictionary<ElectricNode, LineRenderer>();
    private ShockState shockState = ShockState.None;

    public bool extendShock = true;                 /// When true, extends the shock. Typically, this is false on dummy ElectricNodes that only exist to zap certain points.
    [HideInInspector] public bool updateConnectedAlreadyCalled = false;

    [Header("Shock Leader")]
    public bool isLeader = false;                   /// When true, this node runs its coroutine and activates shock stuff on connected nodes.
    public float shockDuration = 1f;
    public float shockInterval = 2f;
    public float shockTelegraphDur = 1f;
    private Coroutine shockRoutine;

    [Header("Zap Point")]                           /// If this a zap point, it can only connect with certain specified nodes. Think node zapping a spot on a wall or something.
    public bool isZapPoint = false;
    public List<ElectricNode> zapPointAcceptableNodes = new List<ElectricNode>();

    [Header("VFX")]
    public GameObject lineRendererPrefab;           /// Prefab for line renderer.
    public Collider2D nodeCol;                      /// Sprite of the beam. May not use this.
    public Material shockMat;                       /// Material to use for the beam.
    private float widthMultiplier = 1f;             /// Width of the beam. Might be dynamic at runtime, plan as such.

    private void Start()
    {
        if (isLeader && !isZapPoint)
        {
            shockRoutine = StartCoroutine(ShockLoop());
        }
    }

    private void FixedUpdate()
    {
        if (shockState == ShockState.None)
        {
            foreach (KeyValuePair<ElectricNode, LineRenderer> line in elecLines)
            {
                line.Value.startColor = new Color(1f, 1f, 1f, 0f);
                line.Value.endColor = new Color(1f, 1f, 1f, 0f);
            }
            return;
        }

        foreach (KeyValuePair<ElectricNode, LineRenderer> line in elecLines)
        {
            line.Value.startColor = new Color(1f, 1f, 1f, (shockState == ShockState.Telegraph) ? .4f : .8f);
            line.Value.endColor = new Color(1f, 1f, 1f, (shockState == ShockState.Telegraph) ? .4f : .8f);

            // Damage tracing
        }
    }

    private IEnumerator ShockLoop()
    {
        while (true)
        {
            shockState = ShockState.None;
            UpdateConnectedNodes();

            yield return new WaitForSeconds(shockInterval);

            // Telegraph animation
            shockState = ShockState.Telegraph;   
            UpdateConnectedNodes();

            yield return new WaitForSeconds(shockTelegraphDur);

            // Shock
            shockState = ShockState.Shock;
            UpdateConnectedNodes();

            yield return new WaitForSeconds(shockDuration);
        }
    }

    public void AddConnectNode(ElectricNode node)
    {
        if (isZapPoint)
        {
            return;
        }

        connectedNodes.Add(node);
    }

    /// <summary>
    /// Iterates through entire chain of nodes to be the same as shockState.
    /// </summary>
    public void UpdateConnectedNodes()
    {
        updateConnectedAlreadyCalled = true;
        StartCoroutine(ResetUpdateConnectedAlreadyCalled());

        if (!isZapPoint)
        {
            foreach (ElectricNode rangeNode in withinRangeNodes)
            {
                if (connectedNodes.Contains(rangeNode) || rangeNode == this || rangeNode.isZapPoint) // Infinite loop with rangeNode == this causing stack overflow. Make it so UpdateConnectedNodes can only call once per node y'know.
                {
                    continue;
                }
                else
                {
                    rangeNode.AddConnectNode(this);
                    if (!rangeNode.updateConnectedAlreadyCalled)
                    {
                        rangeNode.shockState = shockState;
                        rangeNode.UpdateConnectedNodes();
                    }

                    // If there is no line renderer connecting the two nodes already, then do that now.
                    if (!(elecLines.ContainsKey(rangeNode)))
                    {
                        CreateNewElecLine(rangeNode);
                    }
                }
            }
        }
        else
        {
            foreach(ElectricNode node in zapPointAcceptableNodes)
            {
                if (!node.updateConnectedAlreadyCalled)
                {
                    node.UpdateConnectedNodes();
                }

                // If there is no line renderer connecting the two nodes already, then do that now.
                if (!(elecLines.ContainsKey(node)))
                {
                    CreateNewElecLine(node);
                }
            }
        }
    }
    
    private IEnumerator ResetUpdateConnectedAlreadyCalled()
    {
        yield return null;
        updateConnectedAlreadyCalled = false;
    }
    private void CreateNewElecLine(ElectricNode node)
    {
        GameObject lineObj = GameObject.Instantiate(lineRendererPrefab);
        lineObj.transform.parent = node.transform;
        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
        lineRenderer.material = shockMat;
        lineRenderer.startWidth = widthMultiplier * 0.5f;
        lineRenderer.endWidth = widthMultiplier * 0.5f;
        lineRenderer.positionCount = 2;
        //lineRenderer.SetPosition(0, nodeCol.ClosestPoint(node.transform.position));
        lineRenderer.SetPosition(0, transform.position);
        //lineRenderer.SetPosition(1, node.nodeCol.ClosestPoint(transform.position));
        lineRenderer.SetPosition(1, node.transform.position);
        lineObj.GetComponent<ElectricLineRendererUpdate>().SetStartPointParent(transform);
        lineObj.GetComponent<ElectricLineRendererUpdate>().SetEndPointParent(node.transform);
        elecLines.Add(node, lineRenderer);

        //Debug.Log("Creating elec line " + lineRenderer + " between " + this.gameObject + " and " + node.gameObject);
    }

    public void RemoveRangeNode(ElectricNode rangeNode)
    {
        withinRangeNodes.Remove(rangeNode);
        connectedNodes.Remove(rangeNode);
        LineRenderer lineToDestroy;
        elecLines.TryGetValue(rangeNode, out lineToDestroy);
        elecLines.Remove(rangeNode);
        if (lineToDestroy != null)
        {
            Destroy(lineToDestroy.gameObject);
        }
    }
}
