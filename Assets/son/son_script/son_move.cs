using UnityEngine;

namespace Avata2{
        
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private Joystick joystick;
        public float speed = 5f;
        public float runSpeedMultiplier = 1.5f;
        public float jumpForce = 5f;
        public float rollDistance = 2f;
        public float rollDuration = 0.8f;
        public float hoverHeight = 2f;
        public float hoverLiftForce = 2f;
        public float hoverSpeed = 3f;
        public float hoverMaxHeight = 10f; // 최대 고도 제한
        public float hoverMinHeight = 1f;  // 최소 고도 제한

        private Animator animator;
        private Rigidbody rb;
        private bool isGrounded = true;
        private bool isRolling = false;
        private bool isHovering = false;
        private bool isRunning = false;
        private int jumpCount = 0;
        private float targetHeight;
        private Vector3 hoverDirection = Vector3.zero;


        private Vector3 moveDirection;


        private bool isRunButtonPressed = false; // 버튼 입력 상태 확인

        private CustomCameraController cameraController; // 카메라 컨트롤러

        public bool isActive = false;

        void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            targetHeight = transform.position.y;


            // CustomCameraController를 찾아 설정
            cameraController = FindAnyObjectByType<CustomCameraController>();
        }

        void Update()
        {

            if (!isActive){
                return;
            }


            if (isRolling) return;

            float horizontal = joystick.Horizontal + Input.GetAxis("Horizontal");
            float vertical = joystick.Vertical + Input.GetAxis("Vertical");

            
            // 카메라 기준으로 이동 방향 계산
            if (cameraController != null)
            {
                Vector3 cameraForward = cameraController.CameraForward;
                Vector3 cameraRight = new Vector3(cameraForward.z, 0, -cameraForward.x); // 카메라의 오른쪽 방향
                moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
            }

            // 이동 처리
            if (!isHovering)
            {
                Move(moveDirection);
            }
            else
            {
                HoverMove(moveDirection);
            }

            // 키 입력 처리
            if (Input.GetKeyDown(KeyCode.Space)) Jump();        // 점프
            if (Input.GetKeyDown(KeyCode.LeftControl)) roll();  // 구르기
            if (Input.GetKeyDown(KeyCode.I)) ToggleHover();     // 체공 상태 전환

            // 달리기 상태 전환: Shift 키 또는 버튼 입력에 따라 활성화
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
            run(shiftPressed || isRunButtonPressed);

            // 호버링 중 고도 조정
            if (isHovering)
            {
                if (Input.GetKey(KeyCode.O)) HoverLift();  // 고도 상승
                if (Input.GetKey(KeyCode.P)) HoverDescend(); // 고도 하강
            }
        }

        // 이동 처리
        private void Move(Vector3 direction)
        {
            if (direction.magnitude > 0)
            {
                float currentSpeed = isRunning ? speed * runSpeedMultiplier : speed;
                transform.forward = direction;

                Vector3 moveVelocity = direction * currentSpeed;
                rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

                animator.SetBool("isWalking", !isRunning);
                animator.SetBool("isRunning", isRunning);
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }

        // 버튼 입력 처리 함수 (UI에서 호출)
        public void SetRunButtonState(bool isPressed)
        {
            isRunButtonPressed = isPressed;
        }

        // 달리기 상태 전환
        public void run(bool enable)
        {
            isRunning = enable;
            animator.SetBool("isRunning", isRunning);
        }

        // 점프
        public void Jump()
        {
            if (!isGrounded && jumpCount >= 2) return;

            animator.SetBool("isJumping", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;

            jumpCount++;
            if (jumpCount == 2 && !isHovering)
            {
                EnterHoverState();
            }
        }

        // 구르기
        public void roll()
        {
            if (!isRolling && rb.linearVelocity.magnitude > 0) // 이동 중일 때만 구르기 가능
            {
                StartCoroutine(RollCoroutine(rb.linearVelocity.normalized)); // 코루틴 호출
            }
        }

        private System.Collections.IEnumerator RollCoroutine(Vector3 direction)
        {
            isRolling = true;
            animator.SetBool("isRolling", true);

            float elapsedTime = 0f;
            float rollSpeed = rollDistance / rollDuration;

            while (elapsedTime < rollDuration)
            {
                transform.position += direction * rollSpeed * Time.deltaTime;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            animator.SetBool("isRolling", false);
            isRolling = false;
        }

        // 체공 상태 전환 (날기 상태 포함)
        public void ToggleHover()
        {
            if (isHovering)
            {
                ExitHoverState();
            }
            else
            {
                EnterHoverState();
            }
        }

        // 체공 상태 진입
        public void EnterHoverState()
        {
            isHovering = true;
            rb.useGravity = false;
            animator.SetBool("isHovering", true);
            targetHeight = transform.position.y;
        }

        // 체공 상태 종료
        public void ExitHoverState()
        {
            isHovering = false;
            animator.SetBool("isHovering", false);
            rb.useGravity = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // 점프 적용
            jumpCount = 1; // 점프 카운트 초기화
        }

        // 호버링 이동
        private void HoverMove(Vector3 direction)
        {
            if (direction.magnitude > 0)
            {
                hoverDirection = direction;
                transform.forward = direction;
            }

            rb.linearVelocity = new Vector3(hoverDirection.x * hoverSpeed, rb.linearVelocity.y, hoverDirection.z * hoverSpeed);
        }

        // 호버링 상승
        public void HoverLift()
        {
            if (isHovering && transform.position.y < hoverMaxHeight)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, hoverLiftForce, rb.linearVelocity.z);
            }
        }

        // 호버링 하강
        public void HoverDescend()
        {
            if (isHovering && transform.position.y > hoverMinHeight)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -hoverLiftForce, rb.linearVelocity.z);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                animator.SetBool("isJumping", false);

                if (isHovering)
                {
                    ExitHoverState();
                }
                else
                {
                    jumpCount = 0;
                }
            }
        }
    }

}