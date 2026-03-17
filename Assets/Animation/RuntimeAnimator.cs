using UnityEngine;

public class RuntimeAnimator : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    public float playRate = 1f;
    public float startOffset = 0f;
    protected void OnValidate()
    {
        if (animator == null)
        {
            if (GetComponent<Animator>() == null)
            {
                Debug.LogError("Animator is not set on " + gameObject.name);
            }
            else
            {
                animator = GetComponent<Animator>();
            }
        }
    }
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetFloat("playRate", playRate);
        //animator.SetFloat("normalizedOffset", startOffset);
        // Start Offset
        AnimatorClipInfo[] currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (currentClipInfo.Length > 0)
        {
            animator.Play(currentClipInfo[0].clip.name, 0, startOffset);
        }
        //animator.Play(currentClipInfo[0].clip.name);

    }
}
