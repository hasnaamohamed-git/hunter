using UnityEngine;
using System.Collections;

namespace HunterRush.Characters
{
    /// <summary>
    /// Kurapika Kurta character implementation with chain abilities and Emperor Time
    /// </summary>
    public class KurapikaKurta : BaseCharacter
    {
        [Header("Kurapika Specific Settings")]
        [SerializeField] private float emperorTimeDuration = 15f;
        [SerializeField] private float emperorTimeCooldown = 45f;
        [SerializeField] private float chainRange = 8f;
        [SerializeField] private float bindingDuration = 5f;
        [SerializeField] private GameObject chainPrefab;
        [SerializeField] private GameObject judgementChainPrefab;
        [SerializeField] private ParticleSystem emperorTimeAura;
        [SerializeField] private ParticleSystem scarletEyesEffect;
        
        [Header("Chain Abilities")]
        [SerializeField] private ChainType[] availableChains;
        
        [Header("Voice Lines")]
        [SerializeField] private AudioClip[] vengeanceLines;
        [SerializeField] private AudioClip[] chainLines;
        [SerializeField] private AudioClip[] emperorTimeLines;
        [SerializeField] private AudioClip[] phantomTroupeLines;
        
        // Emperor Time state
        private bool isEmperorTimeActive = false;
        private bool canUseEmperorTime = true;
        private float emperorTimeLifeDrain = 10f; // Life drain per second during Emperor Time
        
        // Chain system
        private ChainType selectedChain = ChainType.Holy;
        private GameObject activeChain;
        private bool chainDeployed = false;
        
        // Scarlet Eyes state
        private bool scarletEyesActive = false;
        private Color originalEyeColor;
        private Color scarletColor = Color.red;
        
        // Binding targets
        private System.Collections.Generic.List<BaseCharacter> boundEnemies = new System.Collections.Generic.List<BaseCharacter>();
        
        protected override void InitializeCharacter()
        {
            characterName = "Kurapika Kurta";
            characterType = CharacterType.Kurapika;
            auraColor = Color.blue;
            originalEyeColor = Color.blue;
            
            // Kurapika's stats - balanced with high Nen capacity
            runSpeed = 11f;
            jumpPower = 14f;
            attackDamage = 28f;
            maxHealth = 110f;
            maxNenCapacity = 140f; // Highest Nen capacity
            
            // Set current stats
            currentHealth = maxHealth;
            currentNen = maxNenCapacity;
            
            // Initialize available chains
            if (availableChains == null || availableChains.Length == 0)
            {
                availableChains = new ChainType[] { ChainType.Holy, ChainType.Dowsing, ChainType.Chain, ChainType.Judgement, ChainType.Steal };
            }
        }
        
        protected override void HandleInput()
        {
            base.HandleInput();
            
            // Emperor Time activation
            if (Input.GetKeyDown(KeyCode.Q) && canUseEmperorTime)
            {
                ActivateEmperorTime();
            }
            
            // Chain selection
            if (Input.GetKeyDown(KeyCode.Alpha1))
                selectedChain = ChainType.Holy;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                selectedChain = ChainType.Dowsing;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                selectedChain = ChainType.Chain;
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                selectedChain = ChainType.Judgement;
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                selectedChain = ChainType.Steal;
            
            // Deploy/use selected chain
            if (Input.GetMouseButtonDown(1)) // Right click
            {
                UseSelectedChain();
            }
            
            // Retract chain
            if (Input.GetKeyDown(KeyCode.R) && chainDeployed)
            {
                RetractChain();
            }
            
            // Scarlet Eyes toggle (emotional state)
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleScarletEyes();
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            // Emperor Time life drain
            if (isEmperorTimeActive)
            {
                TakeDamage(emperorTimeLifeDrain * Time.deltaTime);
            }
            
            // Update bound enemies
            UpdateBoundEnemies();
        }
        
        private void ActivateEmperorTime()
        {
            if (!isEmperorTimeActive && ConsumeNen(50f))
            {
                StartCoroutine(EmperorTimeCoroutine());
            }
        }
        
        private IEnumerator EmperorTimeCoroutine()
        {
            isEmperorTimeActive = true;
            canUseEmperorTime = false;
            
            // Activate Scarlet Eyes automatically
            if (!scarletEyesActive)
                ToggleScarletEyes();
            
            // Massive stat boosts - become 100% in all Nen categories
            float originalAttack = attackDamage;
            float originalSpeed = runSpeed;
            float originalJump = jumpPower;
            float originalNenRegen = nenRegenRate;
            
            attackDamage *= 2f;
            runSpeed *= 1.5f;
            jumpPower *= 1.5f;
            nenRegenRate *= 3f;
            
            // Visual effects
            if (emperorTimeAura != null)
            {
                emperorTimeAura.Play();
                var main = emperorTimeAura.main;
                main.startColor = scarletColor;
            }
            
            // All chains become available and enhanced
            EnableAllChains();
            
            // Play voice line
            PlayRandomAudioClip(emperorTimeLines);
            
            yield return new WaitForSeconds(emperorTimeDuration);
            
            // Restore original stats
            attackDamage = originalAttack;
            runSpeed = originalSpeed;
            jumpPower = originalJump;
            nenRegenRate = originalNenRegen;
            
            // Stop effects
            if (emperorTimeAura != null)
                emperorTimeAura.Stop();
            
            isEmperorTimeActive = false;
            
            // Start cooldown
            yield return new WaitForSeconds(emperorTimeCooldown);
            canUseEmperorTime = true;
        }
        
        private void ToggleScarletEyes()
        {
            scarletEyesActive = !scarletEyesActive;
            
            if (scarletEyesActive)
            {
                // Change aura color to red
                auraColor = scarletColor;
                
                // Boost against Phantom Troupe members
                // TODO: Implement Phantom Troupe detection
                
                // Visual effects
                if (scarletEyesEffect != null)
                    scarletEyesEffect.Play();
                
                // Stat boosts
                attackDamage *= 1.3f;
                maxNenCapacity *= 1.2f;
            }
            else
            {
                // Restore original state
                auraColor = Color.blue;
                
                if (scarletEyesEffect != null)
                    scarletEyesEffect.Stop();
                
                // Restore stats
                attackDamage /= 1.3f;
                maxNenCapacity /= 1.2f;
            }
            
            // Update aura effect color
            if (auraEffect != null)
            {
                var main = auraEffect.main;
                main.startColor = auraColor;
            }
        }
        
        private void UseSelectedChain()
        {
            switch (selectedChain)
            {
                case ChainType.Holy:
                    UseHolyChain();
                    break;
                case ChainType.Dowsing:
                    UseDowsingChain();
                    break;
                case ChainType.Chain:
                    UseChainJail();
                    break;
                case ChainType.Judgement:
                    UseJudgementChain();
                    break;
                case ChainType.Steal:
                    UseStealChain();
                    break;
            }
        }
        
        private void UseHolyChain()
        {
            if (ConsumeNen(15f))
            {
                // Healing chain
                Heal(30f);
                
                // Heal nearby allies
                Collider[] allies = Physics.OverlapSphere(transform.position, 5f);
                foreach (Collider ally in allies)
                {
                    if (ally.CompareTag("Player") || ally.CompareTag("Ally"))
                    {
                        var character = ally.GetComponent<BaseCharacter>();
                        if (character != null && character != this)
                        {
                            character.Heal(20f);
                        }
                    }
                }
                
                // Create healing effect
                CreateChainEffect(transform.position, Color.white);
                
                if (animator != null)
                {
                    animator.SetTrigger("HolyChain");
                }
                
                Debug.Log("Holy Chain - Healing!");
            }
        }
        
        private void UseDowsingChain()
        {
            if (ConsumeNen(10f))
            {
                // Reveals hidden enemies and items
                Collider[] hiddenObjects = Physics.OverlapSphere(transform.position, 15f);
                foreach (Collider obj in hiddenObjects)
                {
                    if (obj.gameObject.layer == LayerMask.NameToLayer("Hidden"))
                    {
                        // Reveal hidden object
                        obj.gameObject.layer = LayerMask.NameToLayer("Default");
                        
                        // Add highlight effect
                        // TODO: Add highlight shader
                    }
                }
                
                // Create dowsing effect
                CreateChainEffect(transform.position + transform.forward * 3f, Color.yellow);
                
                Debug.Log("Dowsing Chain - Revealing hidden objects!");
            }
        }
        
        private void UseChainJail()
        {
            if (ConsumeNen(25f))
            {
                // Get target enemy
                BaseCharacter target = GetClosestEnemy();
                if (target != null)
                {
                    // Bind the enemy
                    StartCoroutine(BindEnemy(target));
                    
                    // Create chain effect
                    CreateChainEffect(target.transform.position, Color.gray);
                    
                    PlayRandomAudioClip(chainLines);
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("ChainJail");
                }
            }
        }
        
        private void UseJudgementChain()
        {
            if (ConsumeNen(40f))
            {
                BaseCharacter target = GetClosestEnemy();
                if (target != null)
                {
                    // Instant defeat for Phantom Troupe members, heavy damage for others
                    if (IsPhantomTroupeMember(target))
                    {
                        target.TakeDamage(target.maxHealth); // Instant kill
                        PlayRandomAudioClip(phantomTroupeLines);
                    }
                    else
                    {
                        target.TakeDamage(attackDamage * 3f);
                    }
                    
                    // Create judgement effect
                    if (judgementChainPrefab != null)
                    {
                        GameObject effect = Instantiate(judgementChainPrefab, target.transform.position, Quaternion.identity);
                        Destroy(effect, 3f);
                    }
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("JudgementChain");
                }
            }
        }
        
        private void UseStealChain()
        {
            if (ConsumeNen(35f))
            {
                BaseCharacter target = GetClosestEnemy();
                if (target != null)
                {
                    // Steal enemy ability (simplified - boost own stats temporarily)
                    StartCoroutine(StealAbilityCoroutine(target));
                    
                    // Create steal effect
                    CreateChainEffect(target.transform.position, Color.magenta);
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("StealChain");
                }
            }
        }
        
        private IEnumerator BindEnemy(BaseCharacter enemy)
        {
            if (!boundEnemies.Contains(enemy))
            {
                boundEnemies.Add(enemy);
                
                // Disable enemy movement and attacks
                enemy.ChangeState(CharacterState.TakingDamage); // Use damage state to prevent actions
                
                // Create visual binding effect
                GameObject chainEffect = CreateChainEffect(enemy.transform.position, Color.gray);
                
                yield return new WaitForSeconds(bindingDuration);
                
                // Release binding
                boundEnemies.Remove(enemy);
                if (enemy != null)
                {
                    enemy.ChangeState(CharacterState.Idle);
                }
                
                if (chainEffect != null)
                    Destroy(chainEffect);
            }
        }
        
        private IEnumerator StealAbilityCoroutine(BaseCharacter target)
        {
            // Temporarily boost stats based on target's abilities
            float statBoost = 1.5f;
            float originalAttack = attackDamage;
            float originalSpeed = runSpeed;
            
            attackDamage *= statBoost;
            runSpeed *= statBoost;
            
            yield return new WaitForSeconds(10f);
            
            // Restore original stats
            attackDamage = originalAttack;
            runSpeed = originalSpeed;
        }
        
        private GameObject CreateChainEffect(Vector3 position, Color color)
        {
            if (chainPrefab != null)
            {
                GameObject chain = Instantiate(chainPrefab, position, Quaternion.identity);
                
                // Set chain color
                Renderer renderer = chain.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
                
                return chain;
            }
            return null;
        }
        
        private void RetractChain()
        {
            if (activeChain != null)
            {
                Destroy(activeChain);
                chainDeployed = false;
            }
        }
        
        private BaseCharacter GetClosestEnemy()
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, chainRange);
            BaseCharacter closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = enemy.GetComponent<BaseCharacter>();
                    }
                }
            }
            
            return closest;
        }
        
        private bool IsPhantomTroupeMember(BaseCharacter character)
        {
            // TODO: Implement Phantom Troupe member detection
            // For now, check if enemy has specific tag or component
            return character.gameObject.CompareTag("PhantomTroupe");
        }
        
        private void EnableAllChains()
        {
            // During Emperor Time, all chain types become available
            availableChains = new ChainType[] { ChainType.Holy, ChainType.Dowsing, ChainType.Chain, ChainType.Judgement, ChainType.Steal };
        }
        
        private void UpdateBoundEnemies()
        {
            // Remove null references from bound enemies list
            boundEnemies.RemoveAll(enemy => enemy == null);
        }
        
        public override void PerformSpecialAbility()
        {
            if (canUseEmperorTime)
            {
                ActivateEmperorTime();
            }
            else
            {
                UseSelectedChain();
            }
        }
        
        public override void PerformHatsuAbility()
        {
            // Use most powerful chain ability
            selectedChain = ChainType.Judgement;
            UseSelectedChain();
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
                
                // Chain whip attack
                Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 2f, 2f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            float damage = attackDamage;
                            if (scarletEyesActive && IsPhantomTroupeMember(enemyChar))
                            {
                                damage *= 2f; // Double damage against Phantom Troupe
                            }
                            enemyChar.TakeDamage(damage);
                        }
                    }
                }
                
                StartCoroutine(AttackCooldown());
            }
        }
        
        public override void HeavyAttack()
        {
            if (canAttack && ConsumeNen(20f))
            {
                canAttack = false;
                ChangeState(CharacterState.Attacking);
                
                if (animator != null)
                {
                    animator.SetTrigger("HeavyAttack");
                }
                
                // Chain slam attack
                Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 3f, 3f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            float damage = attackDamage * 2.5f;
                            if (scarletEyesActive && IsPhantomTroupeMember(enemyChar))
                            {
                                damage *= 2f;
                            }
                            Vector3 knockback = (enemy.transform.position - transform.position).normalized * 10f;
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
                    // Chain combo finisher
                    UseChainJail();
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
            if (scarletEyesActive)
                PlayRandomAudioClip(vengeanceLines);
            else
                PlayRandomAudioClip(chainLines);
        }
        
        public override void PlayVictoryLine()
        {
            PlayRandomAudioClip(vengeanceLines);
        }
        
        public override void PlayDeathLine()
        {
            if (audioSource != null && vengeanceLines.Length > 0)
            {
                audioSource.PlayOneShot(vengeanceLines[0]);
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
        
        private enum ChainType
        {
            Holy = 0,      // Healing chain
            Dowsing = 1,   // Detection chain
            Chain = 2,     // Binding chain (Chain Jail)
            Judgement = 3, // Instant defeat chain
            Steal = 4      // Ability stealing chain
        }
    }
}