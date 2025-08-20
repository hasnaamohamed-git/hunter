using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HunterRush.Combat
{
    /// <summary>
    /// Advanced combat system with anime-accurate Hunter x Hunter combat mechanics
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("Combat Settings")]
        public float attackRange = 2f;
        public float attackAngle = 60f;
        public float comboWindow = 1.5f;
        public int maxComboCount = 4;
        public LayerMask enemyLayers = 1;
        
        [Header("Attack Types")]
        public AttackData lightAttack;
        public AttackData heavyAttack;
        public AttackData[] comboAttacks;
        
        [Header("Defensive Options")]
        public float blockReduction = 0.7f;
        public float dodgeDistance = 3f;
        public float dodgeDuration = 0.5f;
        public float counterWindow = 0.3f;
        
        [Header("Visual Effects")]
        public GameObject hitEffectPrefab;
        public GameObject blockEffectPrefab;
        public GameObject counterEffectPrefab;
        public ParticleSystem combatAura;
        
        [Header("Audio")]
        public AudioClip[] attackSounds;
        public AudioClip[] hitSounds;
        public AudioClip[] blockSounds;
        public AudioClip[] comboSounds;
        
        // Components
        private BaseCharacter character;
        private Rigidbody rb;
        private Animator animator;
        private AudioSource audioSource;
        
        // Combat state
        private bool isAttacking = false;
        private bool isBlocking = false;
        private bool isDodging = false;
        private bool canAttack = true;
        private bool canBlock = true;
        private bool canDodge = true;
        private bool inCounterWindow = false;
        
        // Combo system
        private int currentComboCount = 0;
        private float lastAttackTime = 0f;
        private Queue<AttackInput> attackQueue = new Queue<AttackInput>();
        
        // Target system
        private Transform currentTarget;
        private List<Transform> nearbyEnemies = new List<Transform>();
        
        void Awake()
        {
            character = GetComponent<BaseCharacter>();
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }
        
        void Update()
        {
            if (character != null && character.IsPlayerControlled())
            {
                HandleCombatInput();
            }
            
            UpdateCombatState();
            UpdateTargeting();
        }
        
        private void HandleCombatInput()
        {
            // Light attack
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                QueueAttack(AttackType.Light);
            }
            
            // Heavy attack
            if (Input.GetMouseButtonDown(1)) // Right click
            {
                QueueAttack(AttackType.Heavy);
            }
            
            // Block
            if (Input.GetButton("Fire2")) // Right mouse hold
            {
                StartBlocking();
            }
            else
            {
                StopBlocking();
            }
            
            // Dodge
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PerformDodge();
            }
            
            // Target lock
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleTarget();
            }
        }
        
        private void UpdateCombatState()
        {
            // Process attack queue
            if (attackQueue.Count > 0 && canAttack && !isAttacking)
            {
                ProcessNextAttack();
            }
            
            // Check combo window
            if (Time.time - lastAttackTime > comboWindow && currentComboCount > 0)
            {
                ResetCombo();
            }
        }
        
        private void UpdateTargeting()
        {
            // Find nearby enemies
            nearbyEnemies.Clear();
            Collider[] enemies = Physics.OverlapSphere(transform.position, 10f, enemyLayers);
            
            foreach (Collider enemy in enemies)
            {
                if (enemy.transform != transform)
                {
                    nearbyEnemies.Add(enemy.transform);
                }
            }
            
            // Auto-target closest enemy if no target
            if (currentTarget == null && nearbyEnemies.Count > 0)
            {
                currentTarget = GetClosestEnemy();
            }
            
            // Face target during combat
            if (currentTarget != null && (isAttacking || isBlocking))
            {
                FaceTarget(currentTarget);
            }
        }
        
        private void QueueAttack(AttackType type)
        {
            AttackInput input = new AttackInput
            {
                type = type,
                inputTime = Time.time
            };
            
            attackQueue.Enqueue(input);
        }
        
        private void ProcessNextAttack()
        {
            if (attackQueue.Count == 0) return;
            
            AttackInput nextAttack = attackQueue.Dequeue();
            
            switch (nextAttack.type)
            {
                case AttackType.Light:
                    PerformLightAttack();
                    break;
                case AttackType.Heavy:
                    PerformHeavyAttack();
                    break;
                case AttackType.Combo:
                    PerformComboAttack();
                    break;
            }
        }
        
        private void PerformLightAttack()
        {
            if (!canAttack) return;
            
            StartCoroutine(ExecuteAttack(lightAttack));
            
            // Check for combo
            if (currentComboCount < maxComboCount && Time.time - lastAttackTime < comboWindow)
            {
                currentComboCount++;
            }
            else
            {
                currentComboCount = 1;
            }
            
            lastAttackTime = Time.time;
        }
        
        private void PerformHeavyAttack()
        {
            if (!canAttack) return;
            
            StartCoroutine(ExecuteAttack(heavyAttack));
            
            // Heavy attacks reset combo but can start new ones
            currentComboCount = 1;
            lastAttackTime = Time.time;
        }
        
        private void PerformComboAttack()
        {
            if (!canAttack || currentComboCount <= 0) return;
            
            int comboIndex = Mathf.Min(currentComboCount - 1, comboAttacks.Length - 1);
            AttackData comboData = comboAttacks[comboIndex];
            
            StartCoroutine(ExecuteAttack(comboData));
            
            currentComboCount++;
            lastAttackTime = Time.time;
            
            // Play combo sound
            PlayRandomSound(comboSounds);
        }
        
        private IEnumerator ExecuteAttack(AttackData attack)
        {
            isAttacking = true;
            canAttack = false;
            
            // Set animation
            if (animator != null && !string.IsNullOrEmpty(attack.animationTrigger))
            {
                animator.SetTrigger(attack.animationTrigger);
            }
            
            // Play attack sound
            PlayRandomSound(attackSounds);
            
            // Wait for attack startup
            yield return new WaitForSeconds(attack.startupTime);
            
            // Execute attack hitbox
            PerformAttackHitbox(attack);
            
            // Wait for attack recovery
            yield return new WaitForSeconds(attack.recoveryTime);
            
            isAttacking = false;
            canAttack = true;
        }
        
        private void PerformAttackHitbox(AttackData attack)
        {
            Vector3 attackPosition = transform.position + transform.forward * (attackRange * 0.5f);
            Collider[] hits = Physics.OverlapSphere(attackPosition, attack.range, enemyLayers);
            
            foreach (Collider hit in hits)
            {
                // Check if target is within attack angle
                Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);
                
                if (angle <= attackAngle * 0.5f)
                {
                    BaseCharacter target = hit.GetComponent<BaseCharacter>();
                    if (target != null)
                    {
                        DealDamageToTarget(target, attack);
                    }
                }
            }
        }
        
        private void DealDamageToTarget(BaseCharacter target, AttackData attack)
        {
            float damage = attack.damage;
            
            // Apply character's attack modifiers
            if (character != null)
            {
                damage *= character.attackDamage / 25f; // Normalize to base attack
            }
            
            // Apply Ren boost if active
            if (character.currentNenState == NenState.Ren)
            {
                damage *= character.renAttackMultiplier;
            }
            
            // Calculate knockback
            Vector3 knockback = Vector3.zero;
            if (attack.knockbackForce > 0)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                knockback = direction * attack.knockbackForce;
            }
            
            // Deal damage
            target.TakeDamage(damage, knockback);
            
            // Create hit effect
            CreateHitEffect(target.transform.position);
            
            // Play hit sound
            PlayRandomSound(hitSounds);
            
            // Camera shake
            if (Camera.main != null)
            {
                StartCoroutine(CameraShake(0.1f, 0.3f));
            }
        }
        
        private void StartBlocking()
        {
            if (!canBlock || isAttacking) return;
            
            isBlocking = true;
            
            if (animator != null)
            {
                animator.SetBool("Blocking", true);
            }
            
            // Start counter window
            if (!inCounterWindow)
            {
                StartCoroutine(CounterWindowCoroutine());
            }
        }
        
        private void StopBlocking()
        {
            isBlocking = false;
            
            if (animator != null)
            {
                animator.SetBool("Blocking", false);
            }
        }
        
        private IEnumerator CounterWindowCoroutine()
        {
            inCounterWindow = true;
            yield return new WaitForSeconds(counterWindow);
            inCounterWindow = false;
        }
        
        public void OnTakeDamage(float damage, Vector3 knockback, BaseCharacter attacker)
        {
            if (isBlocking && attacker != null)
            {
                // Reduce damage when blocking
                damage *= blockReduction;
                
                // Create block effect
                CreateBlockEffect(transform.position);
                PlayRandomSound(blockSounds);
                
                // Counter attack opportunity
                if (inCounterWindow)
                {
                    PerformCounterAttack(attacker);
                }
            }
        }
        
        private void PerformCounterAttack(BaseCharacter attacker)
        {
            if (attacker == null) return;
            
            // Quick counter attack
            StartCoroutine(ExecuteCounterAttack(attacker));
        }
        
        private IEnumerator ExecuteCounterAttack(BaseCharacter target)
        {
            // Face the attacker
            FaceTarget(target.transform);
            
            // Create counter effect
            CreateCounterEffect(transform.position);
            
            if (animator != null)
            {
                animator.SetTrigger("Counter");
            }
            
            yield return new WaitForSeconds(0.2f);
            
            // Deal counter damage
            float counterDamage = character.attackDamage * 1.5f;
            Vector3 counterKnockback = (target.transform.position - transform.position).normalized * 8f;
            target.TakeDamage(counterDamage, counterKnockback);
        }
        
        private void PerformDodge()
        {
            if (!canDodge || isAttacking) return;
            
            StartCoroutine(ExecuteDodge());
        }
        
        private IEnumerator ExecuteDodge()
        {
            isDodging = true;
            canDodge = false;
            
            // Determine dodge direction
            Vector3 dodgeDirection = -transform.forward; // Dodge backward by default
            
            // Check for input direction
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
            {
                Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
                dodgeDirection = transform.TransformDirection(inputDirection);
            }
            
            // Apply dodge movement
            rb.AddForce(dodgeDirection * dodgeDistance, ForceMode.VelocityChange);
            
            // Set animation
            if (animator != null)
            {
                animator.SetTrigger("Dodge");
            }
            
            // Invincibility frames
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
            
            yield return new WaitForSeconds(dodgeDuration);
            
            // Remove invincibility
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
            
            isDodging = false;
            
            // Cooldown
            yield return new WaitForSeconds(1f);
            canDodge = true;
        }
        
        private void CycleTarget()
        {
            if (nearbyEnemies.Count == 0)
            {
                currentTarget = null;
                return;
            }
            
            int currentIndex = nearbyEnemies.IndexOf(currentTarget);
            int nextIndex = (currentIndex + 1) % nearbyEnemies.Count;
            currentTarget = nearbyEnemies[nextIndex];
        }
        
        private Transform GetClosestEnemy()
        {
            if (nearbyEnemies.Count == 0) return null;
            
            Transform closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (Transform enemy in nearbyEnemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }
            
            return closest;
        }
        
        private void FaceTarget(Transform target)
        {
            if (target == null) return;
            
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // Keep on horizontal plane
            
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        
        private void ResetCombo()
        {
            currentComboCount = 0;
            lastAttackTime = 0f;
        }
        
        private void CreateHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        private void CreateBlockEffect(Vector3 position)
        {
            if (blockEffectPrefab != null)
            {
                GameObject effect = Instantiate(blockEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
        
        private void CreateCounterEffect(Vector3 position)
        {
            if (counterEffectPrefab != null)
            {
                GameObject effect = Instantiate(counterEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 1.5f);
            }
        }
        
        private IEnumerator CameraShake(float duration, float magnitude)
        {
            Vector3 originalPosition = Camera.main.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                
                Camera.main.transform.localPosition = originalPosition + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Camera.main.transform.localPosition = originalPosition;
        }
        
        private void PlayRandomSound(AudioClip[] sounds)
        {
            if (audioSource != null && sounds.Length > 0)
            {
                AudioClip clip = sounds[Random.Range(0, sounds.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
        
        // Public methods
        public bool IsAttacking() => isAttacking;
        public bool IsBlocking() => isBlocking;
        public bool IsDodging() => isDodging;
        public int GetComboCount() => currentComboCount;
        public Transform GetCurrentTarget() => currentTarget;
        
        public void SetCombatEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
        
        public void ForceStopAttack()
        {
            StopAllCoroutines();
            isAttacking = false;
            canAttack = true;
            ResetCombo();
        }
    }
    
    [System.Serializable]
    public class AttackData
    {
        public string name;
        public float damage = 25f;
        public float range = 2f;
        public float knockbackForce = 5f;
        public float startupTime = 0.2f;
        public float recoveryTime = 0.5f;
        public string animationTrigger;
        public AttackType type;
    }
    
    public enum AttackType
    {
        Light,
        Heavy,
        Combo,
        Special
    }
    
    private struct AttackInput
    {
        public AttackType type;
        public float inputTime;
    }
}