using UnityEngine;
using System.Collections;

namespace HunterRush.Characters
{
    /// <summary>
    /// Killua Zoldyck character implementation with lightning abilities and assassin techniques
    /// </summary>
    public class KilluaZoldyck : BaseCharacter
    {
        [Header("Killua Specific Settings")]
        [SerializeField] private float godspeedDuration = 8f;
        [SerializeField] private float godspeedCooldown = 25f;
        [SerializeField] private float lightningDamageMultiplier = 2f;
        [SerializeField] private float yoyoRange = 5f;
        [SerializeField] private int afterimageCount = 3;
        [SerializeField] private GameObject lightningEffectPrefab;
        [SerializeField] private GameObject yoyoPrefab;
        [SerializeField] private GameObject thunderboltPrefab;
        [SerializeField] private ParticleSystem electricAura;
        [SerializeField] private ParticleSystem afterimageEffect;
        
        [Header("Voice Lines")]
        [SerializeField] private AudioClip[] coolQuips;
        [SerializeField] private AudioClip[] lightningLines;
        [SerializeField] private AudioClip[] assassinLines;
        [SerializeField] private AudioClip[] godspeedLines;
        
        // Lightning abilities
        private bool isGodspeedActive = false;
        private bool canUseGodspeed = true;
        private float electricCharge = 0f;
        private float maxElectricCharge = 100f;
        
        // Assassin abilities
        private bool isInvisible = false;
        private float invisibilityDuration = 5f;
        private bool canUseInvisibility = true;
        
        // Yo-yo weapon
        private GameObject activeYoyo;
        private bool yoyoDeployed = false;
        
        protected override void InitializeCharacter()
        {
            characterName = "Killua Zoldyck";
            characterType = CharacterType.Killua;
            auraColor = Color.cyan;
            
            // Killua's stats - high speed and agility, moderate strength
            runSpeed = 15f; // Fastest base speed
            jumpPower = 16f;
            attackDamage = 25f;
            maxHealth = 100f;
            maxNenCapacity = 120f; // High Nen capacity for abilities
            dashDistance = 8f; // Longer dash distance
            dashCooldown = 0.5f; // Shorter dash cooldown
            
            // Set current stats
            currentHealth = maxHealth;
            currentNen = maxNenCapacity;
            electricCharge = maxElectricCharge;
        }
        
        protected override void HandleInput()
        {
            base.HandleInput();
            
            // Godspeed ability
            if (Input.GetKeyDown(KeyCode.Q) && canUseGodspeed)
            {
                ActivateGodspeed();
            }
            
            // Thunderbolt attack
            if (Input.GetKeyDown(KeyCode.E) && electricCharge >= 30f)
            {
                PerformThunderbolt();
            }
            
            // Yo-yo attack
            if (Input.GetMouseButtonDown(1)) // Right click
            {
                if (!yoyoDeployed)
                    DeployYoyo();
                else
                    RetractYoyo();
            }
            
            // Invisibility (Zetsu + assassin training)
            if (Input.GetKeyDown(KeyCode.R) && canUseInvisibility)
            {
                ActivateInvisibility();
            }
            
            // Electric discharge (area attack)
            if (Input.GetKeyDown(KeyCode.F) && electricCharge >= 50f)
            {
                PerformElectricDischarge();
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            // Regenerate electric charge
            if (electricCharge < maxElectricCharge)
            {
                electricCharge = Mathf.Min(maxElectricCharge, electricCharge + 10f * Time.deltaTime);
            }
            
            // Update afterimage effect during high speed movement
            if (rb.velocity.magnitude > 10f && !isGodspeedActive)
            {
                CreateAfterimage();
            }
        }
        
        private void ActivateGodspeed()
        {
            if (!isGodspeedActive && ConsumeNen(40f))
            {
                StartCoroutine(GodspeedCoroutine());
            }
        }
        
        private IEnumerator GodspeedCoroutine()
        {
            isGodspeedActive = true;
            canUseGodspeed = false;
            
            // Massive stat boosts
            float originalSpeed = runSpeed;
            float originalJump = jumpPower;
            float originalDashCooldown = dashCooldown;
            
            runSpeed *= 2.5f;
            jumpPower *= 2f;
            dashCooldown *= 0.3f;
            
            // Visual effects
            if (electricAura != null)
            {
                electricAura.Play();
                var main = electricAura.main;
                main.startColor = Color.white; // Godspeed aura is white/silver
            }
            
            // Invincibility frames during Godspeed
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
            
            // Play voice line
            PlayRandomAudioClip(godspeedLines);
            
            // Continuous afterimage effect
            StartCoroutine(ContinuousAfterimages());
            
            yield return new WaitForSeconds(godspeedDuration);
            
            // Restore original stats
            runSpeed = originalSpeed;
            jumpPower = originalJump;
            dashCooldown = originalDashCooldown;
            
            // Remove invincibility
            Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
            
            // Stop effects
            if (electricAura != null)
                electricAura.Stop();
            
            isGodspeedActive = false;
            
            // Start cooldown
            yield return new WaitForSeconds(godspeedCooldown);
            canUseGodspeed = true;
        }
        
        private IEnumerator ContinuousAfterimages()
        {
            while (isGodspeedActive)
            {
                CreateAfterimage();
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void CreateAfterimage()
        {
            if (afterimageEffect != null)
            {
                // Create afterimage at current position
                GameObject afterimage = new GameObject("Afterimage");
                afterimage.transform.position = transform.position;
                afterimage.transform.rotation = transform.rotation;
                
                // Copy character model
                if (characterModel != null)
                {
                    GameObject afterimageModel = Instantiate(characterModel, afterimage.transform);
                    
                    // Make it semi-transparent and fade out
                    Renderer[] renderers = afterimageModel.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material mat in renderer.materials)
                        {
                            Color color = mat.color;
                            color.a = 0.3f;
                            mat.color = color;
                        }
                    }
                }
                
                // Destroy afterimage after short time
                Destroy(afterimage, 0.5f);
            }
        }
        
        private void PerformThunderbolt()
        {
            if (electricCharge >= 30f)
            {
                electricCharge -= 30f;
                
                // Get target (closest enemy or mouse position)
                Vector3 targetPosition = GetTargetPosition();
                
                // Create thunderbolt effect
                if (thunderboltPrefab != null)
                {
                    GameObject thunderbolt = Instantiate(thunderboltPrefab, targetPosition + Vector3.up * 10f, Quaternion.identity);
                    
                    // Animate thunderbolt striking down
                    StartCoroutine(AnimateThunderbolt(thunderbolt, targetPosition));
                }
                
                // Play voice line
                PlayRandomAudioClip(lightningLines);
                
                if (animator != null)
                {
                    animator.SetTrigger("Thunderbolt");
                }
            }
        }
        
        private IEnumerator AnimateThunderbolt(GameObject thunderbolt, Vector3 targetPos)
        {
            float duration = 0.5f;
            Vector3 startPos = targetPos + Vector3.up * 10f;
            
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                thunderbolt.transform.position = Vector3.Lerp(startPos, targetPos, progress);
                yield return null;
            }
            
            // Damage enemies in area
            Collider[] enemies = Physics.OverlapSphere(targetPos, 3f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    var enemyChar = enemy.GetComponent<BaseCharacter>();
                    if (enemyChar != null)
                    {
                        float damage = attackDamage * lightningDamageMultiplier;
                        enemyChar.TakeDamage(damage);
                    }
                }
            }
            
            Destroy(thunderbolt, 1f);
        }
        
        private void DeployYoyo()
        {
            if (yoyoPrefab != null && ConsumeNen(10f))
            {
                Vector3 deployPosition = transform.position + transform.forward * 2f;
                activeYoyo = Instantiate(yoyoPrefab, deployPosition, transform.rotation);
                yoyoDeployed = true;
                
                // Add yo-yo behavior
                YoyoWeapon yoyoScript = activeYoyo.GetComponent<YoyoWeapon>();
                if (yoyoScript != null)
                {
                    yoyoScript.Initialize(this, yoyoRange);
                }
                
                if (animator != null)
                {
                    animator.SetBool("YoyoDeployed", true);
                }
            }
        }
        
        private void RetractYoyo()
        {
            if (activeYoyo != null)
            {
                Destroy(activeYoyo);
                yoyoDeployed = false;
                
                if (animator != null)
                {
                    animator.SetBool("YoyoDeployed", false);
                }
            }
        }
        
        private void ActivateInvisibility()
        {
            if (!isInvisible && ConsumeNen(25f))
            {
                StartCoroutine(InvisibilityCoroutine());
            }
        }
        
        private IEnumerator InvisibilityCoroutine()
        {
            isInvisible = true;
            canUseInvisibility = false;
            
            // Change to Zetsu state
            ChangeNenState(NenState.Zetsu);
            
            // Make character semi-transparent
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    Color color = mat.color;
                    color.a = 0.3f;
                    mat.color = color;
                }
            }
            
            // Ignore enemy detection
            gameObject.layer = LayerMask.NameToLayer("Hidden");
            
            yield return new WaitForSeconds(invisibilityDuration);
            
            // Restore visibility
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    Color color = mat.color;
                    color.a = 1f;
                    mat.color = color;
                }
            }
            
            gameObject.layer = LayerMask.NameToLayer("Player");
            ChangeNenState(NenState.Normal);
            isInvisible = false;
            
            // Cooldown
            yield return new WaitForSeconds(10f);
            canUseInvisibility = true;
        }
        
        private void PerformElectricDischarge()
        {
            if (electricCharge >= 50f)
            {
                electricCharge -= 50f;
                
                // Create electric explosion around character
                if (lightningEffectPrefab != null)
                {
                    GameObject effect = Instantiate(lightningEffectPrefab, transform.position, Quaternion.identity);
                    effect.transform.localScale = Vector3.one * 5f;
                    Destroy(effect, 2f);
                }
                
                // Damage all enemies in range
                Collider[] enemies = Physics.OverlapSphere(transform.position, 5f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            float damage = attackDamage * lightningDamageMultiplier * 1.5f;
                            Vector3 knockback = (enemy.transform.position - transform.position).normalized * 6f;
                            enemyChar.TakeDamage(damage, knockback);
                        }
                    }
                }
                
                if (animator != null)
                {
                    animator.SetTrigger("ElectricDischarge");
                }
                
                PlayRandomAudioClip(lightningLines);
            }
        }
        
        private Vector3 GetTargetPosition()
        {
            // Find closest enemy
            Collider[] enemies = Physics.OverlapSphere(transform.position, 15f);
            float closestDistance = float.MaxValue;
            Vector3 targetPos = transform.position + transform.forward * 5f;
            
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetPos = enemy.transform.position;
                    }
                }
            }
            
            return targetPos;
        }
        
        public override void PerformSpecialAbility()
        {
            if (canUseGodspeed)
            {
                ActivateGodspeed();
            }
            else if (electricCharge >= 30f)
            {
                PerformThunderbolt();
            }
        }
        
        public override void PerformHatsuAbility()
        {
            if (electricCharge >= 50f)
            {
                PerformElectricDischarge();
            }
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
                
                // Claw attack with electric damage
                Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 1.5f, 1.5f);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        var enemyChar = enemy.GetComponent<BaseCharacter>();
                        if (enemyChar != null)
                        {
                            float damage = attackDamage;
                            if (electricCharge >= 10f)
                            {
                                damage *= lightningDamageMultiplier;
                                electricCharge -= 10f;
                                
                                // Electric effect
                                if (lightningEffectPrefab != null)
                                {
                                    GameObject effect = Instantiate(lightningEffectPrefab, enemy.transform.position, Quaternion.identity);
                                    Destroy(effect, 1f);
                                }
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
            if (canAttack && ConsumeNen(15f))
            {
                canAttack = false;
                ChangeState(CharacterState.Attacking);
                
                if (animator != null)
                {
                    animator.SetTrigger("HeavyAttack");
                }
                
                // Electric claw strike
                if (electricCharge >= 20f)
                {
                    electricCharge -= 20f;
                    
                    Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 2f, 2f);
                    foreach (Collider enemy in enemies)
                    {
                        if (enemy.CompareTag("Enemy"))
                        {
                            var enemyChar = enemy.GetComponent<BaseCharacter>();
                            if (enemyChar != null)
                            {
                                float damage = attackDamage * 2f * lightningDamageMultiplier;
                                Vector3 knockback = (enemy.transform.position - transform.position).normalized * 8f;
                                enemyChar.TakeDamage(damage, knockback);
                                
                                // Paralysis effect
                                // TODO: Add paralysis status effect
                            }
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
                    // Lightning combo finisher
                    PerformElectricDischarge();
                    break;
            }
        }
        
        private IEnumerator AttackCooldown()
        {
            yield return new WaitForSeconds(attackCooldown * 0.8f); // Killua attacks faster
            canAttack = true;
            ChangeState(CharacterState.Idle);
        }
        
        public override void PlayBattleCry()
        {
            PlayRandomAudioClip(coolQuips);
        }
        
        public override void PlayVictoryLine()
        {
            PlayRandomAudioClip(coolQuips);
        }
        
        public override void PlayDeathLine()
        {
            if (audioSource != null && assassinLines.Length > 0)
            {
                audioSource.PlayOneShot(assassinLines[0]);
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
    /// Yo-yo weapon component for Killua's ranged attacks
    /// </summary>
    public class YoyoWeapon : MonoBehaviour
    {
        private KilluaZoldyck owner;
        private float maxRange;
        private Vector3 initialPosition;
        private bool returning = false;
        
        public void Initialize(KilluaZoldyck killuaOwner, float range)
        {
            owner = killuaOwner;
            maxRange = range;
            initialPosition = owner.transform.position;
        }
        
        void Update()
        {
            if (owner == null) return;
            
            float distanceFromOwner = Vector3.Distance(transform.position, owner.transform.position);
            
            // Return if too far
            if (distanceFromOwner >= maxRange || returning)
            {
                returning = true;
                transform.position = Vector3.MoveTowards(transform.position, owner.transform.position, 15f * Time.deltaTime);
                
                if (distanceFromOwner < 1f)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<BaseCharacter>();
                if (enemy != null)
                {
                    enemy.TakeDamage(owner.attackDamage * 1.2f);
                    returning = true; // Start returning after hit
                }
            }
        }
    }
}