using UnityEngine;

namespace Avata4{
        
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private Joystick joystick;

        public float speed = 5f; // 기본 걷기 속도
        public float runSpeedMultiplier = 1.5f; // 달리기 속도 배수
        public float jumpForce = 5f; // 점프 힘

        private Animator animator; // 캐릭터 애니메이터
        private Rigidbody rb; // Rigidbody 컴포넌트
        
        private Vector3 moveDirection;
        private bool isGrounded = true; // 땅에 닿았는지 여부
        private bool isRunning = false; // 달리기 상태
        private bool runButtonPressed = false;  // 런 버튼 눌렸는지 여부
        private bool isProne = false; // 엎드린 상태인지 여부

        public bool isActive = false; // 캐릭터 활성화 여부
        private CustomCameraController cameraController; // 카메라 컨트롤러

        void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();

            // Rigidbody 회전을 고정합니다.
            rb.freezeRotation = true;


            // CustomCameraController를 찾아 설정
            cameraController = FindAnyObjectByType<CustomCameraController>();
        }

        void Update()
        {

            if (!isActive){
                StopCharacter();
                return;
            }


            // 입력 처리
            float horizontal = joystick.Horizontal + Input.GetAxis("Horizontal");
            float vertical = joystick.Vertical + Input.GetAxis("Vertical");

            // 카메라 기준으로 이동 방향 계산
            if (cameraController != null)
            {
                Vector3 cameraForward = cameraController.CameraForward;
                Vector3 cameraRight = new Vector3(cameraForward.z, 0, -cameraForward.x); // 카메라의 오른쪽 방향
                moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
            }
            // 엎드리기 상태 전환
            if (Input.GetKeyDown(KeyCode.X))
            {
                ToggleProne();
            }

            // 엎드린 상태에서는 이동
            if (isProne && moveDirection.magnitude > 0)
            {
                ProneMove(moveDirection);
            }
            else if (moveDirection.magnitude > 0)
            {
                MoveCharacter(moveDirection);
            }
            else
            {
                StopCharacter();
            }

            // 달리기 처리
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Run(true);
            }
            else if (runButtonPressed)
            {
                Run(true);
            }
            else
            {
                Run(false);
            }

            // 점프
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }

        public void MoveCharacter(Vector3 moveDirection)
        {
            float currentSpeed = speed * (isRunning ? runSpeedMultiplier : 1f);

            // 캐릭터의 이동 방향 설정
            transform.forward = moveDirection;

            // Rigidbody로 이동 처리
            Vector3 moveVelocity = moveDirection * currentSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

            // 애니메이션 상태 업데이트
            animator.SetBool("isWalking", !isRunning);
            animator.SetBool("isRunning", isRunning);
        }

        public void ProneMove(Vector3 moveDirection)
        {
            float proneSpeed = speed * 0.5f; // 엎드릴 때 속도 감소

            // 캐릭터의 이동 방향 설정
            transform.forward = moveDirection;

            // Rigidbody로 이동 처리
            Vector3 moveVelocity = moveDirection * proneSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

            // 애니메이션 상태 업데이트
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isPW", true);
        }

        public void StopCharacter()
        {
            // 애니메이션 중지
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isPW", false);

            // 이동 중지
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        public void Jump()
        {
            if (isGrounded && !isProne) // 엎드린 상태에서는 점프 불가능
            {
                // 점프 애니메이션 실행
                animator.SetTrigger("doJumping");

                // 점프 물리 적용
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        public void OnRunButtonPressed(bool pressed)
        {
            runButtonPressed = pressed;
        }

        public void Run(bool shouldRun)
        {
            if (!isProne) // 엎드린 상태에서는 달리기 불가능
            {
                isRunning = shouldRun;

                // 디버그 메시지로 상태 확인
                Debug.Log($"Run state: {isRunning}");
            }
        }

        public void ToggleProne()
        {
            isProne = !isProne;
            animator.SetBool("isProne", isProne);

            // 디버그 메시지
            Debug.Log($"Prone state: {isProne}");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                // 땅에 닿았을 때 점프 상태 해제
                isGrounded = true;
                animator.SetBool("isJumping", false);
            }
        }
    }


}