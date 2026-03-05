using FunctionLibrary;
using System.Collections;
using System.Collections.Generic;
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
    // NOTE: Right now just making these exist and function. May change how these are setup later. For instance: querying nearby nodes in a threshold and shocking with them.
    
    [HideInInspector] public bool updateConnectedAlreadyCalled = false;
    public bool isLeader = false;           /// When true, this node runs its coroutine and activates shock stuff on connected nodes.
    public bool extendShock = true;         /// When true, extends the shock. Typically, this is false on dummy ElectricNodes that only exist to zap certain points.
    public float shockDuration = 1f;
    public float shockInterval = 2f;
    public float shockTelegraphDur = 1f;
    private ShockState shockState = ShockState.None;
    private Coroutine shockRoutine;

    #region Electric VFX
    [Header("VFX")]
    private Vector2 endPos;                             /// Pos to use for end of effect.

    public Collider2D nodeCol;                      /// Sprite of the beam. May not use this.
    public Material shockMat;                      /// Material to use for the beam.
    private float widthMultiplier = 1f;                          /// Width of the beam. Might be dynamic at runtime, plan as such.
    private Vector2 dir = Vector2.zero;                 /// Direction of the beam.

    #endregion

    private void Start()
    {
        if (isLeader)
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
            Debug.Log("Shock");
            shockState = ShockState.Shock;
            UpdateConnectedNodes();

            yield return new WaitForSeconds(shockDuration);
        }
    }

    public void AddConnectNode(ElectricNode node)
    {
        connectedNodes.Add(node);
    }

    /// <summary>
    /// Iterates through entire chain of nodes to be the same as shockState.
    /// </summary>
    public void UpdateConnectedNodes()
    {
        updateConnectedAlreadyCalled = true;
        StartCoroutine(ResetUpdateConnectedAlreadyCalled());
        /// Need a way to set this to false once all of the nodes have been gone through. Maybe just wait one frame?
        foreach (ElectricNode rangeNode in withinRangeNodes)
        {
            if (connectedNodes.Contains(rangeNode) || rangeNode.updateConnectedAlreadyCalled || rangeNode == this) // Infinite loop with rangeNode == this causing stack overflow. Make it so UpdateConnectedNodes can only call once per node y'know.
            {
                continue;
            }
            else
            {
                rangeNode.AddConnectNode(this);
                rangeNode.shockState = shockState;
                rangeNode.UpdateConnectedNodes();
                

                // If there is no line renderer connecting the two nodes already, then do that now.
                if (!(elecLines.ContainsKey(rangeNode)))
                {
                    CreateNewElecLine(rangeNode);
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
        //try
        //{
            LineRenderer lineRenderer = this.gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = shockMat;
            lineRenderer.startWidth = widthMultiplier * 0.5f;
            lineRenderer.endWidth = widthMultiplier * 0.5f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, nodeCol.ClosestPoint(node.transform.position));
            lineRenderer.SetPosition(1, node.nodeCol.ClosestPoint(transform.position));
            elecLines.Add(node, lineRenderer);

            Debug.Log("Creating elec line " + lineRenderer + " between " + this.gameObject + " and " + node.gameObject);
        //}
        //catch
        //{
        //    Debug.LogWarning("Not sure why, but lineRenderer was null immediately after creating it in ElectricNode.cs");
        //}
    }

    public void RemoveRangeNode(ElectricNode rangeNode)
    {
        withinRangeNodes.Remove(rangeNode);
        elecLines.Remove(rangeNode);
    }
}
