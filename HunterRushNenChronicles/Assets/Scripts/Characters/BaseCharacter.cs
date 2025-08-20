using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HunterRush.Characters
{
    /// <summary>
    /// Base class for all playable characters in Hunter Rush: Nen Chronicles
    /// Handles common functionality like movement, health, Nen, and animations
    /// </summary>
    public abstract class BaseCharacter : MonoBehaviour
    {
        [Header("Character Info")]
        public string characterName;
        public CharacterType characterType;
        public GameObject characterModel;
        public Animator animator;
        
        [Header("Stats")]
        public float maxHealth = 100f;
        public float currentHealth;
        public float maxNenCapacity = 100f;
        public float currentNen;
        public float nenRegenRate = 5f;
        
        [Header("Movement")]
        public float runSpeed = 10f;
        public float jumpPower = 15f;
        public float wallRunDuration = 3f;
        public float dashDistance = 5f;
        public float dashCooldown = 1f;
        
        [Header("Combat")]
        public float attackDamage = 25f;
        public float attackRange = 2f;
        public float attackCooldown = 0.5f;
        public float comboWindow = 1f;
        
        [Header("Nen Abilities")]
        public Color auraColor = Color.white;
        public ParticleSystem auraEffect;
        public float tenDefenseMultiplier = 0.5f;
        public float renAttackMultiplier = 1.5f;
        
        // Components
        protected Rigidbody rb;
        protected CapsuleCollider col;
        protected MovementController movement;
        protected CombatController combat;
        protected NenController nen;
        protected AudioSource audioSource;
        
        // State
        public CharacterState currentState = CharacterState.Idle;
        public NenState currentNenState = NenState.Normal;
        protected bool isGrounded = true;
        protected bool canAttack = true;
        protected bool canDash = true;
        protected int comboCount = 0;
        protected float lastAttackTime;
        
        // Events
        public System.Action<float> OnHealthChanged;
        public System.Action<float> OnNenChanged;
        public System.Action<CharacterState> OnStateChanged;
        public System.Action OnCharacterDeath;
        
        protected virtual void Awake()
        {
            // Get components
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();
            movement = GetComponent<MovementController>();
            combat = GetComponent<CombatController>();
            nen = GetComponent<NenController>();
            audioSource = GetComponent<AudioSource>();
            
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
        
        protected virtual void Start()
        {
            // Initialize stats
            currentHealth = maxHealth;
            currentNen = maxNenCapacity;
            
            // Setup aura effect
            if (auraEffect != null)
            {
                var main = auraEffect.main;
                main.startColor = auraColor;
            }
            
            // Initialize character-specific setup
            InitializeCharacter();
        }
        
        protected virtual void Update()
        {
            // Regenerate Nen
            RegenerateNen();
            
            // Update state machine
            UpdateState();
            
            // Handle input (if this is the player character)
            if (IsPlayerControlled())
            {
                HandleInput();
            }
        }
        
        protected virtual void RegenerateNen()
        {
            if (currentNen < maxNenCapacity && currentNenState != NenState.Zetsu)
            {
                currentNen = Mathf.Min(maxNenCapacity, currentNen + nenRegenRate * Time.deltaTime);
                OnNenChanged?.Invoke(currentNen);
            }
        }
        
        protected virtual void UpdateState()
        {
            // Update animator parameters
            if (animator != null)
            {
                animator.SetFloat("Speed", rb.velocity.magnitude);
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetInteger("State", (int)currentState);
                animator.SetInteger("NenState", (int)currentNenState);
            }
        }
        
        protected virtual void HandleInput()
        {
            // Base input handling - override in derived classes for character-specific controls
        }
        
        public virtual void TakeDamage(float damage, Vector3 knockback = default)
        {
            // Apply Ten defense if active
            if (currentNenState == NenState.Ten)
            {
                damage *= tenDefenseMultiplier;
            }
            
            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth);
            
            // Apply knockback
            if (knockback != Vector3.zero)
            {
                rb.AddForce(knockback, ForceMode.Impulse);
            }
            
            // Trigger damage animation
            if (animator != null)
            {
                animator.SetTrigger("TakeDamage");
            }
            
            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        public virtual void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth);
        }
        
        public virtual bool ConsumeNen(float amount)
        {
            if (currentNen >= amount)
            {
                currentNen -= amount;
                OnNenChanged?.Invoke(currentNen);
                return true;
            }
            return false;
        }
        
        public virtual void ChangeNenState(NenState newState)
        {
            currentNenState = newState;
            
            // Apply state effects
            switch (newState)
            {
                case NenState.Ten:
                    ShowAuraEffect(true);
                    break;
                case NenState.Ren:
                    ShowAuraEffect(true);
                    break;
                case NenState.Zetsu:
                    ShowAuraEffect(false);
                    break;
                default:
                    ShowAuraEffect(false);
                    break;
            }
        }
        
        protected virtual void ShowAuraEffect(bool show)
        {
            if (auraEffect != null)
            {
                if (show)
                    auraEffect.Play();
                else
                    auraEffect.Stop();
            }
        }
        
        protected virtual void Die()
        {
            ChangeState(CharacterState.Dead);
            OnCharacterDeath?.Invoke();
            
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
        }
        
        public virtual void ChangeState(CharacterState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnStateChanged?.Invoke(newState);
            }
        }
        
        protected virtual bool IsPlayerControlled()
        {
            return GameManager.Instance != null && GameManager.Instance.selectedCharacter == characterType;
        }
        
        // Abstract methods to be implemented by specific characters
        protected abstract void InitializeCharacter();
        public abstract void PerformSpecialAbility();
        public abstract void PerformHatsuAbility();
        
        // Character-specific attack methods
        public abstract void LightAttack();
        public abstract void HeavyAttack();
        public abstract void ComboAttack(int comboIndex);
        
        // Voice line methods
        public abstract void PlayBattleCry();
        public abstract void PlayVictoryLine();
        public abstract void PlayDeathLine();
    }
    
    public enum CharacterState
    {
        Idle,
        Running,
        Jumping,
        WallRunning,
        Attacking,
        Dashing,
        TakingDamage,
        Dead,
        UsingAbility
    }
    
    public enum NenState
    {
        Normal,
        Ten,
        Zetsu,
        Ren,
        Hatsu
    }
}