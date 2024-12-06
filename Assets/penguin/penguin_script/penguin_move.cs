using UnityEngine;


namespace Avata1{

[RequireComponent(typeof(Rigidbody))]

    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private Joystick joystick;
        public float speed = 5f;
        public float runSpeedMultiplier = 1.5f;
        public float slideSpeedMultiplier = 3f; // 슬라이딩 속도 배수
        public float slideDecelerationRate = 5f; // 슬라이딩 감속률 (슬라이딩 속도 줄어드는 비율)
        public float jumpForce = 5f;

        private Animator animator;
        private Rigidbody rb;
        private Vector3 moveDirection;
        private bool isGrounded = true;

        private bool isSliding = false; // 슬라이딩 상태 확인용
        private bool isRunning = false; // 달리기 상태

        private float currentSlideSpeed; // 슬라이딩 중 현재 속도

        public bool isActive = false; // 캐릭터 활성화 여부
        private CustomCameraController cameraController; // 카메라 컨트롤러

        void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            // CustomCameraController를 찾아 설정
            cameraController = FindAnyObjectByType<CustomCameraController>();
        }

        void Update()
        {
            if (!isActive)
            {
                StopMovement();
                return;
            }

            // 입력 받기
            float horizontal = joystick.Horizontal + Input.GetAxis("Horizontal");
            float vertical = joystick.Vertical + Input.GetAxis("Vertical");

            // 카메라 기준으로 이동 방향 계산
            if (cameraController != null)
            {
                Vector3 cameraForward = cameraController.CameraForward;
                Vector3 cameraRight = new Vector3(cameraForward.z, 0, -cameraForward.x); // 카메라의 오른쪽 방향
                moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
            }

            // 이동 및 회전 처리
            if (moveDirection.magnitude > 0.1f && !isSliding)
            {
                RotateCharacter(moveDirection);
                Move();
            }
            else
            {
                StopMovement();
            }

            // 달리기 처리
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Run(true);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                Run(false);
            }


            // 점프 처리
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }

            // 슬라이딩 처리
            if (Input.GetKeyDown(KeyCode.LeftControl) && moveDirection.magnitude > 0)
            {
                Slide();
            }

            // 이동 처리
            if (moveDirection.magnitude > 0 && !isSliding)
            {
                Move();
            }
            else if (!isSliding)
            {
                StopMovement();
            }

            if (isSliding)
            {
                HandleSliding();
            }
        }

        private void RotateCharacter(Vector3 direction)
        {
            // 캐릭터를 이동 방향으로 즉각 회전
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        private void Move()
        {
            rb.linearVelocity = moveDirection * speed + new Vector3(0, rb.linearVelocity.y, 0);
            animator.SetBool("isWalking", true);
        }

        private void StopMovement()
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            animator.SetBool("isWalking", false);
        }

        private void Jump()
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
                animator.SetBool("isJumping", true);
            }
        }

        public void Run(bool isRunning)
        {
            this.isRunning = isRunning;
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isWalking", !isRunning);
        }

        public void Slide()
        {
            if (isGrounded && moveDirection.magnitude > 0)
            {
                isSliding = true;
                currentSlideSpeed = speed * slideSpeedMultiplier;
                animator.SetBool("isSliding", true);
            }
        }

        private void HandleSliding()
        {
            currentSlideSpeed -= slideDecelerationRate * Time.deltaTime;

            if (currentSlideSpeed <= 0)
            {
                isSliding = false;
                animator.SetBool("isSliding", false);
                return;
            }

            transform.forward = moveDirection;
            Vector3 moveVelocity = moveDirection * currentSlideSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                animator.SetBool("isJumping", false);
            }
        }
    }
}