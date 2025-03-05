using UnityEngine;
using UnityEngine.UI;

public class ButtonHoverAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnHoverEnter()
    {
        animator.SetTrigger("Hover");
    }
    public void OnHoverExit()
    {
        animator.SetTrigger("Normal");
    }
    public void OnClick()
    {
        animator.SetTrigger("Click");
    }
}
