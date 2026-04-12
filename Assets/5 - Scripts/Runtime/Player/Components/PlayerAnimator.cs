using KinematicCharacterController;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private GameObject Human;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private void Start()
    {
        Transform child = Human.transform.GetChild(0);
        animator = child.GetComponent<Animator>();
    }
    private void Update()
    {
        float speed = motor.BaseVelocity.magnitude;
        animator.SetFloat(SpeedHash, speed);
    }

}
