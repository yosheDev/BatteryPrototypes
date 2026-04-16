using Magnet;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AnimatorEvent : MonoBehaviour, IInterfaceEvent
{
    [SerializeField] Animator animator;
    [SerializeField] private bool disableAtStart = false;
    [SerializeField] private bool hideSpriteAtStart = false;

    [SerializeField] private List<SpriteRenderer> spriteRenderers;

    private void Awake()
    {
        animator.enabled = !disableAtStart;
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.enabled = !disableAtStart;
        }
    }
    public void InterfaceEvent(string name)
    {
        if (name == "Activate")
        {
            animator.enabled = true;
            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                renderer.enabled = true;
            }
        }
        animator.SetTrigger(name);
    }
}
