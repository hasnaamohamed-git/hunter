using UnityEngine;
using System.Collections;

namespace HunterRush.Nen
{
    /// <summary>
    /// Comprehensive Nen system controller handling all four basic principles and advanced techniques
    /// </summary>
    public class NenController : MonoBehaviour
    {
        [Header("Nen Capacity")]
        public float maxNenCapacity = 100f;
        public float currentNen = 100f;
        public float nenRegenRate = 5f;
        public float nenDrainRate = 2f; // Passive drain when using Nen
        
        [Header("Nen States")]
        public NenState currentNenState = NenState.Normal;
        public bool autoTenWhenLowHealth = true;
        public float autoTenThreshold = 0.3f; // Activate Ten when health drops below 30%
        
        [Header("Ten (Shroud)")]
        public float tenDefenseMultiplier = 0.5f; // 50% damage reduction
        public float tenNenCost = 3f; // Nen cost per second
        public ParticleSystem tenAuraEffect;
        
        [Header("Zetsu (Suppress)")]
        public float zetsuSpeedMultiplier = 0.7f; // Slower movement
        public float zetsuDetectionReduction = 0.1f; // Harder to detect
        
        [Header("Ren (Enhance)")]
        public float renAttackMultiplier = 1.5f; // 50% attack boost
        public float renNenCost = 5f; // Nen cost per second
        public ParticleSystem renAuraEffect;
        
        [Header("Hatsu (Release)")]
        public float hatsuNenCost = 20f; // Base cost for Hatsu abilities
        public ParticleSystem hatsuAuraEffect;
        
        [Header("Advanced Techniques")]
        public bool canUseGyo = false;
        public bool canUseIn = false;
        public bool canUseEn = false;
        public bool canUseShu = false;
        public bool canUseKo = false;
        
        [Header("Nen Categories")]
        public NenCategory primaryCategory = NenCategory.Enhancement;
        public float enhancementEfficiency = 1.0f;
        public float emissionEfficiency = 0.8f;
        public float manipulationEfficiency = 0.6f;
        public float transmutationEfficiency = 0.8f;
        public float conjurationEfficiency = 0.6f;
        public float specializationEfficiency = 0.0f;
        
        // Components
        private BaseCharacter character;
        private AudioSource audioSource;
        
        // State tracking
        private bool isTenActive = false;
        private bool isRenActive = false;
        private bool isZetsuActive = false;
        private bool isHatsuActive = false;
        
        // Advanced technique states
        private bool isGyoActive = false;
        private bool isInActive = false;
        private bool isEnActive = false;
        private bool isShuActive = false;
        private bool isKoActive = false;
        
        // Events
        public System.Action<NenState> OnNenStateChanged;
        public System.Action<float> OnNenChanged;
        
        void Awake()
        {
            character = GetComponent<BaseCharacter>();
            audioSource = GetComponent<AudioSource>();
        }
        
        void Start()
        {
            currentNen = maxNenCapacity;
            SetNenEfficiencies();
        }
        
        void Update()
        {
            HandleNenRegeneration();
            HandleNenDrain();
            HandleAutoTen();
            UpdateNenEffects();
        }
        
        private void SetNenEfficiencies()
        {
            // Set efficiencies based on primary category (Hunter x Hunter Nen chart)
            switch (primaryCategory)
            {
                case NenCategory.Enhancement:
                    enhancementEfficiency = 1.0f;
                    emissionEfficiency = 0.8f;
                    transmutationEfficiency = 0.8f;
                    manipulationEfficiency = 0.6f;
                    conjurationEfficiency = 0.6f;
                    specializationEfficiency = 0.0f;
                    break;
                    
                case NenCategory.Emission:
                    enhancementEfficiency = 0.8f;
                    emissionEfficiency = 1.0f;
                    transmutationEfficiency = 0.6f;
                    manipulationEfficiency = 0.8f;
                    conjurationEfficiency = 0.4f;
                    specializationEfficiency = 0.0f;
                    break;
                    
                case NenCategory.Manipulation:
                    enhancementEfficiency = 0.6f;
                    emissionEfficiency = 0.8f;
                    transmutationEfficiency = 0.6f;
                    manipulationEfficiency = 1.0f;
                    conjurationEfficiency = 0.8f;
                    specializationEfficiency = 0.0f;
                    break;
                    
                case NenCategory.Transmutation:
                    enhancementEfficiency = 0.8f;
                    emissionEfficiency = 0.6f;
                    transmutationEfficiency = 1.0f;
                    manipulationEfficiency = 0.6f;
                    conjurationEfficiency = 0.8f;
                    specializationEfficiency = 0.0f;
                    break;
                    
                case NenCategory.Conjuration:
                    enhancementEfficiency = 0.6f;
                    emissionEfficiency = 0.4f;
                    transmutationEfficiency = 0.8f;
                    manipulationEfficiency = 0.8f;
                    conjurationEfficiency = 1.0f;
                    specializationEfficiency = 0.0f;
                    break;
                    
                case NenCategory.Specialization:
                    // Specialization has unique efficiency - varies by individual
                    enhancementEfficiency = 0.4f;
                    emissionEfficiency = 0.6f;
                    transmutationEfficiency = 0.4f;
                    manipulationEfficiency = 0.8f;
                    conjurationEfficiency = 0.6f;
                    specializationEfficiency = 1.0f;
                    break;
            }
        }
        
        private void HandleNenRegeneration()
        {
            if (currentNen < maxNenCapacity && !isZetsuActive)
            {
                float regenAmount = nenRegenRate * Time.deltaTime;
                
                // Slower regen during active Nen use
                if (isTenActive || isRenActive || isHatsuActive)
                {
                    regenAmount *= 0.3f;
                }
                
                currentNen = Mathf.Min(maxNenCapacity, currentNen + regenAmount);
                OnNenChanged?.Invoke(currentNen);
            }
        }
        
        private void HandleNenDrain()
        {
            float drainAmount = 0f;
            
            if (isTenActive)
                drainAmount += tenNenCost * Time.deltaTime;
            
            if (isRenActive)
                drainAmount += renNenCost * Time.deltaTime;
            
            if (isGyoActive)
                drainAmount += 2f * Time.deltaTime;
            
            if (isEnActive)
                drainAmount += 8f * Time.deltaTime;
            
            if (isShuActive)
                drainAmount += 4f * Time.deltaTime;
            
            if (isKoActive)
                drainAmount += 6f * Time.deltaTime;
            
            if (drainAmount > 0)
            {
                ConsumeNen(drainAmount);
            }
        }
        
        private void HandleAutoTen()
        {
            if (autoTenWhenLowHealth && character != null)
            {
                float healthPercent = character.currentHealth / character.maxHealth;
                
                if (healthPercent <= autoTenThreshold && !isTenActive && currentNen >= tenNenCost)
                {
                    ActivateTen();
                }
            }
        }
        
        private void UpdateNenEffects()
        {
            // Update particle effects based on current state
            if (tenAuraEffect != null)
            {
                if (isTenActive && !tenAuraEffect.isPlaying)
                    tenAuraEffect.Play();
                else if (!isTenActive && tenAuraEffect.isPlaying)
                    tenAuraEffect.Stop();
            }
            
            if (renAuraEffect != null)
            {
                if (isRenActive && !renAuraEffect.isPlaying)
                    renAuraEffect.Play();
                else if (!isRenActive && renAuraEffect.isPlaying)
                    renAuraEffect.Stop();
            }
        }
        
        public bool ConsumeNen(float amount)
        {
            if (currentNen >= amount)
            {
                currentNen -= amount;
                OnNenChanged?.Invoke(currentNen);
                return true;
            }
            return false;
        }
        
        public void RestoreNen(float amount)
        {
            currentNen = Mathf.Min(maxNenCapacity, currentNen + amount);
            OnNenChanged?.Invoke(currentNen);
        }
        
        public void ChangeNenState(NenState newState)
        {
            // Deactivate current state
            DeactivateCurrentState();
            
            currentNenState = newState;
            
            // Activate new state
            switch (newState)
            {
                case NenState.Ten:
                    ActivateTen();
                    break;
                case NenState.Zetsu:
                    ActivateZetsu();
                    break;
                case NenState.Ren:
                    ActivateRen();
                    break;
                case NenState.Hatsu:
                    ActivateHatsu();
                    break;
                default:
                    currentNenState = NenState.Normal;
                    break;
            }
            
            OnNenStateChanged?.Invoke(currentNenState);
        }
        
        private void DeactivateCurrentState()
        {
            isTenActive = false;
            isZetsuActive = false;
            isRenActive = false;
            isHatsuActive = false;
            
            // Stop all effects
            if (tenAuraEffect != null && tenAuraEffect.isPlaying)
                tenAuraEffect.Stop();
            if (renAuraEffect != null && renAuraEffect.isPlaying)
                renAuraEffect.Stop();
            if (hatsuAuraEffect != null && hatsuAuraEffect.isPlaying)
                hatsuAuraEffect.Stop();
        }
        
        private void ActivateTen()
        {
            if (currentNen >= tenNenCost)
            {
                isTenActive = true;
                currentNenState = NenState.Ten;
                
                if (tenAuraEffect != null)
                    tenAuraEffect.Play();
                
                Debug.Log("Ten activated - Defense increased");
            }
        }
        
        private void ActivateZetsu()
        {
            isZetsuActive = true;
            currentNenState = NenState.Zetsu;
            
            // Stop all aura effects
            DeactivateCurrentState();
            isZetsuActive = true; // Set back to true after deactivation
            
            Debug.Log("Zetsu activated - Aura suppressed, harder to detect");
        }
        
        private void ActivateRen()
        {
            if (currentNen >= renNenCost)
            {
                isRenActive = true;
                currentNenState = NenState.Ren;
                
                if (renAuraEffect != null)
                    renAuraEffect.Play();
                
                Debug.Log("Ren activated - Attack power increased");
            }
        }
        
        private void ActivateHatsu()
        {
            if (currentNen >= hatsuNenCost)
            {
                isHatsuActive = true;
                currentNenState = NenState.Hatsu;
                
                if (hatsuAuraEffect != null)
                    hatsuAuraEffect.Play();
                
                Debug.Log("Hatsu activated - Special abilities available");
            }
        }
        
        // Advanced Nen Techniques
        public bool ActivateGyo()
        {
            if (canUseGyo && ConsumeNen(10f))
            {
                isGyoActive = true;
                StartCoroutine(GyoCoroutine());
                return true;
            }
            return false;
        }
        
        private IEnumerator GyoCoroutine()
        {
            // Concentrate Nen in eyes to see hidden Nen
            yield return new WaitForSeconds(5f);
            isGyoActive = false;
        }
        
        public bool ActivateIn()
        {
            if (canUseIn && ConsumeNen(15f))
            {
                isInActive = true;
                StartCoroutine(InCoroutine());
                return true;
            }
            return false;
        }
        
        private IEnumerator InCoroutine()
        {
            // Hide Nen presence while using Nen abilities
            yield return new WaitForSeconds(8f);
            isInActive = false;
        }
        
        public bool ActivateEn()
        {
            if (canUseEn && ConsumeNen(25f))
            {
                isEnActive = true;
                StartCoroutine(EnCoroutine());
                return true;
            }
            return false;
        }
        
        private IEnumerator EnCoroutine()
        {
            // Expand aura to sense everything in large area
            float enRadius = 20f * enhancementEfficiency;
            
            // Create detection sphere
            Collider[] objectsInRange = Physics.OverlapSphere(transform.position, enRadius);
            foreach (Collider obj in objectsInRange)
            {
                if (obj.CompareTag("Enemy") || obj.CompareTag("Hidden"))
                {
                    Debug.Log($"En detected: {obj.name} at {obj.transform.position}");
                }
            }
            
            yield return new WaitForSeconds(10f);
            isEnActive = false;
        }
        
        public bool ActivateShu()
        {
            if (canUseShu && ConsumeNen(20f))
            {
                isShuActive = true;
                StartCoroutine(ShuCoroutine());
                return true;
            }
            return false;
        }
        
        private IEnumerator ShuCoroutine()
        {
            // Extend aura to weapon/object
            // Increase weapon damage
            if (character != null)
            {
                character.attackDamage *= 1.5f;
            }
            
            yield return new WaitForSeconds(15f);
            
            if (character != null)
            {
                character.attackDamage /= 1.5f;
            }
            
            isShuActive = false;
        }
        
        public bool ActivateKo()
        {
            if (canUseKo && ConsumeNen(30f))
            {
                isKoActive = true;
                StartCoroutine(KoCoroutine());
                return true;
            }
            return false;
        }
        
        private IEnumerator KoCoroutine()
        {
            // Concentrate all Nen into one body part for maximum power
            if (character != null)
            {
                float originalAttack = character.attackDamage;
                character.attackDamage *= 3f; // Triple damage
                
                yield return new WaitForSeconds(3f); // Short duration, high risk
                
                character.attackDamage = originalAttack;
            }
            
            isKoActive = false;
        }
        
        public float GetNenEfficiency(NenCategory category)
        {
            switch (category)
            {
                case NenCategory.Enhancement:
                    return enhancementEfficiency;
                case NenCategory.Emission:
                    return emissionEfficiency;
                case NenCategory.Manipulation:
                    return manipulationEfficiency;
                case NenCategory.Transmutation:
                    return transmutationEfficiency;
                case NenCategory.Conjuration:
                    return conjurationEfficiency;
                case NenCategory.Specialization:
                    return specializationEfficiency;
                default:
                    return 1.0f;
            }
        }
        
        public bool IsNenStateActive(NenState state)
        {
            return currentNenState == state;
        }
        
        public float GetNenPercentage()
        {
            return currentNen / maxNenCapacity;
        }
        
        // Water Divination Test (for character creation/training)
        public NenCategory PerformWaterDivinationTest()
        {
            // This would be used in character creation or training modes
            // For now, return the primary category
            return primaryCategory;
        }
    }
    
    public enum NenState
    {
        Normal,
        Ten,      // Shroud - Defense
        Zetsu,    // Suppress - Stealth
        Ren,      // Enhance - Attack
        Hatsu     // Release - Special abilities
    }
    
    public enum NenCategory
    {
        Enhancement,    // 100% physical enhancement
        Emission,       // 100% projectile/remote attacks
        Manipulation,   // 100% control of objects/people
        Transmutation,  // 100% changing aura properties
        Conjuration,    // 100% creating objects from aura
        Specialization  // 100% unique abilities
    }
}