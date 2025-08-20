using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HunterRush.Effects
{
    /// <summary>
    /// Visual effects manager for anime-style effects, auras, and impact visuals
    /// </summary>
    public class VisualEffectsManager : MonoBehaviour
    {
        [Header("Aura Effects")]
        public GameObject nenAuraPrefab;
        public GameObject tenAuraPrefab;
        public GameObject renAuraPrefab;
        public GameObject hatsuAuraPrefab;
        
        [Header("Combat Effects")]
        public GameObject hitEffectPrefab;
        public GameObject criticalHitEffectPrefab;
        public GameObject blockEffectPrefab;
        public GameObject dodgeEffectPrefab;
        
        [Header("Character Specific Effects")]
        public GameObject gonRockEffectPrefab;
        public GameObject gonPaperEffectPrefab;
        public GameObject gonScissorsEffectPrefab;
        public GameObject killuaLightningPrefab;
        public GameObject killuaAfterimageePrefab;
        public GameObject kurapikaChainEffectPrefab;
        public GameObject leorioPortalEffectPrefab;
        
        [Header("Environment Effects")]
        public GameObject jumpEffectPrefab;
        public GameObject landingEffectPrefab;
        public GameObject wallRunEffectPrefab;
        public GameObject dashEffectPrefab;
        
        [Header("Screen Effects")]
        public GameObject screenShakeController;
        public Material speedLinesShader;
        public Material impactLinesShader;
        
        // Singleton
        public static VisualEffectsManager Instance { get; private set; }
        
        // Effect pools
        private Dictionary<string, Queue<GameObject>> effectPools = new Dictionary<string, Queue<GameObject>>();
        private List<GameObject> activeEffects = new List<GameObject>();
        
        // Screen effect state
        private bool speedLinesActive = false;
        private Coroutine screenShakeCoroutine;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeEffectPools();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeEffectPools()
        {
            // Pre-instantiate common effects for pooling
            CreateEffectPool("HitEffect", hitEffectPrefab, 10);
            CreateEffectPool("BlockEffect", blockEffectPrefab, 5);
            CreateEffectPool("JumpEffect", jumpEffectPrefab, 5);
            CreateEffectPool("LandingEffect", landingEffectPrefab, 5);
        }
        
        private void CreateEffectPool(string poolName, GameObject prefab, int poolSize)
        {
            if (prefab == null) return;
            
            Queue<GameObject> pool = new Queue<GameObject>();
            
            for (int i = 0; i < poolSize; i++)
            {
                GameObject effect = Instantiate(prefab);
                effect.SetActive(false);
                effect.transform.SetParent(transform);
                pool.Enqueue(effect);
            }
            
            effectPools[poolName] = pool;
        }
        
        private GameObject GetPooledEffect(string poolName)
        {
            if (effectPools.ContainsKey(poolName) && effectPools[poolName].Count > 0)
            {
                GameObject effect = effectPools[poolName].Dequeue();
                return effect;
            }
            return null;
        }
        
        private void ReturnEffectToPool(string poolName, GameObject effect)
        {
            if (effect != null && effectPools.ContainsKey(poolName))
            {
                effect.SetActive(false);
                effect.transform.SetParent(transform);
                effectPools[poolName].Enqueue(effect);
            }
        }
        
        // Aura Effects
        public GameObject CreateAuraEffect(Vector3 position, Color color, AuraType type, float duration = 2f)
        {
            GameObject auraPrefab = GetAuraPrefab(type);
            if (auraPrefab == null) return null;
            
            GameObject aura = Instantiate(auraPrefab, position, Quaternion.identity);
            
            // Set aura color
            ParticleSystem[] particles = aura.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                var main = ps.main;
                main.startColor = color;
            }
            
            // Auto-destroy
            Destroy(aura, duration);
            activeEffects.Add(aura);
            
            return aura;
        }
        
        private GameObject GetAuraPrefab(AuraType type)
        {
            switch (type)
            {
                case AuraType.Nen:
                    return nenAuraPrefab;
                case AuraType.Ten:
                    return tenAuraPrefab;
                case AuraType.Ren:
                    return renAuraPrefab;
                case AuraType.Hatsu:
                    return hatsuAuraPrefab;
                default:
                    return nenAuraPrefab;
            }
        }
        
        // Combat Effects
        public void CreateHitEffect(Vector3 position, bool isCritical = false)
        {
            GameObject effectPrefab = isCritical ? criticalHitEffectPrefab : hitEffectPrefab;
            string poolName = isCritical ? "CriticalHitEffect" : "HitEffect";
            
            GameObject effect = GetPooledEffect(poolName);
            if (effect == null && effectPrefab != null)
            {
                effect = Instantiate(effectPrefab);
            }
            
            if (effect != null)
            {
                effect.transform.position = position;
                effect.SetActive(true);
                
                StartCoroutine(ReturnEffectAfterDelay(poolName, effect, 1f));
            }
        }
        
        public void CreateBlockEffect(Vector3 position, Vector3 direction)
        {
            GameObject effect = GetPooledEffect("BlockEffect");
            if (effect == null && blockEffectPrefab != null)
            {
                effect = Instantiate(blockEffectPrefab);
            }
            
            if (effect != null)
            {
                effect.transform.position = position;
                effect.transform.LookAt(position + direction);
                effect.SetActive(true);
                
                StartCoroutine(ReturnEffectAfterDelay("BlockEffect", effect, 0.5f));
            }
        }
        
        // Character Specific Effects
        public void CreateJajankenEffect(Vector3 position, JajankenType type, float chargeLevel)
        {
            GameObject effectPrefab = null;
            
            switch (type)
            {
                case JajankenType.Rock:
                    effectPrefab = gonRockEffectPrefab;
                    break;
                case JajankenType.Paper:
                    effectPrefab = gonPaperEffectPrefab;
                    break;
                case JajankenType.Scissors:
                    effectPrefab = gonScissorsEffectPrefab;
                    break;
            }
            
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
                
                // Scale effect based on charge level
                effect.transform.localScale = Vector3.one * (1f + chargeLevel);
                
                Destroy(effect, 3f);
                activeEffects.Add(effect);
            }
        }
        
        public void CreateLightningEffect(Vector3 position, Vector3 direction, float intensity = 1f)
        {
            if (killuaLightningPrefab != null)
            {
                GameObject lightning = Instantiate(killuaLightningPrefab, position, Quaternion.LookRotation(direction));
                lightning.transform.localScale = Vector3.one * intensity;
                
                Destroy(lightning, 1.5f);
                activeEffects.Add(lightning);
            }
        }
        
        public void CreateChainEffect(Vector3 startPos, Vector3 endPos, Color chainColor)
        {
            if (kurapikaChainEffectPrefab != null)
            {
                GameObject chain = Instantiate(kurapikaChainEffectPrefab, startPos, Quaternion.identity);
                
                // Stretch chain between positions
                Vector3 direction = endPos - startPos;
                chain.transform.LookAt(endPos);
                chain.transform.localScale = new Vector3(1f, 1f, direction.magnitude);
                
                // Set chain color
                Renderer renderer = chain.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = chainColor;
                }
                
                Destroy(chain, 2f);
                activeEffects.Add(chain);
            }
        }
        
        public void CreatePortalEffect(Vector3 position)
        {
            if (leorioPortalEffectPrefab != null)
            {
                GameObject portal = Instantiate(leorioPortalEffectPrefab, position, Quaternion.identity);
                
                Destroy(portal, 3f);
                activeEffects.Add(portal);
            }
        }
        
        // Movement Effects
        public void CreateJumpEffect(Vector3 position)
        {
            GameObject effect = GetPooledEffect("JumpEffect");
            if (effect == null && jumpEffectPrefab != null)
            {
                effect = Instantiate(jumpEffectPrefab);
            }
            
            if (effect != null)
            {
                effect.transform.position = position;
                effect.SetActive(true);
                
                StartCoroutine(ReturnEffectAfterDelay("JumpEffect", effect, 1f));
            }
        }
        
        public void CreateLandingEffect(Vector3 position, float intensity = 1f)
        {
            GameObject effect = GetPooledEffect("LandingEffect");
            if (effect == null && landingEffectPrefab != null)
            {
                effect = Instantiate(landingEffectPrefab);
            }
            
            if (effect != null)
            {
                effect.transform.position = position;
                effect.transform.localScale = Vector3.one * intensity;
                effect.SetActive(true);
                
                StartCoroutine(ReturnEffectAfterDelay("LandingEffect", effect, 1f));
            }
        }
        
        public void CreateDashEffect(Vector3 position, Vector3 direction)
        {
            if (dashEffectPrefab != null)
            {
                GameObject effect = Instantiate(dashEffectPrefab, position, Quaternion.LookRotation(direction));
                Destroy(effect, 1f);
                activeEffects.Add(effect);
            }
        }
        
        // Screen Effects
        public void ActivateSpeedLines(float duration = 2f)
        {
            if (!speedLinesActive)
            {
                StartCoroutine(SpeedLinesCoroutine(duration));
            }
        }
        
        private IEnumerator SpeedLinesCoroutine(float duration)
        {
            speedLinesActive = true;
            
            // Apply speed lines shader to camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null && speedLinesShader != null)
            {
                // TODO: Apply post-processing effect
            }
            
            yield return new WaitForSeconds(duration);
            
            // Remove speed lines effect
            speedLinesActive = false;
        }
        
        public void TriggerScreenShake(float intensity, float duration)
        {
            if (screenShakeCoroutine != null)
                StopCoroutine(screenShakeCoroutine);
            
            screenShakeCoroutine = StartCoroutine(ScreenShakeCoroutine(intensity, duration));
        }
        
        private IEnumerator ScreenShakeCoroutine(float intensity, float duration)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) yield break;
            
            Vector3 originalPosition = mainCamera.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                
                mainCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            mainCamera.transform.localPosition = originalPosition;
        }
        
        // Utility
        private IEnumerator ReturnEffectAfterDelay(string poolName, GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnEffectToPool(poolName, effect);
        }
        
        public void ClearAllEffects()
        {
            foreach (GameObject effect in activeEffects)
            {
                if (effect != null)
                    Destroy(effect);
            }
            activeEffects.Clear();
        }
        
        void OnDestroy()
        {
            ClearAllEffects();
        }
    }
    
    public enum AuraType
    {
        Nen,
        Ten,
        Ren,
        Hatsu
    }
    
    public enum JajankenType
    {
        Rock,
        Paper,
        Scissors
    }
}