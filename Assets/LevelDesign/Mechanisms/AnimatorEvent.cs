using Magnet;
using UnityEngine;

public class AnimatorEvent : MonoBehaviour, IInterfaceEvent
{
    [SerializeField] Animator animator;
    public void InterfaceEvent(string name)
    {
        animator.SetTrigger(name);
    }
}
