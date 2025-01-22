using UnityEngine;

public class CharacterAnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void SetForward(float value)
    {
        if (_animator == null) return;

        _animator.SetFloat("Forward", value);
    }

    public void SetRight(float value)
    {
        if (_animator == null) return;

        _animator.SetFloat("Right", value);
    }

    public void Play_Dash()
    {
        if (_animator == null) return;

        _animator.SetTrigger("Dash");
    }
}
