using UnityEngine;
using System.Collections;

namespace HunterRush.Characters
{
    /// <summary>
    /// Gon Freecss character implementation with Jajanken abilities and enhanced physical prowess
    /// </summary>
    public class GonFreecss : BaseCharacter
    {
        [Header("Gon Specific Settings")]
        [SerializeField] private float jajankenChargeTime = 2f;
        [SerializeField] private float rockDamageMultiplier = 3f;
        [SerializeField] private float paperRange = 10f;
        [SerializeField] private float scissorsBladeLength = 3f;
        [SerializeField] private GameObject rockEffectPrefab;
        [SerializeField] private GameObject paperEffectPrefab;
        [SerializeField] private GameObject scissorsEffectPrefab;
        [SerializeField] private ParticleSystem enhancementAura;
        
        [Header("Voice Lines")]
        [SerializeField] private AudioClip[] battleCries;
        [SerializeField] private AudioClip[] jajankenLines;
        [SerializeField] private AudioClip[] victoryLines;
        [SerializeField] private AudioClip[] determinationLines;
        
        // Jajanken state
        private bool isChargingJajanken = false;
        private float jajankenChargeProgress = 0f;
        private JajankenType selectedJajanken = JajankenType.Rock;
        
        // Enhanced abilities
        private bool isEnhancementActive = false;
        private float enhancementDuration = 10f;
        private float enhancementCooldown = 30f;
        private bool canUseEnhancement = true;
        
        protected override void InitializeCharacter()
        {
            characterName = "Gon Freecss";
            characterType = CharacterType.Gon;
            auraColor = Color.green;
            
            // Gon's stats - high physical strength and agility
            runSpeed = 12f;
            jumpPower = 18f;
            attackDamage = 30f;
            maxHealth = 120f;
            maxNenCapacity = 80f; // Lower Nen capacity but high physical ability
            
            // Set current stats
            currentHealth = maxHealth;
            currentNen = maxNenCapacity;
        }
        
        protected override void HandleInput()
        {
            base.HandleInput();
            
            // Jajanken input (hold for charge)
            if (Input.GetKeyDown(KeyCode.Q) && canAttack)
            {
                StartJajankenCharge();
            }
            
            if (Input.GetKey(KeyCode.Q) && isChargingJajanken)
            {
                ContinueJajankenCharge();
            }
            
            if (Input.GetKeyUp(KeyCode.Q) && isChargingJajanken)
            {
                ReleaseJajanken();
            }
            
            // Jajanken type selection
            if (Input.GetKeyDown(KeyCode.Alpha1))
                selectedJajanken = JajankenType.Rock;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                selectedJajanken = JajankenType.Paper;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                selectedJajanken = JajankenType.Scissors;
            
            // Enhancement ability
            if (Input.GetKeyDown(KeyCode.E) && canUseEnhancement)
            {
                ActivateEnhancement();
            }
        }
        
        private void StartJajankenCharge()
        {
            if (ConsumeNen(10f)) // Base Nen cost
            {
                isChargingJajanken = true;
                jajankenChargeProgress = 0f;
                ChangeState(CharacterState.UsingAbility);
                
                // Play charging animation
                if (animator != null)
                {
                    animator.SetBool("ChargingJajanken", true);
                    animator.SetInteger("JajankenType", (int)selectedJajanken);
                }
                
                // Play voice line
                PlayRandomAudioClip(jajankenLines);
            }
        }
        
        private void ContinueJajankenCharge()
        {
            jajankenChargeProgress += Time.deltaTime / jajankenChargeTime;
            jajankenChargeProgress = Mathf.Clamp01(jajankenChargeProgress);
            
            // Visual feedback for charging
            if (enhancementAura != null)
            {
                var emission = enhancementAura.emission;
                emission.rateOverTime = jajankenChargeProgress * 50f;
            }
            
            // Consume additional Nen while charging
            ConsumeNen(5f * Time.deltaTime);
        }
        
        private void ReleaseJajanken()
        {
            isChargingJajanken = false;
            
            if (animator != null)
            {
                animator.SetBool("ChargingJajanken", false);
                animator.SetTrigger("ReleaseJajanken");
            }
            
            // Execute Jajanken based on type and charge level
            float damageMultiplier = 1f + (jajankenChargeProgress * 2f);
            
            switch (selectedJajanken)
            {
                case JajankenType.Rock:
                    PerformJajankenRock(damageMultiplier);
                    break;
                case JajankenType.Paper:
                    PerformJajankenPaper(damageMultiplier);
                    break;
                case JajankenType.Scissors:
                    PerformJajankenScissors(damageMultiplier);
                    break;
            }
            
            ChangeState(CharacterState.Idle);
            jajankenChargeProgress = 0f;
        }
        
        private void PerformJajankenRock(float damageMultiplier)
        {
            float damage = attackDamage * rockDamageMultiplier * damageMultiplier;
            
            // Create rock effect
            if (rockEffectPrefab != null)
            {
                Vector3 effectPos = transform.position + transform.forward * 1.5f;
                GameObject effect = Instantiate(rockEffectPrefab, effectPos, transform.rotation);
                Destroy(effect, 2f);
            }
            
            // Damage enemies in front
            Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 2f, 2f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    var enemyChar = enemy.GetComponent<BaseCharacter>();
                    if (enemyChar != null)
                    {
                        Vector3 knockback = (enemy.transform.position - transform.position).normalized * 10f;
                        enemyChar.TakeDamage(damage, knockback);
                    }
                }
            }
            
            Debug.Log($"Jajanken Rock! Damage: {damage}");
        }
        
        private void PerformJajankenPaper(float damageMultiplier)
        {
            float damage = attackDamage * damageMultiplier;
            
            // Create paper projectile effect
            if (paperEffectPrefab != null)
            {
                Vector3 startPos = transform.position + Vector3.up * 1.5f;
                GameObject projectile = Instantiate(paperEffectPrefab, startPos, transform.rotation);
                
                // Add projectile movement
                Rigidbody projRb = projectile.GetComponent<Rigidbody>();
                if (projRb != null)
                {
                    projRb.velocity = transform.forward * 20f;
                }
                
                // Add damage component
                ProjectileDamage projDamage = projectile.GetComponent<ProjectileDamage>();
                if (projDamage != null)
                {
                    projDamage.damage = damage;
                    projDamage.range = paperRange;
                }
                
                Destroy(projectile, 3f);
            }
            
            Debug.Log($"Jajanken Paper! Damage: {damage}, Range: {paperRange}");
        }
        
        private void PerformJajankenScissors(float damageMultiplier)
        {
            float damage = attackDamage * damageMultiplier;
            
            // Create scissors blade effect
            if (scissorsEffectPrefab != null)
            {
                Vector3 effectPos = transform.position + transform.forward * 1f;
                GameObject effect = Instantiate(scissorsEffectPrefab, effectPos, transform.rotation);
                effect.transform.localScale = Vector3.one * scissorsBladeLength;
                Destroy(effect, 1f);
            }
            
            // Line damage in front of character
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, transform.forward, scissorsBladeLength);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    var enemyChar = hit.collider.GetComponent<BaseCharacter>();
                    if (enemyChar != null)
                    {
                        enemyChar.TakeDamage(damage);
                    }
                }
            }
            
            Debug.Log($"Jajanken Scissors! Damage: {damage}, Length: {scissorsBladeLength}");
        }
        
        private void ActivateEnhancement()
        {
            if (!isEnhancementActive && ConsumeNen(30f))
            {
                StartCoroutine(EnhancementCoroutine());
            }
        }
        
        private IEnumerator EnhancementCoroutine()
        {
            isEnhancementActive = true;
            canUseEnhancement = false;
            
            // Boost stats
            float originalSpeed = runSpeed;
            float originalJump = jumpPower;
            float originalAttack = attackDamage;
            
            runSpeed *= 1.5f;
            jumpPower *= 1.5f;
            attackDamage *= 1.3f;
            
            // Visual effects
            if (enhancementAura != null)
                enhancementAura.Play();
            
            // Play enhancement voice line
            PlayRandomAudioClip(determinationLines);
            
            yield return new WaitForSeconds(enhancementDuration);
            
            // Restore original stats
            runSpeed = originalSpeed;
            jumpPower = originalJump;
            attackDamage = originalAttack;
            
            // Stop effects
            if (enhancementAura != null)
                enhancementAura.Stop();
            
            isEnhancementActive = false;
            
            // Start cooldown
            yield return new WaitForSeconds(enhancementCooldown);
            canUseEnhancement = true;
        }
        
        public override void PerformSpecialAbility()
        {
            if (canUseEnhancement)
            {
                ActivateEnhancement();
            }
        }
        
        public override void PerformHatsuAbility()
        {
            if (!isChargingJajanken)
            {
                StartJajankenCharge();
            }
        }
        
        public override void LightAttack()
        {
            if (canAttack && ConsumeNen(5f))
            {
                canAttack = false;
                ChangeState(CharacterState.Attacking);
                
                if (animator != null)
                {
                    animator.SetTrigger("LightAttack");
                }
                
                // Basic punch attack
                Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 1.5f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            float damage = attackDamage * (isEnhancementActive ? 1.3f : 1f);
                            enemyChar.TakeDamage(damage);
                        }
                    }
                }
                
                StartCoroutine(AttackCooldown());
            }
        }
        
        public override void HeavyAttack()
        {
            if (canAttack && ConsumeNen(15f))
            {
                canAttack = false;
                ChangeState(CharacterState.Attacking);
                
                if (animator != null)
                {
                    animator.SetTrigger("HeavyAttack");
                }
                
                // Enhanced punch with knockback
                Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 2f, 2f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            float damage = attackDamage * 2f * (isEnhancementActive ? 1.3f : 1f);
                            Vector3 knockback = (enemy.transform.position - transform.position).normalized * 8f;
                            enemyChar.TakeDamage(damage, knockback);
                        }
                    }
                }
                
                StartCoroutine(AttackCooldown());
            }
        }
        
        public override void ComboAttack(int comboIndex)
        {
            // Gon's combo system - building up to Jajanken
            switch (comboIndex)
            {
                case 0:
                    LightAttack();
                    break;
                case 1:
                    LightAttack();
                    break;
                case 2:
                    HeavyAttack();
                    break;
                case 3:
                    // Auto-trigger Jajanken Rock as combo finisher
                    selectedJajanken = JajankenType.Rock;
                    StartJajankenCharge();
                    break;
            }
        }
        
        private IEnumerator AttackCooldown()
        {
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
            ChangeState(CharacterState.Idle);
        }
        
        public override void PlayBattleCry()
        {
            PlayRandomAudioClip(battleCries);
        }
        
        public override void PlayVictoryLine()
        {
            PlayRandomAudioClip(victoryLines);
        }
        
        public override void PlayDeathLine()
        {
            if (audioSource != null && determinationLines.Length > 0)
            {
                audioSource.PlayOneShot(determinationLines[0]); // Play a determined "I won't give up" line
            }
        }
        
        private void PlayRandomAudioClip(AudioClip[] clips)
        {
            if (audioSource != null && clips.Length > 0)
            {
                AudioClip randomClip = clips[Random.Range(0, clips.Length)];
                audioSource.PlayOneShot(randomClip);
            }
        }
        
        private enum JajankenType
        {
            Rock = 0,
            Paper = 1,
            Scissors = 2
        }
    }
    
    /// <summary>
    /// Component for projectile damage (used by Jajanken Paper)
    /// </summary>
    public class ProjectileDamage : MonoBehaviour
    {
        public float damage = 25f;
        public float range = 10f;
        private Vector3 startPosition;
        
        void Start()
        {
            startPosition = transform.position;
        }
        
        void Update()
        {
            // Check if projectile has traveled its maximum range
            if (Vector3.Distance(startPosition, transform.position) >= range)
            {
                Destroy(gameObject);
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<BaseCharacter>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
            else if (other.CompareTag("Environment"))
            {
                Destroy(gameObject);
            }
        }
    }
}