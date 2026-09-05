using KinematicCharacterController;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Player.PlayerController controller;

    [Header("Head Settings")]
    [SerializeField] private float headSmoothSpeed = 15f;
    private Quaternion _smoothedHeadRot;
    private Transform headBone;

    [Header("Turn Settings")]
    [SerializeField] private float turnSmoothTime = 0.4f;
    [SerializeField] private float turnDeadzone = 0.1f; 

    private float _lastRotationY;
    private float _currentTurnValue;
    private float _turnVelocityRef;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int TurnSpeedHash = Animator.StringToHash("TurnSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int InteractHash = Animator.StringToHash("Interact");
    private float speed;

    public void SetupCharacters(GameObject humanObj, GameObject ghostObj)
    {
        Animator skinAnimator = humanObj.GetComponent<Animator>();
        if (skinAnimator != null)
        {
            float currentSpeed = animator.GetFloat(SpeedHash);
            float currentTurnSpeed = animator.GetFloat(TurnSpeedHash);
            bool currentGrounded = animator.GetBool(IsGroundedHash);

            animator.avatar = skinAnimator.avatar;
            Destroy(skinAnimator);

            animator.Rebind();
            animator.Update(0f);

            animator.SetFloat(SpeedHash, currentSpeed);
            animator.SetFloat(TurnSpeedHash, currentTurnSpeed);
            animator.SetBool(IsGroundedHash, currentGrounded);
        }

        Animator ghostSkinAnimator = ghostObj.GetComponent<Animator>();
        if (ghostSkinAnimator != null)
        {
            Destroy(ghostSkinAnimator);
        }

        if (animator != null && animator.isHuman)
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        else
            headBone = humanObj.transform.FindRecursive("Head");

        _lastRotationY = motor.TransientRotation.eulerAngles.y;
    }

    private void Update()
    {
        if (animator == null || controller == null) return;

        if (!controller.IsOwner)
        {
            motor.SetRotation(controller.NetworkBodyRotation.Value);
            return; 
        }


        speed = motor.BaseVelocity.magnitude;
        animator.SetFloat(SpeedHash, speed);

        float currentRotationY = motor.TransientRotation.eulerAngles.y;
        float deltaRotation = Mathf.DeltaAngle(_lastRotationY, currentRotationY);
        _lastRotationY = currentRotationY;
        float targetTurnValue = (deltaRotation / Time.deltaTime) / 100f;

        if (Mathf.Abs(targetTurnValue) < turnDeadzone)
            targetTurnValue = 0;

        _currentTurnValue = Mathf.SmoothDamp(_currentTurnValue, targetTurnValue, ref _turnVelocityRef, turnSmoothTime);

        bool isGrounded = motor.GroundingStatus.IsStableOnGround;

        animator.SetFloat(TurnSpeedHash, _currentTurnValue);
        animator.SetBool(IsGroundedHash, isGrounded);
    }

    private void LateUpdate()
    {
        if (headBone == null || controller == null) return;

        float yaw, pitch;
        if (controller.IsOwner && !Anomalies.SplitControlAnomaly.IsSplitActive && controller.CameraTransform != null)
        {
            yaw = controller.CameraTransform.eulerAngles.y;
            pitch = controller.CameraTransform.localEulerAngles.x;
            if (pitch > 180) pitch -= 360;
        }
        else
        {
            yaw = controller.SharedCamYaw.Value;
            pitch = controller.SharedCamPitch.Value;
        }

        float bodyYaw = motor.TransientRotation.eulerAngles.y;

        float relativeYaw = Mathf.DeltaAngle(bodyYaw, yaw);
        relativeYaw = Mathf.Clamp(relativeYaw, -80f, 80f);
        float clampedPitch = Mathf.Clamp(pitch, -50f, 50f);
        Quaternion targetHeadRot = Quaternion.Euler(clampedPitch, relativeYaw, 0);

        _smoothedHeadRot = Quaternion.Slerp(_smoothedHeadRot, targetHeadRot, Time.deltaTime * headSmoothSpeed);
        headBone.localRotation = _smoothedHeadRot;
    }
    public void TriggerJump()
    {
        if (animator != null)
        {
            animator.SetTrigger(JumpHash);
        }
    }
    public void TriggerInteract()
    {
        if (animator != null)
        {
            animator.SetTrigger(InteractHash);
        }
    }
    public void RefreshRig()
    {
        animator.Rebind();
        animator.Update(0f);

        if (animator.isHuman)
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
    }
}
public static class TransformExtensions
{
    public static Transform FindRecursive(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name)) return child;
            Transform result = child.FindRecursive(name);
            if (result != null) return result;
        }
        return null;
    }
}