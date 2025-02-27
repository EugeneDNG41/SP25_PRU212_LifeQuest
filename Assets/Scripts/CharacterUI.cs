using UnityEngine;

public class AgeBasedAnimation : MonoBehaviour
{
    private Animator animator;
    public GameObject GameObject;
    void Start()
    {
        animator = GetComponent<Animator>();
        //PlayAnimationByAge(age);
    }

    public void PlayAnimationByAge(int age)
    {
        if (age < 0 || age > 100) return; // Ignore invalid ages

        string triggerName = GetAnimationTriggerByAge(age);
        if (!string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    public void PlayDeath()
    {
        animator.SetTrigger("PlayDeath");
    }

    private string GetAnimationTriggerByAge(int age)
    {
        if (age >= 0 && age <= 4) return "PlayAnim1";  // Toddler
        if (age >= 5 && age <= 10) return "PlayAnim2"; // Child
        if (age >= 11 && age <= 17) return "PlayAnim3"; // Adolescent
        if (age >= 18 && age <= 39) return "PlayAnim4"; // Young Adult
        if (age >= 40 && age <= 64) return "PlayAnim5"; // Middle Age
        if (age >= 65 && age <= 100) return "PlayAnim6"; // Elder

        return ""; // No animation if age is out of range
    }
}
