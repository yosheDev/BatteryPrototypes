using System.Collections.Generic;
using UnityEngine;

public class RayHazardEmitterGroup : MonoBehaviour
{
    [SerializeField] private List<RayHazardEmitter> rayHazards = new List<RayHazardEmitter>();

    public void EmitBegin()
    {
        for (int i = 0; i < rayHazards.Count; i++)
        {
            rayHazards[i].EmitBegin();
        }
    }

    public void EmitEnd()
    {
        for (int i = 0; i < rayHazards.Count; i++)
        {
            rayHazards[i].EmitEnd();
        }
    }
}
