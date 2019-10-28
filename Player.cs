using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameSystem;
using BlueprintSystem;

namespace EntitySystem
{
    public class Player : MonoBehaviour
    {
        #region BLUEPRINT CONFIGURATION
        

        internal enum ActiveHotbar
        {
            SlotOne,
            SlotTwo,
            SlotThree,
            SlotFour,
            SlotFive,
            SlotSix,
            SlotSeven,
            SlotEight,
            SlotNine,
            SlotTen
        }

        internal ActiveHotbar hotbarState;

        [SerializeField]
        private BlueprintControlSystem blueprintControlSystem;






        #endregion


        #region STATS
        #region HEALTH
        [Header("HEALTH")]
        [SerializeField]
        private int base_maxVitality; //Starting Health
        internal float active_maxVitality; //Current Max Health
        internal float active_currentVitality; //Current Health
        internal bool isDead;
        [SerializeField]
        private float base_vitalityRegen; //Starting amount healed per second
        internal float enhanced_vitalityRegen; //Active amount healed per second **** THIS IS AN ENHANCER ****
        private bool regenActive; //If true, is regening vitality.
        [SerializeField]
        private float base_naniteCostToRegen; //Starting cost of nanites per 1 vitality point.
        internal float active_naniteCostToRegen; //Active cost of nanites per 1 vitalitiy point.
        private bool canHeal; //If true, run healing functions.
        #endregion
        #region NANITES
        [Header("NANITES")]
        [Space]
        [SerializeField]
        private float base_maxNanites; //Starting max nanites
        internal float active_maxNanites; //Current max nanites
        internal float active_currentNanites; //Current nanites
        private float delayToRegen; //How long after taking damage to regen.
        [SerializeField]
        private float base_naniteAssemblyRate; //Starting nanites built per second
        internal float enhanced_naniteAssemblyRate; //Current nanites built per second **** THIS IS AN ENHANCER ****
        private bool assemblyActive; //If true, is building nanites.
        #endregion
        #region ARMOR
        [Header("ARMOR")]
        [Tooltip("Armor has a direct effect on damage being received.")]
        [SerializeField]
        internal float playerArmor; //Players have different armor ratings based on character.
        #endregion
        #region SHIELDS
        [Header("SHIELDS")]
        [SerializeField]
        private float base_maxShields; //Starting shields
        internal float active_maxShields; //Current max shields
        internal float active_currentShields; //Current shields
        [SerializeField]
        private float base_delayToRegenShields; //Starting amount of time in seconds until shields can regen.
        internal float active_delayToRegenShields; //Current amount of time in seconds until shields can regen. 
        internal float active_shieldRegenDelayCountdown; //Set to = active_delayToRegen and count down x*Time.deltaTime.
        [SerializeField]
        private float base_shieldRegenRate; //Starting amount of shields regenerated per second.
        internal float active_shieldRegenRate; //Current amount of shields regenerated per second. **** IS CONSTANT % OF SHIELDS ****
        internal bool shieldRegenActive; //If true, is regenerating shields.
        #endregion
        #region ENHANCERS
        [Space(3)]
        [Header("ENHANCER INTEGRATION")]




        [Space]
        [Header("ENHANCER CONFIGURATION")]

        [Space]
        [SerializeField]
        internal float base_movementSpeed; //50 (Factored as a percentage, cannot be 0.)
        [SerializeField]
        internal float enhanced_movementSpeed; //Is a multiplication of the base movement speed applied as a boost. Needs to be referenced by the controller.

        [Space]
        [SerializeField]
        internal float base_sprintBoost; //25 (Factored as a percentage, cannot be 0.)
        [SerializeField]
        internal float enhanced_sprintBoost; //Adds a multiplication of the base sprint value. Needs to be referenced by the controller or overwrite the controller's sprint value.
        
        [Space]
        [SerializeField]
        internal float base_attackSpeedBoost; //1 (Factored as a percentage, cannot be 0.)
        [SerializeField]
        internal float enhanced_attackSpeedBoost; //Adds a multiplication factor for blueprints to reference for casting speed.
        
        [Space]
        [SerializeField]
        internal int base_jumpCount; //1
        [SerializeField]
        internal int enhanced_jumpCount; //Adds a enhancer amount to the jump counter.
        internal int availableJumps; //Is maxed at the enhanced jump count. Is reset when grounded.

        [Space]
        [SerializeField]
        internal int base_dashCount; //1
        [SerializeField]
        internal int enhanced_dashCount; //Adds a enhancer amount to the dash count limit.
        internal int availableDashes;

        [Space]
        [SerializeField]
        internal int base_maxSentries; //2
        [SerializeField]
        internal int enhanced_maxSentries; //Adds a enhancer amount to the max active turrets.
        [SerializeField]
        internal List<GameObject> sentriesInCommission;

        [Space]
        [SerializeField]
        internal int base_maxProxies; //1
        [SerializeField]
        internal int enhanced_maxProxies; //Adds a enhancer amount to the max active takeovers.
        [SerializeField]
        internal List<GameObject> proxiesInCommission;

        [Space]
        [SerializeField]
        internal float base_blockChance; //0 (Not calculating percentage, simply stating it as a * 100 value.)
        [SerializeField]
        internal float enhanced_blockChance; //Adds an initial %, then subsequent enhancements add an inversely scaled %. IE 10(%) + 10% * (100 - current%) = 19% 
        
        [Space]
        [SerializeField]
        internal int base_criticalHitChance; //0 (Not calculating percentage, simply stating it as a * 100 value.)
        [SerializeField]
        internal int enhanced_criticalHitChance; //Adds a enhancer % increase, up to 100%. 0-100 int

        [Space]
        [SerializeField]
        internal int base_bossDamage; //0 (NOTE: Is factored as a % of BASE DAMAGE, not BOSS DAMAGE, and can be 0
        [SerializeField]
        internal int enhanced_bossDamageBoost; //Adds a enhancer + bonus to damage for bosses. 
        
        //ENHANCER: VITALITY REGEN > LOCATED IN STATS:HEALTH

        //ENHANCER: NANITE REGEN > LOCATED IN STATS:NANITES

        [Space]
        [SerializeField]
        internal int base_luckFactor; //0 Luck factor can be 0, but need to protect from it.
        [SerializeField]
        internal int enhanced_luckFactor; //Difficulty to implement. Drops are calculated using a 0 - 1000 scale. Basically, this value replaces the "0" so the odds of getting more rare items goes up until == 1000.
        //Display as 1000 - luckFactor / luckFactor == chance for Most rare items.

        [Space]
        [SerializeField]
        internal float base_dashCooldown; //3 (calculates the -% cannot be 0)
        [SerializeField]
        internal float enhanced_dashCooldown; //Uses inverse % algo to reduce dash charge rate by a current% + increase% * (time - current%)
       
        [Space]
        [SerializeField]
        internal float base_abilityCooldown; //10 (calculates the -% cannot be 0)
        [SerializeField]
        internal float enhanced_abilityCooldown; //Same as above.

        [Space]
        [SerializeField]
        internal float base_ultimateCooldown; //60 (calculates the -% cannot be 0)
        [SerializeField]
        internal float enhanced_ultimateCooldown; //Same as above

        [Space]
        [SerializeField]
        internal int base_naniteSurges; //0 (NO PERCENTAGE CALCULATED)
        [SerializeField]
        internal int enhanced_naniteSurges; //Default is 0. Can be temporarily buffed by 1 or permanently raised by ultra rare enhancement. Player doesn't actually die, but goes into "Instant Heal" when 0 vitality. Consumes 100% of the nanite pool, period.
        
        #endregion
        #endregion
        #region ABILITIES
        private EntitySystem_Abilities playerAbilitySystem;
        [SerializeField]
        private EntitySystem_Abilities.Abilities currentAbility;
        internal bool canUseAbility;
        internal bool abilityIsActive;
        private float timeUntilAbilityCharge;
        #endregion
        


        #region INTERNALLY SET
        internal EntitySystem_Core entityCore;
        internal Rigidbody playerRigidbody;
        internal PlayerSystem.PlayerController playerController;
        #endregion

        #region GCStringCollection
        private string vitalityUpdateString;
        private string naniteUpdateString;
        #endregion

        #region MELEE 
        internal float active_meleeDamage;

        #endregion

        #region SUMMONING 
        [SerializeField]
        internal Transform summoningPoint;

        #endregion

        #region FLUX RADIATION
        [SerializeField]
        internal float startingFlux;
        internal float currentFlux;
        internal int totalEnhancements; //Used to configure cost of next enhancement, etc. Is shown as "Level: L## + E##" on the HUD 


        #endregion

        void Start()
        {
            AssignReferences();
            SetActiveValues();
            SetHotbarDefaultState();
            StartEnhancerWindow();
            UpdateHUD();
        }

        void Update()
        {
            if(isDead == true)
            {
                return;
            }

            #region CHECK FOR REGEN
            if (EntityOperations.NeedToRegenerate(active_maxVitality, active_currentVitality)) //Vitality
            {
                if(delayToRegen > 0)
                {
                    delayToRegen -= 1 * Time.deltaTime;
                }
                else
                {
                    EntityOperations.RegenerateVitality(this, enhanced_vitalityRegen, active_naniteCostToRegen);
                }
            }
            if(EntityOperations.NeedToRegenerate(active_maxNanites, active_currentNanites)) //Nanites
            {
                EntityOperations.RegenerateNanites(this, enhanced_naniteAssemblyRate);
            }


            #endregion
            #region COOLDOWNS
            AbilityCooldown();
            #endregion
            #region HUD UPDATES
            UpdateHUD();
            #endregion
            #region CORE LINK
            KeepCoreUpToDate();

            #endregion
        }

        #region START CALLS
        private void AssignReferences()
        {
            entityCore = this.GetComponent<EntitySystem_Core>();
            playerRigidbody = this.GetComponent<Rigidbody>();
            playerController = this.GetComponent<PlayerSystem.PlayerController>();
            playerAbilitySystem = this.GetComponent<EntitySystem_Abilities>();

            entityCore.entityInfo.entityOwner = entityCore.entityInfo.entityId; //We own ourselves.
            
        }
        private void SetActiveValues()
        {
            //VITALITY
            active_maxVitality = base_maxVitality;
            active_currentVitality = active_maxVitality;
            enhanced_vitalityRegen = base_vitalityRegen;
            //NANITES
            active_maxNanites = base_maxNanites;
            active_currentNanites = active_maxNanites;
            active_naniteCostToRegen = base_naniteCostToRegen;
            enhanced_naniteAssemblyRate = base_naniteAssemblyRate;
            //SHIELDS
            active_maxShields = base_maxShields;
            active_currentShields = active_maxShields;
            active_delayToRegenShields = base_delayToRegenShields;
            //ENHANCERS
            enhanced_movementSpeed = base_movementSpeed;
            enhanced_sprintBoost = base_sprintBoost;
            enhanced_attackSpeedBoost = base_attackSpeedBoost;
            enhanced_jumpCount = base_jumpCount;
            enhanced_dashCount = base_dashCount;
            enhanced_maxSentries = base_maxSentries;
            enhanced_maxProxies = base_maxProxies;
            enhanced_blockChance = base_blockChance;
            enhanced_criticalHitChance = base_criticalHitChance;
            enhanced_bossDamageBoost = base_bossDamage;
            enhanced_luckFactor = base_luckFactor;
            enhanced_dashCooldown = base_dashCooldown;
            enhanced_abilityCooldown = base_abilityCooldown;
            enhanced_ultimateCooldown = base_ultimateCooldown;
            enhanced_naniteSurges = base_naniteSurges;
            //MELEE
            active_meleeDamage = entityCore.active_baseDamage;
            //BOOLS
            canUseAbility = true;
            //ANIMATION
            entityCore.thisAnimator.SetLayerWeight(1, 1);
        }
        private void SetHotbarDefaultState()
        {
            SelectHotbarSlot(0); //Select first slot by default.
        }
        #endregion
        #region ACTIONS
        internal void Damaged(float rawDamage)
        {
            delayToRegen = 3; //Reset the healing cooldown to 3 seconds.
            active_currentVitality -= TakeDamage.DamageCalculation(rawDamage);
            active_currentVitality = Mathf.Clamp(active_currentVitality, -100, active_maxVitality);
        }

        #endregion
        #region ABILITY FUNCTIONS
        internal void ActivateAbility()
        {
            if (playerAbilitySystem.CanUseAbility() == true)
            {
                canUseAbility = false;
                abilityIsActive = true;

                if (currentAbility == EntitySystem_Abilities.Abilities.HighJump)
                {
                    playerAbilitySystem.Ability_HighJump(playerRigidbody);
                }

                if (currentAbility == EntitySystem_Abilities.Abilities.Invisibility)
                {
                    foreach (SkinnedMeshRenderer skinRend in entityCore.entityRenderers)
                    {
                        playerAbilitySystem.Ability_Invisibility(entityCore, skinRend.materials, skinRend);
                    }
                }
            }
        }

        private void AbilityCooldown()
        {
            if (canUseAbility == false)
            {
                if (timeUntilAbilityCharge > 0)
                {
                    timeUntilAbilityCharge -= 1 * Time.deltaTime;

                    //UpdateHUD(); REPLACE WITH DIFFERENT HUD CALL
                }
                else
                {
                    timeUntilAbilityCharge = enhanced_abilityCooldown;
                    canUseAbility = true;
                    //UpdateHUD(); REPLACE WITH DIFFERENT HUD CALL
                }
            }
        }
        #endregion
        #region ENHANCER FUNCTIONS
        private void StartEnhancerWindow()
        {
            for (int i = 0; i < 17; i++)
            {
                ApplyEnhancer(i, 0); //Set start stuff
            }
        }

        enum EnhancerType
        {
            MovementSpeed,
            SprintBoost,
            AttackSpeed,
            JumpCount,
            DashCount,
            MaxSentries,
            MaxProxies,
            BlockChance,
            CriticalHitChance,
            BossDamageBoost,
            VitalityRegen,
            NaniteRegen,
            LuckFactor,
            CooldownDash,
            CooldownAbility,
            CooldownUltimate,
            NaniteSurges
        }

        internal void ApplyEnhancer(int slotNumber, float changeAmount)
        {
            // Debug.Log("Apply enhancer, " + slotNumber + " with " + changeAmount);
            if ((int)EnhancerType.MovementSpeed == slotNumber)
            {
                float oldValue = enhanced_movementSpeed; //Store the old value
                enhanced_movementSpeed = enhanced_movementSpeed + changeAmount; //Flat number change

                float newValue = enhanced_movementSpeed; //Store the new value
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.SprintBoost == slotNumber)
            {
                float oldValue = enhanced_sprintBoost;
                enhanced_sprintBoost = enhanced_sprintBoost + changeAmount; //Flat number change

                float newValue = enhanced_sprintBoost;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.AttackSpeed == slotNumber)
            {
                float oldValue = enhanced_attackSpeedBoost;
                enhanced_attackSpeedBoost = enhanced_attackSpeedBoost + changeAmount; //Flat number change. Is a float value. Visualized as value * 100, so "100%, 110%" etc.

                float newValue = enhanced_attackSpeedBoost;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.JumpCount == slotNumber)
            {
                float oldValue = enhanced_jumpCount;
                enhanced_jumpCount = enhanced_jumpCount + Mathf.RoundToInt(changeAmount); //Flat number change.

                float newValue = enhanced_jumpCount;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.DashCount == slotNumber)
            {
                float oldValue = enhanced_dashCount;
                enhanced_dashCount = enhanced_dashCount + Mathf.RoundToInt(changeAmount); //Flat number change.

                float newValue = enhanced_dashCount;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.MaxSentries == slotNumber)
            {
                float oldValue = enhanced_maxSentries;
                enhanced_maxSentries = enhanced_maxSentries + Mathf.RoundToInt(changeAmount); //Flat number change.

                float newValue = enhanced_maxSentries;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.MaxProxies == slotNumber)
            {
                float oldValue = enhanced_maxProxies;
                enhanced_maxProxies = enhanced_maxProxies + Mathf.RoundToInt(changeAmount);

                float newValue = enhanced_maxProxies;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.BlockChance == slotNumber)
            {
                float oldValue = enhanced_blockChance;
                enhanced_blockChance = enhanced_blockChance + (enhanced_blockChance - (enhanced_blockChance * changeAmount)); //Percentage increase. Is 0 - 100 10 + (10 - (10*.1)[1] = 19

                float newValue = enhanced_blockChance;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.CriticalHitChance == slotNumber)
            {
                float oldValue = enhanced_criticalHitChance;
                enhanced_criticalHitChance = enhanced_criticalHitChance + Mathf.RoundToInt(changeAmount); //Flat number change. 0 - 100 % value.

                float newValue = enhanced_criticalHitChance;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.BossDamageBoost == slotNumber)
            {
                float oldValue = enhanced_bossDamageBoost;
                enhanced_bossDamageBoost = enhanced_bossDamageBoost + Mathf.RoundToInt(changeAmount); //Flat number change, represented as a % of the base damage, not base boss damage.

                float newValue = enhanced_bossDamageBoost;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.VitalityRegen == slotNumber)
            {
                float oldValue = enhanced_vitalityRegen;
                enhanced_vitalityRegen = enhanced_vitalityRegen + Mathf.RoundToInt(changeAmount);

                float newValue = enhanced_vitalityRegen;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.NaniteRegen == slotNumber)
            {
                float oldValue = enhanced_naniteAssemblyRate;
                enhanced_naniteAssemblyRate = enhanced_naniteAssemblyRate + Mathf.RoundToInt(changeAmount);

                float newValue = enhanced_naniteAssemblyRate;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.LuckFactor == slotNumber)
            {
                float oldValue = enhanced_luckFactor;
                enhanced_luckFactor = enhanced_luckFactor + Mathf.RoundToInt(changeAmount);

                float newValue = enhanced_luckFactor;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.CooldownDash == slotNumber)
            {
                float oldValue = enhanced_dashCooldown;
                enhanced_dashCooldown = enhanced_dashCooldown + changeAmount; //Add NEGATIVE values to remove time!

                float newValue = enhanced_dashCooldown;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.CooldownAbility == slotNumber)
            {
                float oldValue = enhanced_abilityCooldown;
                enhanced_abilityCooldown = enhanced_abilityCooldown + changeAmount;

                float newValue = enhanced_abilityCooldown;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.CooldownUltimate == slotNumber)
            {
                float oldValue = enhanced_ultimateCooldown;
                enhanced_ultimateCooldown = enhanced_ultimateCooldown + changeAmount;

                float newValue = enhanced_ultimateCooldown;
                UpdateEnhancerWindow(slotNumber);
            }
            else if ((int)EnhancerType.NaniteSurges == slotNumber)
            {
                float oldValue = enhanced_naniteSurges;
                enhanced_naniteSurges = enhanced_naniteSurges + Mathf.RoundToInt(changeAmount);

                float newValue = enhanced_naniteSurges;
                UpdateEnhancerWindow(slotNumber);
                //StartCoroutine(EnhancerUpgradeEffect(slotNumber, oldValue, newValue));
            }
        }
        #endregion
        #region HUD CALLS
        private void UpdateHUD()
        {
            #region Vitality, Nanite, Shield Bars 
            GameSystem_UIManager.Instance.image_vitalityBarFill.fillAmount = active_currentVitality / active_maxVitality;
            GameSystem_UIManager.Instance.text_vitalityAmount.text = Mathf.RoundToInt(active_currentVitality) + " / " + active_maxVitality;
            GameSystem_UIManager.Instance.image_naniteBarFill.fillAmount = active_currentNanites / active_maxNanites;
            GameSystem_UIManager.Instance.text_naniteAmount.text = $"{Mathf.RoundToInt(active_currentNanites)} / {active_maxNanites}";
            #endregion
        }
        internal void UpdateEnhancerWindow(int slotNumber)
        {
            // Debug.Log("Update enhancer window, " + slotNumber);
            if ((int)EnhancerType.MovementSpeed == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + (enhanced_movementSpeed / base_movementSpeed) * 100 + " %"; //50 + 10 = 20% increase >> 10 / 50 * 100 = 20%
            }
            else if ((int)EnhancerType.SprintBoost == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + (enhanced_sprintBoost / base_sprintBoost) * 100 + "%"; //25 + 5 = 20% increase >> 5 / 25 * 100 = 20%
            }
            else if ((int)EnhancerType.AttackSpeed == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + Math.Round(enhanced_attackSpeedBoost / base_attackSpeedBoost, 2) * 100 + "%"; //1.5 * 100 = 150, -100 == "+50%"
            }
            else if ((int)EnhancerType.JumpCount == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + enhanced_jumpCount.ToString();
            }
            else if ((int)EnhancerType.DashCount == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + enhanced_dashCount.ToString();
            }
            else if ((int)EnhancerType.MaxSentries == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + enhanced_maxSentries.ToString();
            }
            else if ((int)EnhancerType.MaxProxies == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + enhanced_maxProxies.ToString();
            }
            else if ((int)EnhancerType.BlockChance == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = enhanced_blockChance + "%"; //10% = 10/100 * 100, bc represented in 0-100 format  (newValue / 100) * 100 = "(0 / 100) * 100 = 0", "(17.3 / 100) * 100 = 17.3%
            }
            else if ((int)EnhancerType.CriticalHitChance == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = enhanced_criticalHitChance + "%";
            }
            else if ((int)EnhancerType.BossDamageBoost == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + (Mathf.RoundToInt(enhanced_bossDamageBoost) / entityCore.active_baseDamage) * 100 + "%"; //3 / 10 = .3 * 100 = 30%
            }
            else if ((int)EnhancerType.VitalityRegen == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + enhanced_vitalityRegen + "/s"; //Regen is a set amount /s
            }
            else if ((int)EnhancerType.NaniteRegen == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "+" + enhanced_naniteAssemblyRate + "/s";
            }
            else if ((int)EnhancerType.LuckFactor == slotNumber)
            {
                if(GameSystem_EntityManager.Instance.players.Count > 1)
                {
                    if (AKUtilities.Utilities.GetPlayerLuck() > 995 * GameSystem_EntityManager.Instance.players.Count)
                    {
                        GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "100%"; //At luck factor 996 or more, we have 100% chance to get legendary drops.
                    }
                    GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = (5 / ((1000 * GameSystem_EntityManager.Instance.players.Count) - enhanced_luckFactor)) + "%"; //5 / (1000 - luckFactor)

                }
                else
                {
                    if (enhanced_luckFactor > 995)
                    {
                        GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "100%"; //At luck factor 996 or more, we have 100% chance to get legendary drops.
                    }
                    GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = (5 / (1000 - enhanced_luckFactor)) + "%"; //5 / (1000 - luckFactor)
                }
            }
            else if ((int)EnhancerType.CooldownDash == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "-" + Math.Round(enhanced_dashCooldown / base_dashCooldown, 2) + "%"; //10 seconds EX: 10 - (.10 * current) = 9(r), (r)/(base) = % reduction
            }
            else if ((int)EnhancerType.CooldownAbility == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "-" + Math.Round(enhanced_abilityCooldown / base_abilityCooldown, 2) + "%";
            }
            else if ((int)EnhancerType.CooldownUltimate == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = "-" + Math.Round(enhanced_ultimateCooldown / base_ultimateCooldown, 2) + "%";
            }
            else if ((int)EnhancerType.NaniteSurges == slotNumber)
            {
                GameSystem_UIManager.Instance.list_enhancerSlots[slotNumber].text_enhancerModifier.text = enhanced_naniteSurges.ToString();
            }
        }

        #endregion
        #region Hotbar Selection
        internal void SelectHotbarSlot(int slotNumber)
        {
            //currentHotbarAsInt = slotNumber;

            if (slotNumber == 0)
            {
                hotbarState = ActiveHotbar.SlotOne;
            }
            else if (slotNumber == 1)
            {
                hotbarState = ActiveHotbar.SlotTwo;
            }
            else if (slotNumber == 2)
            {
                hotbarState = ActiveHotbar.SlotThree;
            }
            else if (slotNumber == 3)
            {
                hotbarState = ActiveHotbar.SlotFour;
            }
            else if (slotNumber == 4)
            {
                hotbarState = ActiveHotbar.SlotFive;
            }
            else if (slotNumber == 5)
            {
                hotbarState = ActiveHotbar.SlotSix;
            }
            else if (slotNumber == 6)
            {
                hotbarState = ActiveHotbar.SlotSeven;
            }
            else if (slotNumber == 7)
            {
                hotbarState = ActiveHotbar.SlotEight;
            }
            else if (slotNumber == 8)
            {
                hotbarState = ActiveHotbar.SlotNine;
            }
            else if (slotNumber == 9)
            {
                hotbarState = ActiveHotbar.SlotTen;
            }

            for (int i = 0; i < 10; i++)
            {
                if (i != slotNumber)
                {
                    if (blueprintControlSystem.blueprintSlots[i].isEmpty == false) //If there is a blueprint interface inside the slot...
                    {
                        blueprintControlSystem.blueprintSlots[i].blueprint.isActive = false; //Disable every blueprint interface that isn't selected.
                    }
                    GameSystem_UIManager.Instance.list_hotbarSlots[i].bool_isActive = false; //Update the hud
                    GameSystem_UIManager.Instance.list_hotbarSlots[i].image_activeBlueprintBackground.color = GameSystem_UIManager.Instance.color_inactiveHotbarSlot; //Set the inactive color.
                    blueprintControlSystem.blueprintSlots[i].isActive = false; //Change the state of the blueprint for later refences
                }
                else if (i == slotNumber)
                {
                    Debug.Log("Slot number: " + slotNumber);
                    if (blueprintControlSystem.blueprintSlots[i].isEmpty == false)
                    {
                        blueprintControlSystem.blueprintSlots[i].blueprint.isActive = true; //Enable the selected blueprint
                        entityCore.targetingType = blueprintControlSystem.blueprintSlots[i].blueprint.targetingType; //Sets our current targeting type based on the blueprint selected!
                        GameSystem_UIManager.Instance.text_activeBlueprintName.text = blueprintControlSystem.blueprintSlots[i].blueprintName;
                        GameSystem_UIManager.Instance.text_activeBlueprintDescription.text = blueprintControlSystem.blueprintSlots[i].blueprintDescription;
                        GameSystem_UIManager.Instance.image_activeBlueprintElementIcon.sprite = blueprintControlSystem.blueprintSlots[i].blueprintElementIcon;
                    }
                    GameSystem_UIManager.Instance.list_hotbarSlots[i].bool_isActive = true; //Update the hud
                    GameSystem_UIManager.Instance.list_hotbarSlots[i].image_activeBlueprintBackground.color = GameSystem_UIManager.Instance.color_activeHotbarSlot; //Set the active color.

                    blueprintControlSystem.blueprintSlots[i].isActive = true; //Change the state of the blueprint for later refences
                }
            }
        }
        #endregion
        #region BLUEPRINT FUNCTIONS
        internal void DropBlueprint()
        {
            int slotNumber = (int)hotbarState;
            //Set the item to dropped
            if (blueprintControlSystem.blueprintSlots[slotNumber].isEmpty == true)
            {
                return;
            }
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemObject.transform.parent = null; //Remove us as the parent of the object.
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemSystem.enabled = true; //Re-enable the blueprint's item script.
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemSystem.DropItem(this.gameObject.transform.position); //Set drop position and all the other stuff the item needs to do as a dropped item
            blueprintControlSystem.blueprintSlots[slotNumber].isEmpty = true; //Update our storage to say we're empty.


            //Update HUD
            GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_blueprintCost.text = "Empty"; //Update the visual cost to say it's empty
            GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].image_blueprintPicture.sprite = GameSystem_UIManager.Instance.sprite_emptyHotbarSlot; //Remove the icon and replace it with the empty hotbar icon
            GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_blueprintStackCount.text = "0";

        }

        internal void CheckForEmptyBlueprintSlot(GameObject blueprintItemObject, string itemId) //THIS is called when a blueprint is trying to be "picked up"! It checks if we have an available slot every frame we're inside it's collider!
        {
            for (int i = 0; i < blueprintControlSystem.blueprintSlots.Capacity; i++) //Check for duplicates first.
            {
                if (blueprintControlSystem.blueprintSlots[i].isEmpty == false)
                {
                    if (blueprintControlSystem.blueprintSlots[i].blueprintItemSystem.itemId == itemId)
                    {
                        PickupDuplicateBlueprint(i);
                        Destroy(blueprintItemObject); //We apply duplication to the current blueprint interface and destroy this item.
                        return;
                    }
                }
            }

            for (int i = 0; i < blueprintControlSystem.blueprintSlots.Capacity; i++) //Changed to an int from a list.count due to list being empty.
            {
                if (blueprintControlSystem.blueprintSlots[i].isEmpty == true) //Check all of our storage slots for the first empty slot.
                {
                    BlueprintPickedUp(blueprintItemObject, i); //Parse our blueprint and the available slot to the blueprint pickup.
                    return;
                }
            }
        }

        internal void BlueprintPickedUp(GameObject blueprintPickup, int slotNumber)
        {
            //Physically pickup the blueprint
            blueprintPickup.transform.parent = this.gameObject.transform;
            blueprintPickup.transform.localPosition = Vector3.zero;
            //Occupy the slot
            blueprintControlSystem.blueprintSlots[slotNumber].isEmpty = false;
            //Configure our slot!
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemObject = blueprintPickup; //Store our object reference
            blueprintControlSystem.blueprintSlots[slotNumber].blueprint = blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemObject.GetComponent<Blueprint>(); //Store our blueprint's weapon system reference
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemSystem = blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemObject.GetComponent<ItemSystem.Item>(); //Store our blueprint's item system reference
            //Tell the item we've picked it up!
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemSystem.PickUpItem();
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintName = blueprintControlSystem.blueprintSlots[slotNumber].blueprint.blueprintName; //Store our blueprint's name
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintCost = blueprintControlSystem.blueprintSlots[slotNumber].blueprint.naniteCost; //Store our blueprint's nanite cost
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintStackValue = blueprintControlSystem.blueprintSlots[slotNumber].blueprint.stackValue; //Store our blueprint's stack value
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintIcon = blueprintControlSystem.blueprintSlots[slotNumber].blueprint.blueprintImage; //Store our blueprint's icon
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintDescription = blueprintControlSystem.blueprintSlots[slotNumber].blueprint.blueprintDescription; //Store our blueprints description
            //Give it important references
            blueprintControlSystem.blueprintSlots[slotNumber].blueprint.entityCore = entityCore;
            blueprintControlSystem.blueprintSlots[slotNumber].blueprint.slotNumber = slotNumber;
            //Stop the item system from running unneeded update calls.
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintItemSystem.enabled = false;
            //Check to see if this is the currently active blueprint.
            int value = (int)hotbarState; //Cast our enum to an int value.
            if (value == slotNumber) //Check to see if our current hotbar is the same as the replaced slot
            {
                blueprintControlSystem.blueprintSlots[slotNumber].blueprint.isActive = true;
                GameSystem_UIManager.Instance.text_activeBlueprintName.text = blueprintControlSystem.blueprintSlots[slotNumber].blueprintName;
                GameSystem_UIManager.Instance.text_activeBlueprintDescription.text = blueprintControlSystem.blueprintSlots[slotNumber].blueprintDescription;
                GameSystem_UIManager.Instance.image_activeBlueprintElementIcon.sprite = blueprintControlSystem.blueprintSlots[slotNumber].blueprintElementIcon;
            }
            //Update our Hud!

            GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_blueprintCost.text = blueprintControlSystem.blueprintSlots[slotNumber].blueprintCost.ToString(); //Update the hud with the new cost
            GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_blueprintStackCount.text = blueprintControlSystem.blueprintSlots[slotNumber].blueprintStackValue.ToString();
            GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].image_blueprintPicture.sprite = blueprintControlSystem.blueprintSlots[slotNumber].blueprintIcon; //Update the hud with the new icon of our blueprint

        }

        private void PickupDuplicateBlueprint(int slotNumber)
        {
            blueprintControlSystem.blueprintSlots[slotNumber].blueprint.DuplicatePickup(); //Tell the duplicate blueprint we've duplicated.
            blueprintControlSystem.blueprintSlots[slotNumber].blueprintStackValue++; //Increment our local knowledge of the blueprint's value by one.
            GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_blueprintStackCount.text = blueprintControlSystem.blueprintSlots[slotNumber].blueprintStackValue.ToString(); //Update our hud.
        }

        #endregion
        private void KeepCoreUpToDate()
        {
            entityCore.active_entityMaxVitality = base_maxVitality;
            entityCore.active_entityVitality = active_currentVitality;
        }


        internal void Die()
        {
            Debug.Log("Player has died!");
            isDead = true;
            //Death animation or some shit here. ? playerController.playerVisibleBody ?

            GameSystem_EntityManager.Instance.players.Remove(entityCore);
            GameSystem_EntityManager.Instance.deadPlayers += 1;
        }
    }
}