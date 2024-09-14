using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetIdle()
    {
        Debug.Log("Setting Idle animation.");
        animator.SetBool("isListening", false);
        animator.SetBool("isPlayingAudio", false);
    }

    public void StartListening()
    {
        Debug.Log("Starting Listen animation.");
        animator.SetBool("isListening", true);
        animator.SetBool("isPlayingAudio", false);
    }

    public void TriggerCoffeeAnimation()
    {
        Debug.Log("Triggering Coffee animation.");
        animator.SetTrigger("triggerCoffee");
    }
}
