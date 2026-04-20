using KinematicCharacterController;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Player.PlayerController controller;
    private Quaternion _smoothedHeadRot;
    [SerializeField] private float smoothSpeed = 15f;

    private Transform headBone;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    public void SetupNewCharacter(GameObject characterObj)
    {
        animator = characterObj.GetComponent<Animator>();

        if (animator != null && animator.isHuman)
        {
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        }
        else
        {
            headBone = characterObj.transform.FindRecursive("Head");
        }
    }

    private void Update()
    {
        if (animator == null) return;
        float speed = motor.BaseVelocity.magnitude;
        animator.SetFloat(SpeedHash, speed);
    }

    private void LateUpdate()
    {
        if (headBone == null || controller == null) return;
        float yaw = controller.SharedCamYaw.Value;
        float pitch = controller.SharedCamPitch.Value;

        // Берем актуальный поворот тела из KCC
        float bodyYaw = controller.Motor.TransientRotation.eulerAngles.y;

        float relativeYaw = Mathf.DeltaAngle(bodyYaw, yaw);
        relativeYaw = Mathf.Clamp(relativeYaw, -80f, 80f);
        float clampedPitch = Mathf.Clamp(pitch, -50f, 50f);
        Quaternion targetHeadRot = Quaternion.Euler(clampedPitch, relativeYaw, 0);
        _smoothedHeadRot = Quaternion.Slerp(_smoothedHeadRot, targetHeadRot, Time.deltaTime * smoothSpeed);
        headBone.localRotation = _smoothedHeadRot;
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