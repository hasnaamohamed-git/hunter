using UnityEngine;
using System.Collections;

namespace HunterRush.Characters
{
    /// <summary>
    /// Leorio Paradinight character implementation with emission abilities and medical support
    /// </summary>
    public class LeorioParadinight : BaseCharacter
    {
        [Header("Leorio Specific Settings")]
        [SerializeField] private float remotePunchRange = 15f;
        [SerializeField] private float healingAuraRadius = 8f;
        [SerializeField] private float healingRate = 15f;
        [SerializeField] private float briefcaseSwingDamage = 35f;
        [SerializeField] private GameObject portalPrefab;
        [SerializeField] private GameObject briefcasePrefab;
        [SerializeField] private GameObject healingAuraPrefab;
        [SerializeField] private ParticleSystem medicalAura;
        [SerializeField] private ParticleSystem emissionEffect;
        
        [Header("Voice Lines")]
        [SerializeField] private AudioClip[] doctorLines;
        [SerializeField] private AudioClip[] comedyLines;
        [SerializeField] private AudioClip[] seriousLines;
        [SerializeField] private AudioClip[] supportLines;
        
        // Medical abilities
        private bool isHealingAuraActive = false;
        private float healingAuraDuration = 12f;
        private float healingAuraCooldown = 20f;
        private bool canUseHealingAura = true;
        
        // Emission abilities
        private bool canUseRemotePunch = true;
        private float remotePunchCooldown = 3f;
        private GameObject activePortal;
        
        // Support role
        private System.Collections.Generic.List<BaseCharacter> supportTargets = new System.Collections.Generic.List<BaseCharacter>();
        
        protected override void InitializeCharacter()
        {
            characterName = "Leorio Paradinight";
            characterType = CharacterType.Leorio;
            auraColor = Color.blue;
            
            // Leorio's stats - lower agility but high health and support abilities
            runSpeed = 9f; // Slowest runner
            jumpPower = 12f;
            attackDamage = 20f; // Lower base attack
            maxHealth = 140f; // Highest health
            maxNenCapacity = 100f;
            briefcaseSwingDamage = 35f; // Briefcase hits hard
            
            // Set current stats
            currentHealth = maxHealth;
            currentNen = maxNenCapacity;
        }
        
        protected override void HandleInput()
        {
            base.HandleInput();
            
            // Remote Punch ability
            if (Input.GetMouseButtonDown(1) && canUseRemotePunch) // Right click
            {
                PerformRemotePunch();
            }
            
            // Healing Aura activation
            if (Input.GetKeyDown(KeyCode.Q) && canUseHealingAura)
            {
                ActivateHealingAura();
            }
            
            // Medical examination (reveals enemy info)
            if (Input.GetKeyDown(KeyCode.E))
            {
                PerformMedicalExamination();
            }
            
            // Briefcase throw attack
            if (Input.GetKeyDown(KeyCode.R))
            {
                ThrowBriefcase();
            }
            
            // Emergency heal (self)
            if (Input.GetKeyDown(KeyCode.T) && currentHealth < maxHealth * 0.3f)
            {
                EmergencyHeal();
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            // Update healing aura effect
            if (isHealingAuraActive)
            {
                HealNearbyAllies();
            }
            
            // Update support targets
            UpdateSupportTargets();
        }
        
        private void PerformRemotePunch()
        {
            if (ConsumeNen(20f))
            {
                canUseRemotePunch = false;
                
                // Get target position (closest enemy or cursor position)
                Vector3 targetPosition = GetRemotePunchTarget();
                
                // Create portal effect at target location
                if (portalPrefab != null)
                {
                    activePortal = Instantiate(portalPrefab, targetPosition, Quaternion.identity);
                    StartCoroutine(RemotePunchSequence(targetPosition));
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("RemotePunch");
                }
                
                PlayRandomAudioClip(seriousLines);
            }
        }
        
        private IEnumerator RemotePunchSequence(Vector3 targetPos)
        {
            // Portal opening animation
            yield return new WaitForSeconds(0.5f);
            
            // Damage enemies at target location
            Collider[] enemies = Physics.OverlapSphere(targetPos, 2f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    var enemyChar = enemy.GetComponent<BaseCharacter>();
                    if (enemyChar != null)
                    {
                        float damage = attackDamage * 2.5f; // Remote punch hits harder
                        Vector3 knockback = Vector3.up * 5f; // Uppercut effect
                        enemyChar.TakeDamage(damage, knockback);
                    }
                }
            }
            
            // Create punch impact effect
            if (emissionEffect != null)
            {
                GameObject effect = Instantiate(emissionEffect.gameObject, targetPos, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Clean up portal
            if (activePortal != null)
            {
                Destroy(activePortal, 1f);
            }
            
            // Start cooldown
            yield return new WaitForSeconds(remotePunchCooldown);
            canUseRemotePunch = true;
        }
        
        private Vector3 GetRemotePunchTarget()
        {
            // Find closest enemy within range
            Collider[] enemies = Physics.OverlapSphere(transform.position, remotePunchRange);
            BaseCharacter closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy.GetComponent<BaseCharacter>();
                    }
                }
            }
            
            if (closestEnemy != null)
            {
                return closestEnemy.transform.position;
            }
            else
            {
                // Default to forward position
                return transform.position + transform.forward * 8f;
            }
        }
        
        private void ActivateHealingAura()
        {
            if (!isHealingAuraActive && ConsumeNen(30f))
            {
                StartCoroutine(HealingAuraCoroutine());
            }
        }
        
        private IEnumerator HealingAuraCoroutine()
        {
            isHealingAuraActive = true;
            canUseHealingAura = false;
            
            // Create healing aura visual effect
            if (healingAuraPrefab != null)
            {
                GameObject auraEffect = Instantiate(healingAuraPrefab, transform.position, Quaternion.identity);
                auraEffect.transform.SetParent(transform);
                auraEffect.transform.localScale = Vector3.one * healingAuraRadius;
                
                yield return new WaitForSeconds(healingAuraDuration);
                
                Destroy(auraEffect);
            }
            else
            {
                yield return new WaitForSeconds(healingAuraDuration);
            }
            
            isHealingAuraActive = false;
            
            // Start cooldown
            yield return new WaitForSeconds(healingAuraCooldown);
            canUseHealingAura = true;
        }
        
        private void HealNearbyAllies()
        {
            Collider[] allies = Physics.OverlapSphere(transform.position, healingAuraRadius);
            foreach (Collider ally in allies)
            {
                if (ally.CompareTag("Player") || ally.CompareTag("Ally"))
                {
                    var character = ally.GetComponent<BaseCharacter>();
                    if (character != null)
                    {
                        character.Heal(healingRate * Time.deltaTime);
                        
                        // Visual healing effect
                        if (medicalAura != null && !medicalAura.isPlaying)
                        {
                            medicalAura.transform.position = character.transform.position + Vector3.up * 2f;
                            medicalAura.Play();
                        }
                    }
                }
            }
        }
        
        private void PerformMedicalExamination()
        {
            if (ConsumeNen(15f))
            {
                // Scan area for enemies and reveal their health/status
                Collider[] enemies = Physics.OverlapSphere(transform.position, 10f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            // Reveal enemy health and weaknesses
                            Debug.Log($"Enemy Health: {enemyChar.currentHealth}/{enemyChar.maxHealth}");
                            
                            // Add UI element to show enemy status
                            // TODO: Implement enemy health bar UI
                            
                            // Mark weak points for extra damage
                            StartCoroutine(MarkWeakPoints(enemyChar));
                        }
                    }
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("Examine");
                }
                
                PlayRandomAudioClip(doctorLines);
            }
        }
        
        private IEnumerator MarkWeakPoints(BaseCharacter enemy)
        {
            // Mark enemy for increased damage for 10 seconds
            float originalDamage = attackDamage;
            
            // TODO: Add visual marker on enemy
            
            yield return new WaitForSeconds(10f);
            
            // Remove marker effect
        }
        
        private void ThrowBriefcase()
        {
            if (ConsumeNen(15f))
            {
                if (briefcasePrefab != null)
                {
                    Vector3 throwPosition = transform.position + Vector3.up * 1.5f;
                    Vector3 throwDirection = transform.forward + Vector3.up * 0.3f;
                    
                    GameObject briefcase = Instantiate(briefcasePrefab, throwPosition, transform.rotation);
                    
                    // Add physics to briefcase
                    Rigidbody briefcaseRb = briefcase.GetComponent<Rigidbody>();
                    if (briefcaseRb != null)
                    {
                        briefcaseRb.velocity = throwDirection * 15f;
                        briefcaseRb.angularVelocity = Vector3.right * 10f; // Spinning effect
                    }
                    
                    // Add damage component
                    BriefcaseProjectile projectile = briefcase.GetComponent<BriefcaseProjectile>();
                    if (projectile != null)
                    {
                        projectile.Initialize(briefcaseSwingDamage, this);
                    }
                    
                    // Destroy briefcase after some time
                    Destroy(briefcase, 5f);
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("ThrowBriefcase");
                }
                
                PlayRandomAudioClip(comedyLines);
            }
        }
        
        private void EmergencyHeal()
        {
            if (ConsumeNen(25f))
            {
                // Powerful self-heal when critically injured
                float healAmount = maxHealth * 0.4f;
                Heal(healAmount);
                
                // Temporary damage resistance
                StartCoroutine(EmergencyHealBoost());
                
                if (animator != null)
                {
                    animator.SetTrigger("EmergencyHeal");
                }
                
                PlayRandomAudioClip(doctorLines);
            }
        }
        
        private IEnumerator EmergencyHealBoost()
        {
            // Temporary boost to defense and Nen regen
            float originalNenRegen = nenRegenRate;
            nenRegenRate *= 2f;
            
            yield return new WaitForSeconds(8f);
            
            nenRegenRate = originalNenRegen;
        }
        
        private void UpdateSupportTargets()
        {
            // Remove null references
            supportTargets.RemoveAll(target => target == null);
            
            // Find nearby allies to support
            Collider[] allies = Physics.OverlapSphere(transform.position, 15f);
            foreach (Collider ally in allies)
            {
                if (ally.CompareTag("Player") || ally.CompareTag("Ally"))
                {
                    var character = ally.GetComponent<BaseCharacter>();
                    if (character != null && character != this && !supportTargets.Contains(character))
                    {
                        if (character.currentHealth < character.maxHealth * 0.5f)
                        {
                            supportTargets.Add(character);
                        }
                    }
                }
            }
        }
        
        public override void PerformSpecialAbility()
        {
            if (canUseHealingAura)
            {
                ActivateHealingAura();
            }
            else if (canUseRemotePunch)
            {
                PerformRemotePunch();
            }
        }
        
        public override void PerformHatsuAbility()
        {
            // Ultimate support ability - mass heal and buff
            if (ConsumeNen(50f))
            {
                Collider[] allies = Physics.OverlapSphere(transform.position, 20f);
                foreach (Collider ally in allies)
                {
                    if (ally.CompareTag("Player") || ally.CompareTag("Ally"))
                    {
                        var character = ally.GetComponent<BaseCharacter>();
                        if (character != null)
                        {
                            character.Heal(character.maxHealth * 0.6f); // 60% heal
                            
                            // Temporary stat boost
                            StartCoroutine(BuffAlly(character));
                        }
                    }
                }
                
                PlayRandomAudioClip(supportLines);
            }
        }
        
        private IEnumerator BuffAlly(BaseCharacter ally)
        {
            // Temporary stat boosts
            ally.attackDamage *= 1.3f;
            ally.runSpeed *= 1.2f;
            
            yield return new WaitForSeconds(15f);
            
            ally.attackDamage /= 1.3f;
            ally.runSpeed /= 1.2f;
        }
        
        public override void LightAttack()
        {
            if (canAttack)
            {
                canAttack = false;
                ChangeState(CharacterState.Attacking);
                
                if (animator != null)
                {
                    animator.SetTrigger("LightAttack");
                }
                
                // Briefcase swing attack
                Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 1.5f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            enemyChar.TakeDamage(briefcaseSwingDamage);
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
                
                // Powerful briefcase slam
                Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 2f, 2.5f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            float damage = briefcaseSwingDamage * 2f;
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
                    // Remote punch combo finisher
                    PerformRemotePunch();
                    break;
            }
        }
        
        private IEnumerator AttackCooldown()
        {
            yield return new WaitForSeconds(attackCooldown * 1.2f); // Leorio attacks slower
            canAttack = true;
            ChangeState(CharacterState.Idle);
        }
        
        public override void TakeDamage(float damage, Vector3 knockback = default)
        {
            base.TakeDamage(damage, knockback);
            
            // Play appropriate reaction
            if (currentHealth > maxHealth * 0.5f)
            {
                PlayRandomAudioClip(comedyLines); // Comedic reaction when not too hurt
            }
            else
            {
                PlayRandomAudioClip(seriousLines); // Serious when badly injured
            }
        }
        
        public override void PlayBattleCry()
        {
            PlayRandomAudioClip(seriousLines);
        }
        
        public override void PlayVictoryLine()
        {
            PlayRandomAudioClip(doctorLines);
        }
        
        public override void PlayDeathLine()
        {
            if (audioSource != null && seriousLines.Length > 0)
            {
                audioSource.PlayOneShot(seriousLines[0]);
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
    }
    
    /// <summary>
    /// Briefcase projectile component for Leorio's thrown briefcase attack
    /// </summary>
    public class BriefcaseProjectile : MonoBehaviour
    {
        private float damage;
        private LeorioParadinight owner;
        private bool hasHit = false;
        
        public void Initialize(float projectileDamage, LeorioParadinight leorioOwner)
        {
            damage = projectileDamage;
            owner = leorioOwner;
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (!hasHit)
            {
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    var enemy = collision.gameObject.GetComponent<BaseCharacter>();
                    if (enemy != null)
                    {
                        Vector3 knockback = (collision.transform.position - transform.position).normalized * 6f;
                        enemy.TakeDamage(damage, knockback);
                        hasHit = true;
                    }
                }
                else if (collision.gameObject.CompareTag("Environment"))
                {
                    hasHit = true;
                }
                
                // Stop the briefcase after hitting something
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}