using UnityEngine;
using System.Collections;

namespace HunterRush.Movement
{
    /// <summary>
    /// Advanced movement controller with anime-accurate Hunter x Hunter movement mechanics
    /// </summary>
    public class MovementController : MonoBehaviour
    {
        [Header("Basic Movement")]
        public float baseRunSpeed = 10f;
        public float sprintMultiplier = 1.5f;
        public float jumpPower = 15f;
        public float gravity = 20f;
        public float airControl = 0.3f;
        
        [Header("Nen Enhanced Movement")]
        public float nenSpeedBoost = 1.3f;
        public float nenJumpBoost = 1.5f;
        public ParticleSystem nenTrailEffect;
        public Color nenTrailColor = Color.white;
        
        [Header("Wall Running")]
        public bool canWallRun = true;
        public float wallRunSpeed = 8f;
        public float wallRunDuration = 3f;
        public float wallRunGravity = 5f;
        public float wallJumpForce = 12f;
        public LayerMask wallLayers = 1;
        
        [Header("Dashing")]
        public float dashDistance = 5f;
        public float dashDuration = 0.3f;
        public float dashCooldown = 1f;
        public int maxAirDashes = 1;
        public ParticleSystem dashEffect;
        
        [Header("Ground Detection")]
        public float groundCheckDistance = 0.1f;
        public LayerMask groundLayers = 1;
        public Transform groundCheckPoint;
        
        [Header("Animation")]
        public Animator animator;
        
        // Components
        private Rigidbody rb;
        private CapsuleCollider col;
        private BaseCharacter character;
        private NenController nenController;
        
        // Movement state
        private Vector3 moveInput;
        private Vector3 velocity;
        private bool isGrounded = true;
        private bool isSprinting = false;
        private bool isWallRunning = false;
        private bool isDashing = false;
        private bool canDash = true;
        private int airDashCount = 0;
        
        // Wall running
        private Vector3 wallNormal;
        private float wallRunTimer = 0f;
        private bool isWallRunningLeft = false;
        
        // Dash
        private Vector3 dashDirection;
        private float dashTimer = 0f;
        
        // Input
        private bool jumpInput = false;
        private bool sprintInput = false;
        private bool dashInput = false;
        
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();
            character = GetComponent<BaseCharacter>();
            nenController = GetComponent<NenController>();
            
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
        
        void Start()
        {
            // Setup physics
            rb.freezeRotation = true;
            
            if (groundCheckPoint == null)
            {
                groundCheckPoint = new GameObject("GroundCheck").transform;
                groundCheckPoint.SetParent(transform);
                groundCheckPoint.localPosition = Vector3.down * (col.height * 0.5f + 0.1f);
            }
        }
        
        void Update()
        {
            if (character != null && character.IsPlayerControlled())
            {
                HandleInput();
            }
            
            CheckGrounded();
            UpdateAnimations();
        }
        
        void FixedUpdate()
        {
            if (isDashing)
            {
                HandleDashing();
            }
            else if (isWallRunning)
            {
                HandleWallRunning();
            }
            else
            {
                HandleMovement();
                HandleJumping();
            }
            
            ApplyGravity();
        }
        
        private void HandleInput()
        {
            // Movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            moveInput = new Vector3(horizontal, 0, vertical).normalized;
            
            // Transform relative to camera
            if (Camera.main != null)
            {
                Vector3 forward = Camera.main.transform.forward;
                Vector3 right = Camera.main.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
                
                moveInput = forward * vertical + right * horizontal;
            }
            
            // Jump input
            jumpInput = Input.GetButtonDown("Jump");
            
            // Sprint input
            sprintInput = Input.GetButton("Fire3"); // Left Shift
            
            // Dash input
            dashInput = Input.GetKeyDown(KeyCode.LeftControl);
        }
        
        private void CheckGrounded()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckDistance, groundLayers);
            
            // Reset air dash count when landing
            if (!wasGrounded && isGrounded)
            {
                airDashCount = 0;
                
                // Landing effect
                if (rb.velocity.y < -5f)
                {
                    CreateLandingEffect();
                }
            }
        }
        
        private void HandleMovement()
        {
            if (moveInput.magnitude > 0.1f)
            {
                // Calculate movement speed
                float currentSpeed = baseRunSpeed;
                
                // Sprint modifier
                if (sprintInput && isGrounded)
                {
                    currentSpeed *= sprintMultiplier;
                    isSprinting = true;
                    
                    // Consume Nen for enhanced sprinting
                    if (nenController != null && nenController.ConsumeNen(2f * Time.fixedDeltaTime))
                    {
                        currentSpeed *= nenSpeedBoost;
                        ShowNenTrail();
                    }
                }
                else
                {
                    isSprinting = false;
                }
                
                // Apply movement
                Vector3 targetVelocity = moveInput * currentSpeed;
                
                if (isGrounded)
                {
                    // Ground movement
                    rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
                }
                else
                {
                    // Air control
                    Vector3 velocityChange = (targetVelocity - new Vector3(rb.velocity.x, 0, rb.velocity.z)) * airControl;
                    rb.AddForce(velocityChange, ForceMode.VelocityChange);
                }
                
                // Rotate character to face movement direction
                if (targetVelocity.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetVelocity);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                }
            }
            else
            {
                isSprinting = false;
                HideNenTrail();
                
                // Apply friction when not moving
                if (isGrounded)
                {
                    Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.fixedDeltaTime * 8f);
                    rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
                }
            }
            
            // Handle dashing
            if (dashInput && canDash && (isGrounded || airDashCount < maxAirDashes))
            {
                StartDash();
            }
        }
        
        private void HandleJumping()
        {
            if (jumpInput)
            {
                if (isGrounded)
                {
                    // Ground jump
                    PerformJump(jumpPower);
                }
                else if (isWallRunning)
                {
                    // Wall jump
                    PerformWallJump();
                }
            }
        }
        
        private void PerformJump(float power)
        {
            // Enhanced jump with Nen
            if (nenController != null && nenController.ConsumeNen(5f))
            {
                power *= nenJumpBoost;
                CreateJumpEffect();
            }
            
            rb.velocity = new Vector3(rb.velocity.x, power, rb.velocity.z);
            
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
        
        private void StartDash()
        {
            if (nenController != null && nenController.ConsumeNen(10f))
            {
                isDashing = true;
                canDash = false;
                dashTimer = 0f;
                
                // Determine dash direction
                if (moveInput.magnitude > 0.1f)
                {
                    dashDirection = moveInput.normalized;
                }
                else
                {
                    dashDirection = transform.forward;
                }
                
                // Count air dash
                if (!isGrounded)
                {
                    airDashCount++;
                }
                
                // Visual effects
                if (dashEffect != null)
                {
                    dashEffect.Play();
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("Dash");
                }
                
                StartCoroutine(DashCooldown());
            }
        }
        
        private void HandleDashing()
        {
            dashTimer += Time.fixedDeltaTime;
            
            if (dashTimer < dashDuration)
            {
                // Move in dash direction
                float dashSpeed = dashDistance / dashDuration;
                rb.velocity = dashDirection * dashSpeed;
            }
            else
            {
                isDashing = false;
                
                if (dashEffect != null)
                {
                    dashEffect.Stop();
                }
            }
        }
        
        private IEnumerator DashCooldown()
        {
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
        
        private void HandleWallRunning()
        {
            if (!canWallRun) return;
            
            wallRunTimer += Time.fixedDeltaTime;
            
            if (wallRunTimer < wallRunDuration && CheckWallContact())
            {
                // Apply wall run movement
                Vector3 wallRunDirection = Vector3.Cross(wallNormal, Vector3.up);
                if (isWallRunningLeft)
                    wallRunDirection = -wallRunDirection;
                
                rb.velocity = new Vector3(wallRunDirection.x * wallRunSpeed, -wallRunGravity, wallRunDirection.z * wallRunSpeed);
                
                // Tilt character during wall run
                Vector3 tiltDirection = isWallRunningLeft ? Vector3.forward : Vector3.back;
                Quaternion targetRotation = Quaternion.LookRotation(wallRunDirection, wallNormal) * Quaternion.Euler(0, 0, isWallRunningLeft ? -15f : 15f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
            }
            else
            {
                ExitWallRun();
            }
        }
        
        private bool CheckWallContact()
        {
            // Check for wall contact
            RaycastHit hit;
            Vector3 rayDirection = isWallRunningLeft ? -transform.right : transform.right;
            
            if (Physics.Raycast(transform.position, rayDirection, out hit, 1.5f, wallLayers))
            {
                wallNormal = hit.normal;
                return true;
            }
            
            return false;
        }
        
        private void StartWallRun(Vector3 normal, bool isLeft)
        {
            if (!isGrounded && rb.velocity.y < 0 && nenController != null && nenController.ConsumeNen(8f))
            {
                isWallRunning = true;
                wallNormal = normal;
                isWallRunningLeft = isLeft;
                wallRunTimer = 0f;
                
                if (animator != null)
                {
                    animator.SetBool("WallRunning", true);
                }
            }
        }
        
        private void ExitWallRun()
        {
            isWallRunning = false;
            wallRunTimer = 0f;
            
            if (animator != null)
            {
                animator.SetBool("WallRunning", false);
            }
        }
        
        private void PerformWallJump()
        {
            if (isWallRunning && nenController != null && nenController.ConsumeNen(8f))
            {
                // Jump away from wall
                Vector3 jumpDirection = (wallNormal + Vector3.up).normalized;
                rb.velocity = jumpDirection * wallJumpForce;
                
                ExitWallRun();
                CreateJumpEffect();
                
                if (animator != null)
                {
                    animator.SetTrigger("WallJump");
                }
            }
        }
        
        private void ApplyGravity()
        {
            if (!isGrounded && !isWallRunning && !isDashing)
            {
                rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
            }
        }
        
        void OnCollisionEnter(Collision collision)
        {
            // Check for wall running opportunities
            if (!isGrounded && canWallRun && collision.gameObject.layer == Mathf.Log(wallLayers.value, 2))
            {
                Vector3 wallNormal = collision.contacts[0].normal;
                
                // Determine if wall is to the left or right
                Vector3 wallDirection = Vector3.Cross(wallNormal, Vector3.up);
                bool isLeft = Vector3.Dot(transform.right, wallDirection) < 0;
                
                StartWallRun(wallNormal, isLeft);
            }
        }
        
        private void ShowNenTrail()
        {
            if (nenTrailEffect != null && !nenTrailEffect.isPlaying)
            {
                var main = nenTrailEffect.main;
                main.startColor = nenTrailColor;
                nenTrailEffect.Play();
            }
        }
        
        private void HideNenTrail()
        {
            if (nenTrailEffect != null && nenTrailEffect.isPlaying)
            {
                nenTrailEffect.Stop();
            }
        }
        
        private void CreateJumpEffect()
        {
            // Create jump burst effect at ground position
            Vector3 effectPosition = groundCheckPoint.position;
            
            // TODO: Instantiate jump effect prefab
            Debug.Log("Jump effect at " + effectPosition);
        }
        
        private void CreateLandingEffect()
        {
            // Create landing impact effect
            Vector3 effectPosition = groundCheckPoint.position;
            
            // TODO: Instantiate landing effect prefab
            Debug.Log("Landing effect at " + effectPosition);
        }
        
        private void UpdateAnimations()
        {
            if (animator != null)
            {
                // Speed parameters
                float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
                animator.SetFloat("Speed", speed);
                animator.SetFloat("VerticalSpeed", rb.velocity.y);
                
                // State parameters
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("IsSprinting", isSprinting);
                animator.SetBool("IsWallRunning", isWallRunning);
                animator.SetBool("IsDashing", isDashing);
                
                // Movement direction
                Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
                animator.SetFloat("Horizontal", localVelocity.x);
                animator.SetFloat("Vertical", localVelocity.z);
            }
        }
        
        // Public methods for external control
        public void SetMovementEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
        
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Impulse)
        {
            rb.AddForce(force, mode);
        }
        
        public void SetVelocity(Vector3 velocity)
        {
            rb.velocity = velocity;
        }
        
        public Vector3 GetVelocity()
        {
            return rb.velocity;
        }
        
        public bool IsMoving()
        {
            return rb.velocity.magnitude > 0.1f;
        }
        
        public bool IsGrounded()
        {
            return isGrounded;
        }
        
        public bool IsWallRunning()
        {
            return isWallRunning;
        }
        
        public bool IsDashing()
        {
            return isDashing;
        }
    }
}