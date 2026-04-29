using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float smoothMove = 10f;

    [Header("鼠标视角")]
    public float mouseSensitivity = 2f;
    public float camClampUp = 60f;
    public float camClampDown = -60f;
    private float _camPitch;

    [Header("走路相机摇晃（仅相机）")]
    public bool enableWalkBob = true;
    public float bobSpeed = 12f;
    public float bobUpAmount = 0.06f;
    public float bobSideAmount = 0.03f;
    private float _bobTime;
    private Vector3 _camOriginalLocalPos;

    [Header("绑定")]
    public Camera mainCam;

    private CharacterController _cc;
    private Vector3 _moveDir;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        // 记录相机初始本地位置
        _camOriginalLocalPos = mainCam.transform.localPosition;

        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MouseLookRotateOnlyY();
        PlayerMove();
        CameraWalkBob();
    }

    // 核心：角色只旋转Y轴，上下只有相机动
    void MouseLookRotateOnlyY()
    {
        // 左右鼠标 -> 角色Y轴旋转
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        // 上下鼠标 -> 仅相机俯仰，角色不动
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        _camPitch -= mouseY;
        _camPitch = Mathf.Clamp(_camPitch, camClampDown, camClampUp);

        mainCam.transform.localEulerAngles = new Vector3(_camPitch, 0, 0);
    }

    // WASD 视角朝向移动
    void PlayerMove()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Transform camTf = mainCam.transform;
        Vector3 forward = camTf.forward;
        Vector3 right = camTf.right;
        // 水平移动，剔除Y
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        _moveDir = (forward * v + right * h).normalized;
        _cc.Move(_moveDir * moveSpeed * Time.deltaTime);
    }

    // 仅相机摇晃！主角完全不动
    void CameraWalkBob()
    {
        if (!enableWalkBob)
        {
            // 平滑回归相机默认位置
            mainCam.transform.localPosition = Vector3.Lerp(
                mainCam.transform.localPosition,
                _camOriginalLocalPos,
                Time.deltaTime * 15f
            );
            return;
        }

        // 判断是否在地面移动
        bool isMoving = Mathf.Abs(_cc.velocity.x) > 0.1f || Mathf.Abs(_cc.velocity.z) > 0.1f;

        if (isMoving)
        {
            _bobTime += Time.deltaTime * bobSpeed;
            // 上下起伏 + 左右轻微摆动
            float bobY = Mathf.Abs(Mathf.Sin(_bobTime)) * bobUpAmount;
            float bobX = Mathf.Cos(_bobTime) * bobSideAmount;

            mainCam.transform.localPosition = new Vector3(
                _camOriginalLocalPos.x + bobX,
                _camOriginalLocalPos.y - bobY,
                _camOriginalLocalPos.z
            );
        }
        else
        {
            // 静止重置
            _bobTime = 0;
            mainCam.transform.localPosition = Vector3.Lerp(
                mainCam.transform.localPosition,
                _camOriginalLocalPos,
                Time.deltaTime * 15f
            );
        }
    }
}