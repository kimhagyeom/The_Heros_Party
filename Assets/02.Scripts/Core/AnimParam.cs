using UnityEngine;

public enum AnimParam
{
    IsMoving,//bool
    Attack,//Trigger
    Death,
    Damaged
}
public static class AnimHash
{
    public static readonly int IsMoving = Animator.StringToHash(nameof(AnimParam.IsMoving));
    public static readonly int Attack = Animator.StringToHash(nameof(AnimParam.Attack));
    public static readonly int Death = Animator.StringToHash(nameof(AnimParam.Death));
    public static readonly int Damaged = Animator.StringToHash(nameof(AnimParam.Damaged));
}