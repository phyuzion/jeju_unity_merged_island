using UnityEngine;

namespace Avata3{
        
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private Joystick joystick;
        public float speed = 5f;
        public float runSpeedMultiplier = 1.5f;
        public float jumpForce = 5f;
        public float slideSpeed = 8f;
        public float slideDuration = 0.5f;

        private Animator animator;
        private Rigidbody rb;
        private Vector3 moveDirection;
        private bool isGrounded = true;
        private bool isSliding = false;
        private bool isDancing = false; // 춤 상태 토글
        private bool isRunning = false; // 달리기 상태 토글
        private float slideTimer = 0f;
        private float currentSlideSpeed = 0f;

        public bool isActive = false;

        private CustomCameraController cameraController; // 카메라 컨트롤러
        void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            Vector3 startPosition = transform.position;
            startPosition.y += 0.5f;
            transform.position = startPosition;

            // CustomCameraController를 찾아 설정
            cameraController = FindAnyObjectByType<CustomCameraController>();
        }

        void Update()
        {

            if(!isActive){
                return;
            }

            if (isDancing) return; // 춤 상태 중 다른 입력 무시

            float horizontal = joystick.Horizontal + Input.GetAxis("Horizontal");
            float vertical = joystick.Vertical + Input.GetAxis("Vertical");


            // 카메라 기준으로 이동 방향 계산
            if (cameraController != null)
            {
                Vector3 cameraForward = cameraController.CameraForward;
                Vector3 cameraRight = new Vector3(cameraForward.z, 0, -cameraForward.x); // 카메라의 오른쪽 방향
                moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
            }

            bool isJumping = Input.GetKeyDown(KeyCode.Space) && isGrounded;
            bool isSlidingInput = Input.GetKeyDown(KeyCode.C) && isGrounded; // 슬라이딩 키보드 입력

            // 이동 처리
            if (!isSliding)
            {
                HandleMovement(moveDirection);
            }

            // 슬라이딩 처리
            if (isSlidingInput && !isSliding)
            {
                StartSliding();
            }

            if (isSliding)
            {
                slideTimer -= Time.deltaTime;
                currentSlideSpeed = Mathf.Lerp(0, slideSpeed, slideTimer / slideDuration);
                Vector3 slideVelocity = transform.forward * currentSlideSpeed;
                rb.linearVelocity = new Vector3(slideVelocity.x, rb.linearVelocity.y, slideVelocity.z);

                if (slideTimer <= 0)
                {
                    StopSliding();
                }
            }

            if (isJumping)
            {
                Jump();
            }
        }

        // 이동 처리 메서드
        private void HandleMovement(Vector3 moveDirection)
        {
            if (moveDirection.magnitude > 0)
            {
                animator.SetBool("isWalking", !isRunning);
                animator.SetBool("isRunning", isRunning);

                float currentSpeed = speed * (isRunning ? runSpeedMultiplier : 1f);
                transform.forward = moveDirection;

                Vector3 moveVelocity = moveDirection * currentSpeed;
                rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }

        // 점프 버튼 외부 호출
        public void JumpButton()
        {
            if (isGrounded && !isSliding)
            {
                Jump();
            }
        }

        // 달리기 버튼 외부 호출 (UI 버튼 포함, 토글 방식)
        public void ToggleRunButton()
        {
            isRunning = !isRunning; // 달리기 상태 반전
            animator.SetBool("isRunning", isRunning);
        }

        // 슬라이딩 버튼 외부 호출
        public void SlideButton()
        {
            if (!isSliding && isGrounded)
            {
                StartSliding();
            }
        }

        // 춤추기 버튼 - UI 버튼 포함, 토글 방식
        public void ToggleDanceButton()
        {
            if (!isSliding && isGrounded)
            {
                if (isDancing)
                {
                    StopDancing();
                }
                else
                {
                    StartDancing();
                }
            }
        }

        private void Jump()
        {
            animator.SetBool("isJumping", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        private void StartSliding()
        {
            isSliding = true;
            slideTimer = slideDuration;
            currentSlideSpeed = slideSpeed;
            animator.SetBool("isSliding", true);
        }

        private void StopSliding()
        {
            isSliding = false;
            animator.SetBool("isSliding", false);
        }

        private void StartDancing()
        {
            isDancing = true;
            animator.SetTrigger("Dance"); // 춤 시작
        }

        private void StopDancing()
        {
            isDancing = false;
            animator.ResetTrigger("Dance"); // 춤 종료
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