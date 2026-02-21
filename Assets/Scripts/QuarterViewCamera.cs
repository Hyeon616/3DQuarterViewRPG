using UnityEngine;
using Cinemachine;

public class QuarterViewCamera : MonoBehaviour
{
    [Header("Camera View")]
    [SerializeField] private float height = 4.5f;
    [SerializeField] private float distance = 3f;
    [SerializeField] private float angle = 55f;
    [SerializeField] private float damping = 0f;

    private CinemachineVirtualCamera vcam;
    private CinemachineTransposer transposer;
    private CameraOcclusionController occlusionController;
    private bool hasTarget;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        occlusionController = GetComponent<CameraOcclusionController>();
    }

    void Update()
    {
        if (!hasTarget || transposer == null) return;

        transposer.m_FollowOffset = new Vector3(0f, height, -distance);
        transposer.m_XDamping = damping;
        transposer.m_YDamping = damping;
        transposer.m_ZDamping = damping;
        transform.eulerAngles = new Vector3(angle, 0f, 0f);
    }

    public void SetTarget(Transform target)
    {
        if (vcam == null) return;

        // Transposer 설정
        transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
        {
            vcam.AddCinemachineComponent<CinemachineTransposer>();
            transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        }
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        var composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        if (composer != null)
            Destroy(composer);

        vcam.Follow = target;
        hasTarget = true;

        if (occlusionController != null)
        {
            occlusionController.SetTarget(target);
        }
    }
}
