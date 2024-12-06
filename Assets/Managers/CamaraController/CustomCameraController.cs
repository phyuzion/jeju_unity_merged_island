using UnityEngine;

public class CustomCameraController : MonoBehaviour
{
    public Transform target; // 따라갈 캐릭터
    public Vector3 offset = new Vector3(0, 5, -10); // 기본 오프셋
    public float rotationSpeed = 2000f; // 카메라 회전 속도
    private float currentRotation = 0f; // 카메라의 Y축 회전값

    public Vector3 CameraForward { get; private set; } // 카메라의 forward 방향 (외부에서 접근 가능)

    void LateUpdate()
    {
        if (target != null)
        {
            // 마우스 입력으로 카메라 회전 처리
            HandleRotation();

            // 카메라 위치 계산
            Quaternion rotation = Quaternion.Euler(0, currentRotation, 0);
            Vector3 desiredPosition = target.position + rotation * offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 5);

            // 카메라가 타겟을 바라보게
            transform.LookAt(target);

            // 카메라의 forward 방향 업데이트
            CameraForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼으로 회전
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentRotation += mouseX * rotationSpeed * Time.deltaTime;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
