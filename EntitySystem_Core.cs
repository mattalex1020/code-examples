using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;
using LibrarySystem;
using AKUtilities;
using BlueprintSystem;
using MyBox;

namespace EntitySystem
{
    /// <summary>
    /// This is a universal access script for everything needing to deal with entities. Damage? This. Stats? This. It checks and filters all interactions and goes on every entity; players, sentries, machines, even tiles.
    /// </summary>
    public class EntitySystem_Core : MonoBehaviour
    {
        #region ENUMS
        /// <summary>
        /// This is the type of entity and is used to determine what the core should do and how it's interacted with.
        /// </summary>
        internal enum EntityType
        {
            Player,
            Sentry,
            Proxy,
            Machine,
            MachineSentry,
            Tile,
            Pawn
        }
        /// <summary>
        /// This is the type of interaction being done. It allows us to route to the appropriate scripts.
        /// </summary>
        internal enum InteractionType
        {
            Damaging,
            Healing,
            Stunning
        }

        internal enum StatCalcType
        {
            TypeOne,
            TypeTwo,
            TypeThree
        }

        #endregion
        #region CONFIGURATION
        [Space(3)]
        [Header("Core Configuration")]

        [Space]
        [SerializeField]
        internal EntityType thisType;
        
        [SerializeField]
        internal bool hasRigidbody { get; } //Unsure if the "get;" is needed here.
        [SerializeField]
        internal GameObject visibleBody;

        [SerializeField]
        internal EnumLibrary.Aspects entityAspect; //NOT IMPLEMENTED YET




        #endregion
        #region SIGHT MANAGEMENT
        [Space(3)]
        [Header("Special Configuration")]

        [Tooltip("If true, the entity will need to have line of sight of it's target or a unique behavior will be run.")]
        [SerializeField]
        private bool needsToCheckForSightOfTarget;

        [ConditionalField(nameof(needsToCheckForSightOfTarget))]
        [Tooltip("This is the layermask used by the sight raycast to determine obstruction.")]
        [SerializeField]
        private LayerMask sightCheckingMask;

        #endregion
        #region STATS
        

        #endregion
        #region COMBAT

        [Space(3)]
        [Header("Combat Configuration")]

        [Space]

        [SerializeField]
        private float base_baseDamage; //The starting damage done by the character before being processed.
        internal float active_baseDamage; //The current damage done by the character before being processed.
        [SerializeField]
        private float base_aggressionRating; //The normal aggro level for this entity.
        internal float active_aggressionRating; //The active/modified aggro level for this entity.

        [Space]
        [SerializeField]
        internal string hitTextIdentity; //The id for the hitText pool.
        internal bool canBeTargeted = true;
        internal bool isStunned;

        #endregion

        #region TARGETING CONFIGURATION

        [Space(3)]
        [Header("Targeting Configuration")]

        

        [SerializeField]
        private EnumLibrary.TargetAcquisitionMethod targetingMethod; //How will we get our targets?

        [ConditionalField(nameof(targetingMethod), false, EnumLibrary.TargetAcquisitionMethod.ForwardFromPoint)]
        [Tooltip("This is used to determine where the entity should 'fire' the spherecast check for a forward target.")]
        [SerializeField]
        private Transform targetingOutpoint; //The starting point for raycast based target checking.

        [ConditionalField(nameof(targetingMethod), false, EnumLibrary.TargetAcquisitionMethod.Closest, EnumLibrary.TargetAcquisitionMethod.FromPlayerList)]
        [Tooltip("This is used for multiple targeting methods. The special case is when pulling directly from a list, which will do a proximity check if the list is empty.")]
        [SerializeField]
        private float sphericalCheckRadius;

        [ConditionalField(nameof(targetingMethod), false, EnumLibrary.TargetAcquisitionMethod.Closest, EnumLibrary.TargetAcquisitionMethod.FromPlayerList)]
        [Tooltip("This is used for multiple targeting methods. The special case is when pulling directly from a list, which will do a proximity check if the list is empty.")]
        [SerializeField]
        private LayerMask targetableLayers; //Only check for tags on these layers.

        [ConditionalField(nameof(targetingMethod), false, EnumLibrary.TargetAcquisitionMethod.ForwardFromPoint)]
        [Tooltip("This is used for the sphere cast.")]
        [SerializeField]
        private float targetingSpherecastRadius; //10 for players

        [ConditionalField(nameof(targetingMethod), false, EnumLibrary.TargetAcquisitionMethod.ForwardFromPoint)]
        [Tooltip("This is used for the sphere cast.")]
        [SerializeField]
        private float targetingRange; //1000 for players

        [Space]

        [SerializeField]
        internal EnumLibrary.TargetingType targetingType; //THIS IS CHANGED BY THE ACTIVE BLUEPRINT OR AI STATE!

        [SerializeField]
        internal string[] friendlyTags; //What tags are considered friendly?

        [SerializeField]
        internal string[] hostileTags; //What tags are considered hostile?

        [Space]
        
        [SerializeField]
        internal Transform activeTarget;

        private string[] currentTags; //Uses friendly or hostile depending on the target type.

        #endregion


        #region REWARDS

        [Space(3)]
        [Header("DEATH CONFIGURATION")]

        [Space]

        [SerializeField]
        private float dropChance;

        [Space]
        [SerializeField]
        private float base_killExp; //Exp given on kill
        private float active_killExp;
        [SerializeField]
        private float base_killFlux; //Flux given on kill
        private float active_killFlux;

        private bool isDead;

        [SerializeField]
        private float base_threatOnKill;
        private float active_threatOnKill;

        #endregion
        #region READ ONLY SCANS
        internal float active_entityMaxVitality;
        internal float active_entityVitality;
        internal float entityLevel;


        #endregion
        #region CONDITIONALLY SET
        internal Transform thisEntity;
        internal Rigidbody thisRigidbody;
        internal GameSystem_EntityManager.Spawner spawner;
        [SerializeField]
        internal Animator thisAnimator;
        [SerializeField]
        internal SkinnedMeshRenderer[] entityRenderers;
        #endregion
        #region UNIVERSAL SCRIPTS
        internal EntitySystem_Info entityInfo;
        internal BlueprintControlSystem blueprintControlSystem;
        #endregion
        #region CONDITIONAL SCRIPTS
        internal Player player;
        private Sentry sentry;
        private Machine.Proxy proxy;
        private Machine machine;
        private Sentry.MachineSentry machineSentry;
        private Tile tile;
        private Pawn pawn;
        #endregion

        void OnEnable()
        {
            isDead = false;
            canBeTargeted = true;
            AddToRelativeEntityList();

            if (entityInfo == null)
            {
                return;
            }

            if(thisType == EntityType.Player)
            {
                
            }
           
            SetLevel();

            if (thisType == EntityType.Machine)
            {
                UpdateMachineStats();
            }
        }

        void Start()
        {
            #region UNIVERSAL START CALLS
            thisEntity = this.transform; //Set our entity here.

            if (hasRigidbody == true)
            {
                thisRigidbody = thisEntity.GetComponent<Rigidbody>();
            }

            entityInfo = this.gameObject.GetComponent<EntitySystem_Info>();
            blueprintControlSystem = this.gameObject.GetComponent<BlueprintControlSystem>();
            SetupStats();
            entityRenderers = visibleBody.GetComponentsInChildren<SkinnedMeshRenderer>();
            #endregion

            #region CONDITIONAL START CALLS
            LinkToEntityCore();
            #endregion


        }

        void Update()
        {
            CheckForDeath();
        }



        #region INTERACTION FUNCTIONS
        internal void Contact(EntitySystem_Core origin, InteractionType iType, float applicableValue)
        {
            if (CheckOwnership(origin) == true) //ONLY POSITIVE SELF INFLICTIONS ARE ALLOWED.
            {
                if (CanSelfInflict(iType) == true) //ONLY HEALING SO FAR. BUFFS ARE NEXT
                {
                    DoAction(origin, iType, applicableValue); //DO THE ACTION TO OURSELVES OR GET IT FROM OUR PROXY OR SENTRY
                }
                else
                {
                    return;
                }
            }
            else //We don't own this, so we check if we're in PvP
            {
               // if (AK_GameSystems.GameSystem_Core.Instance.pvpEnabled == true) //If so, we don't run any blocks. What's acting on us isn't ours and anything can act on us.
               // {
               //     DoAction(origin, iType, applicableValue); //Do the action as intended.
               // }
               // else
               // {
                    DoAction(origin, iType, applicableValue); //Check for PvP conflicts before doing the action.
               // }
            }


        }

        private void DoAction(EntitySystem_Core origin, InteractionType iType, float applicableValue)
        {
            if (thisType == EntityType.Player) //Run player-only actions
            {
                if (iType == InteractionType.Damaging)
                {
                    DamageAction(player, applicableValue);
                }

                if(iType == InteractionType.Healing)
                {
                    HealingAction();
                }
            }
            else if (thisType == EntityType.Sentry) //Run sentry-only actions
            {
                if (iType == InteractionType.Damaging)
                {
                    DamageAction(sentry, applicableValue);
                }

                if (iType == InteractionType.Healing)
                {

                }
            }
            else if (thisType == EntityType.Proxy) //Run proxy-only actions
            {
                if (iType == InteractionType.Damaging)
                {
                    DamageAction(proxy, applicableValue);
                }

                if (iType == InteractionType.Healing)
                {

                }
            }
            else if (thisType == EntityType.Machine) //Run machine-only actions
            {
                if (iType == InteractionType.Damaging)
                {
                    DamageAction(machine, applicableValue);
                }

                if (iType == InteractionType.Healing)
                {

                }

                if(iType == InteractionType.Stunning)
                {
                    StunningAction(machine, applicableValue);
                }
            }
            else if (thisType == EntityType.MachineSentry) //Run machine sentry-only actions
            {
                if (iType == InteractionType.Damaging)
                {
                    DamageAction(machineSentry, applicableValue);
                }

                if (iType == InteractionType.Healing)
                {

                }
            }
            else if (thisType == EntityType.Tile) //Run tile-only actions
            {
                if (iType == InteractionType.Damaging)
                {
                    DamageAction(tile, applicableValue);
                }

                if (iType == InteractionType.Healing)
                {

                }
            }
            else if(thisType == EntityType.Pawn)
            {
                if(iType == InteractionType.Damaging)
                {
                    DamageAction(pawn, applicableValue);
                }

                if (iType == InteractionType.Healing)
                {

                }
            }
        }
#endregion




#region START CALLS
        /// <summary>
        /// Setup all the references the core is going to need to do it's job.
        /// </summary>
        private void LinkToEntityCore()
        {
            if (thisType == EntityType.Player)
            {
                player = this.gameObject.GetComponent<Player>();
            }
            else if (thisType == EntityType.Sentry)
            {
                sentry = this.gameObject.GetComponent<Sentry>();
            }
            else if (thisType == EntityType.Proxy)
            {
                proxy = this.gameObject.GetComponent<Machine>().machineAsProxy;
            }
            else if (thisType == EntityType.Machine)
            {
                machine = this.gameObject.GetComponent<Machine>();
            }
            else if (thisType == EntityType.MachineSentry)
            {
                machineSentry = this.gameObject.GetComponent<Sentry>().machineSentry;
            }
            else if (thisType == EntityType.Tile)
            {
                tile = this.gameObject.GetComponent<Tile>();
            }
            else if(thisType == EntityType.Pawn)
            {
                pawn = this.gameObject.GetComponent<Pawn>();
            }
        }
        /// <summary>
        /// Set our active variables to equal the base variables.
        /// </summary>
        private void SetupStats()
        {
            active_aggressionRating = base_aggressionRating;
            active_baseDamage = base_baseDamage;
            active_killExp = base_killExp;
            active_killFlux = base_killFlux;
        }
        #endregion
        #region ENABLED CALLS
        private void SetLevel()
        {
            if (thisType == EntityType.Player)
            {
                //Player levels are done using a collective calculation.
            }
            else if (thisType == EntityType.Sentry)
            {
                //Sentries derive their power not from level, but from the player that places them and the blueprint they're placed with.
            }
            else if (thisType == EntityType.Proxy)
            {
                //Proxies use the same method as the sentries.
            }
            else if (thisType == EntityType.Machine)
            {
                entityInfo.CalculateNewLevel();
            }
            else if (thisType == EntityType.MachineSentry)
            {

            }
            else if (thisType == EntityType.Tile)
            {

            }
        }
        #endregion
        
        #region ACTIONS
        #region DAMAGE

        private static void DamageAction(Player playerScript, float rawDamage)
        {
            playerScript.Damaged(rawDamage);
        }
        private static void DamageAction(Sentry sentryScript, float rawDamage)
        {

        }
        private static void DamageAction(Machine.Proxy proxyScript, float rawDamage)
        {

        }
        private static void DamageAction(Machine machineScript, float rawDamage)
        {
            machineScript.Damaged(rawDamage);
        }
        private static void DamageAction(Sentry.MachineSentry machineSentryScript, float rawDamage)
        {

        }
        private static void DamageAction(Tile tileScript, float rawDamage)
        {

        }
        private static void DamageAction(Pawn pawnScript, float rawDamage)
        {
            pawnScript.TakeDamage(rawDamage);
        }
        
        #endregion


        private void HealingAction()
        {

        }

        #region STUNNING
        private static void StunningAction(Machine machineScript, float duration)
        {
            machineScript.Stunned(duration);
        }


        
        #endregion
        #endregion

        #region CHECKING
        /// <summary>
        /// Filter the action taking place based on the type of action. Do one additional filter if PVP mode is disabled.
        /// </summary>
        /// <param name="origin">Entity doing the action.</param>
        /// <param name="iType">Type of action.</param>
        /// <param name="applicableValue">The value used in the action, if needed.</param>
        private void ActionChecking(EntitySystem_Core origin, InteractionType iType, float applicableValue)
        {
            if (iType == InteractionType.Damaging)
            {
                if (BlockPVPAction(origin) == false) //Prevent PVP type damage if the origin belongs to an unowned player/sentry/proxy.
                {
                    return;
                }
                else
                {
                    DoAction(origin, iType, applicableValue); //If damage is from machine or machine sentry, we can do what it wanted to do.
                }
            }
            else if (iType == InteractionType.Healing)
            {
                DoAction(origin, iType, applicableValue);
            }


        }
        /// <summary>
        /// This script checks to see if certain entities are trying to negatively impact eachother. When PvP is disabled, Players cannot hurt other players, sentries, or proxies. And Vice versa.
        /// Note: Tiles can be damaged by almost every entity.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        internal bool BlockPVPAction(EntitySystem_Core origin)
        {
            if (thisType == EntityType.Player)
            {
                if (origin.thisType == EntityType.Player)
                {
                    return false; //Another player is attempting to do a negative thing to this player.
                }
                else if (origin.thisType == EntityType.Sentry)
                {
                    return false; //Another player's sentry is attempting to do a negative thing to this player.
                }
                else if (origin.thisType == EntityType.Proxy)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Machine)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.MachineSentry)
                {
                    return true;
                }
                else if(origin.thisType == EntityType.Pawn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (thisType == EntityType.Sentry)
            {
                if (origin.thisType == EntityType.Player)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Sentry)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Proxy)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Machine)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.MachineSentry)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Pawn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (thisType == EntityType.Proxy)
            {
                if (origin.thisType == EntityType.Player)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Sentry)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Proxy)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Machine)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.MachineSentry)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Pawn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (thisType == EntityType.Machine)
            {
                if (origin.thisType == EntityType.Player)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Sentry)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Proxy)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Machine)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.MachineSentry)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Pawn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (thisType == EntityType.MachineSentry)
            {
                if (origin.thisType == EntityType.Player)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Sentry)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Proxy)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Machine)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.MachineSentry)
                {
                    return false;
                }
                else if (origin.thisType == EntityType.Pawn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (thisType == EntityType.Tile) //Everything can damage tiles.
            {
                if (origin.thisType == EntityType.Player)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Sentry)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Proxy)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Machine)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.MachineSentry)
                {
                    return true;
                }
                else if (origin.thisType == EntityType.Pawn)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Self inflicted negative actions are prohibited here, by checking the action and blocking it if it would do something harmful.
        /// </summary>
        /// <param name="iType">Type of action.</param>
        /// <returns></returns>
        private bool CanSelfInflict(InteractionType iType)
        {
            if (iType == InteractionType.Damaging)
            {
                return false;
            }
            else if (iType == InteractionType.Healing)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
#endregion

#region GENERAL FUNCTIONS
        /// <summary>
        /// Compare the IDs between two entities to see if they're the same. Returns TRUE if SAME, FALSE if NOT SAME.
        /// </summary>
        /// <param name="origin">The entity initiating the action.</param>
        /// <returns></returns>
        private bool CompareIDs(EntitySystem_Core origin)
        {
            if (origin.entityInfo.entityId == entityInfo.entityId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Compare the Ownership ID between two entities to see if the entity being acted upon is owned by the origin entity. Returns TRUE if SAME, FALSE if NOT SAME.
        /// </summary>
        /// <param name="origin">The entity initiating the action.</param>
        /// <returns></returns>
        internal bool CheckOwnership(EntitySystem_Core origin)
        {
            if(origin.entityInfo == null)
            {
                return false;
            }

            if (this == origin || origin.entityInfo.entityOwner == entityInfo.entityId || entityInfo.entityOwner == origin.entityInfo.entityId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// Checks leading up to this have taken place, and the entity is to drop a blueprint on death.
        /// </summary>
        /// <param name="blueprintType">The type of blueprint that gets dropped. 0: Common, 1: Uncommon, 2: Rare, 3: Epic, 4: Legendary</param>
        private void DropBlueprint(int blueprintType)
        {
            if (blueprintType == 0) //Drop common BP
            {
                int _selection = Random.Range(0, GameSystem_EntityManager.Instance.blueprintSpawnables_common.Length);
                GameObject blueprint = Instantiate(GameSystem_EntityManager.Instance.blueprintSpawnables_common[_selection], this.gameObject.transform.position, Quaternion.identity);
            }
            else if (blueprintType == 1) //Drop uncommon BP, etc
            {
                int _selection = Random.Range(0, GameSystem_EntityManager.Instance.blueprintSpawnables_uncommon.Length);
                GameObject blueprint = Instantiate(GameSystem_EntityManager.Instance.blueprintSpawnables_uncommon[_selection], this.gameObject.transform.position, Quaternion.identity);
            }
            else if (blueprintType == 2)
            {
                int _selection = Random.Range(0, GameSystem_EntityManager.Instance.blueprintSpawnables_rare.Length);
                GameObject blueprint = Instantiate(GameSystem_EntityManager.Instance.blueprintSpawnables_rare[_selection], this.gameObject.transform.position, Quaternion.identity);
            }
            else if (blueprintType == 3)
            {
                int _selection = Random.Range(0, GameSystem_EntityManager.Instance.blueprintSpawnables_epic.Length);
                GameObject blueprint = Instantiate(GameSystem_EntityManager.Instance.blueprintSpawnables_epic[_selection], this.gameObject.transform.position, Quaternion.identity);
            }
            else if (blueprintType == 4)
            {
                int _selection = Random.Range(0, GameSystem_EntityManager.Instance.blueprintSpawnables_legendary.Length);
                GameObject blueprint = Instantiate(GameSystem_EntityManager.Instance.blueprintSpawnables_legendary[_selection], this.gameObject.transform.position, Quaternion.identity);
            }
        }

        #endregion

        #region DEATH FUNCTIONS

        private void CheckForDeath()
        {
            if(isDead == true)
            {
                return;
            }

            if (thisType == EntityType.Player)
            {
                if(player.active_currentVitality <= 0)
                {
                    Death();
                }
            }
            else if (thisType == EntityType.Sentry)
            {

            }
            else if (thisType == EntityType.Proxy)
            {

            }
            else if (thisType == EntityType.Machine)
            {
                if(machine.active_currentVitality <= 0)
                {
                    Death();
                }
            }
            else if (thisType == EntityType.MachineSentry)
            {

            }
            else
            {

            }
        }
        private void Death()
        {
            RemoveFromRelativeList();

            if (thisType == EntityType.Player)
            {
                player.Die();
            }
            else if (thisType == EntityType.Sentry)
            {

            }
            else if (thisType == EntityType.Proxy)
            {

            }
            else if (thisType == EntityType.Machine)
            {

                CheckForDrop();
                this.gameObject.SetActive(false);
                machine.active_currentVitality = 1;
                GameSystem_EntityManager.Instance.currentTotalEntities -= 1;
                GameSystem_EntityManager.Instance.collectivePlayerExperience += active_killExp;
                GameSystem_EntityManager.Instance.AddThreat(active_threatOnKill);
                GameSystem_EntityManager.Instance.AddFlux(active_killFlux);
                spawner.currentSpawned -= 1;

            }
            else if (thisType == EntityType.MachineSentry)
            {

                CheckForDrop();
            }
            else
            {

            }
        }
        /// <summary>
        /// Role the dice to see if this entity drops a blueprint.
        /// </summary>
        private void CheckForDrop()
        {
            if (AKUtilities.Utilities.DiceRoll(dropChance) == true)
            {
                DropBlueprint(AKUtilities.Utilities.RollForBlueprintDrop());
            }
            else
            {
                return;
            }
        }


        #endregion

        #region STAT CHANGES

        internal void UpdatePlayerStats()
        {
            active_baseDamage = (GameSystem_EntityManager.Instance.collectivePlayerLevel + GameSystem_EntityManager.Instance.collectivePlayerLevel * base_baseDamage) - Mathf.Sqrt(active_baseDamage);
            player.active_meleeDamage = 1.5f * (GameSystem_EntityManager.Instance.collectivePlayerLevel * GameSystem_EntityManager.Instance.collectivePlayerLevel * base_baseDamage) - Mathf.Sqrt(player.active_meleeDamage);
        }
        private void UpdateMachineStats()
        {
            machine.UpdateStats();
            active_killExp = StatCalc(this, base_killExp, active_killExp, StatCalcType.TypeTwo);
            active_killFlux = StatCalc(this, base_killFlux, active_killFlux, StatCalcType.TypeTwo);
            active_baseDamage = StatCalc(this, base_baseDamage, active_baseDamage, StatCalcType.TypeThree);
            active_aggressionRating = Mathf.RoundToInt(base_aggressionRating * entityInfo.active_entityLevel);
            active_threatOnKill = StatCalc(this, base_threatOnKill, active_threatOnKill, StatCalcType.TypeOne);
        }


        #region CALCULATIONS

        internal static float StatCalc(EntitySystem_Core core, float baseFactor, float activeFactor, StatCalcType type)
        {
            float newValue = 0;

            if (type == StatCalcType.TypeOne)
            {
                newValue = core.entityInfo.active_entityLevel * (baseFactor + Mathf.Sqrt(activeFactor));
            }
            else if(type == StatCalcType.TypeTwo)
            {
                newValue = baseFactor + (core.entityInfo.active_entityLevel * Mathf.Sqrt(activeFactor));
            }
            else if(type == StatCalcType.TypeThree)
            {
                newValue = baseFactor * core.entityInfo.active_entityLevel / 1 + Mathf.RoundToInt(Mathf.Sqrt(activeFactor));
            }

            newValue = Mathf.RoundToInt(newValue);
            return newValue;
        }


        #endregion
        #endregion

        private void AddToRelativeEntityList()
        {
            if (thisType == EntityType.Player)
            {
                Debug.Log("Add player to player list");
                //Happens based on progression and player status.
                GameSystem_EntityManager.Instance.players.Add(this);
            }
            else if (thisType == EntityType.Sentry)
            {
                GameSystem_EntityManager.Instance.playerEntities.Add(this);
            }
            else if (thisType == EntityType.Proxy)
            {
                GameSystem_EntityManager.Instance.playerEntities.Add(this);
            }
            else if (thisType == EntityType.Machine)
            {
                GameSystem_EntityManager.Instance.machineEntities.Add(this);
            }
            else if (thisType == EntityType.MachineSentry)
            {
                GameSystem_EntityManager.Instance.machineEntities.Add(this);
            }




        }

        private void RemoveFromRelativeList()
        {
            if (thisType == EntityType.Player)
            {
                //Happens based on progression and player status.
            }
            else if (thisType == EntityType.Sentry)
            {
                GameSystem_EntityManager.Instance.playerEntities.Remove(this);
            }
            else if (thisType == EntityType.Proxy)
            {
                GameSystem_EntityManager.Instance.playerEntities.Remove(this);
            }
            else if (thisType == EntityType.Machine)
            {
                GameSystem_EntityManager.Instance.machineEntities.Remove(this);
            }
            else if (thisType == EntityType.MachineSentry)
            {
                GameSystem_EntityManager.Instance.machineEntities.Remove(this);
            }
        }




        #region TARGETING
        internal void GetTarget() //Call whenever a target is needed! NOT CONSTANTLY!
        {
            AssignTags(); //Makes sure we use the right tag set.

            if (targetingMethod == EnumLibrary.TargetAcquisitionMethod.ForwardFromPoint)
            {
                GetTarget_FromPoint();
            }
            //
            if(targetingMethod == EnumLibrary.TargetAcquisitionMethod.Closest)
            {
                GetTarget_Closest();
            }
            //
            if(targetingMethod == EnumLibrary.TargetAcquisitionMethod.FromPlayerList)
            {
                GetTarget_FromPlayerList();

            }

        }





        #endregion 








        internal int GetCurrentDamage(bool addBossDamage, bool applyCriticalMultiplier, float criticalMultiplier)
        {
            float finalDamage = 0;

            finalDamage = active_baseDamage;

            if(addBossDamage == true)
            {
                finalDamage += player.enhanced_bossDamageBoost;
            }

            if(applyCriticalMultiplier == true)
            {
                finalDamage = finalDamage * criticalMultiplier;
            }

            return Mathf.RoundToInt(finalDamage);
        }



        #region TARGETING METHODS

        private void GetTarget_FromPoint()
        {
            Transform newTarget = null;

            if (thisType == EntityType.Player)
            {
                Ray targetRay = player.playerController.playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));


                newTarget = Utilities.GetTransformFromViewport(targetRay, targetingSpherecastRadius, targetingRange, targetableLayers, currentTags);


            }
            else
            {
                Ray targetRay = new Ray(targetingOutpoint.position, targetingOutpoint.transform.forward);

                newTarget = Utilities.GetTransformFromViewport(targetRay, targetingSpherecastRadius, targetingRange, targetableLayers, currentTags);

            }

            activeTarget = newTarget;
        }
        private void GetTarget_Closest()
        {
            Transform newTarget = null;

            if (thisType == EntityType.Player)
            {

            }
            else
            {
                Collider[] nearbyEntities = Physics.OverlapSphere(transform.position, 500, targetableLayers);
                List<EntitySystem_Core> entityCores = new List<EntitySystem_Core>();

                for (int i = 0; i < nearbyEntities.Length; i++)
                {

                    foreach (string tag in currentTags)
                    {
                        if (nearbyEntities[i].CompareTag(tag))
                        {
                            entityCores.Add(nearbyEntities[i].GetComponent<EntitySystem_Core>());
                        }

                    }
                }

                EntitySystem_Core[] entityArray = entityCores.ToArray();

                if (entityArray.Length == 0)
                {
                    Debug.Log("Target Nearby Overlap Check: First Pass: No entities in array.");
                    newTarget = null; //Not an error. If no players, we have no target.
                }
                else
                {
                    float highestAggro = 0;

                    for (int i = 0; i < entityArray.Length; i++)
                    {
                        if (entityArray[i].gameObject.activeInHierarchy == false || entityArray[i] == null || entityArray[i].canBeTargeted == false) //If our detect collider is inactive or null, skip.
                        {
                            Debug.Log("Target Nearby Overlap Check: Second Pass - Results: " + entityArray[i].gameObject.activeInHierarchy + " / " + entityArray[i] + " / " + entityArray[i].canBeTargeted);
                            newTarget = null;
                        }
                        else
                        {
                            float thisDistance = Vector3.Distance(entityArray[i].transform.position, transform.position);

                            float thisAggro = (entityArray[i].active_aggressionRating) - Mathf.Sqrt(thisDistance);

                            if (thisAggro > highestAggro)
                            {
                                highestAggro = thisAggro;
                                newTarget = entityArray[i].transform;
                            }
                            Debug.Log("Highest Aggro: " + highestAggro);
                        }
                    }
                }
            }

            activeTarget = newTarget;
        }
        private void GetTarget_FromPlayerList()
        {
            Transform newTarget = null;

            EntitySystem_Core[] playerArray = GameSystem_EntityManager.Instance.players.ToArray();

            if (playerArray.Length == 0)
            {
                Debug.Log("Target From Player List: First Pass: No players in array.");
                newTarget = null; //Not an error. If no players, we have no target.
            }
            else
            {
                float highestAggro = 0;

                for (int i = 0; i < playerArray.Length; i++)
                {
                    if (playerArray[i].gameObject.activeInHierarchy == false || playerArray[i] == null || playerArray[i].canBeTargeted == false) //If our detect collider is inactive or null, skip.
                    {
                        Debug.Log("Target From Player List: Second Pass - Results: " + playerArray[i].gameObject.activeInHierarchy + " / " + playerArray[i] + " / " + playerArray[i].canBeTargeted);
                        newTarget = null;
                    }
                    else
                    {
                        float thisDistance = Vector3.Distance(playerArray[i].transform.position, transform.position);

                        float thisAggro = (playerArray[i].active_aggressionRating) - Mathf.Sqrt(thisDistance);

                        if (thisAggro > highestAggro)
                        {
                            highestAggro = thisAggro;
                            newTarget = playerArray[i].transform;
                        }
                    }
                }
            }
            
            activeTarget = newTarget;
        }



        private void AssignTags()
        {
            if(targetingType == EnumLibrary.TargetingType.FriendlyEntities)
            {
                currentTags = friendlyTags;
            }

            if(targetingType == EnumLibrary.TargetingType.HostileEntities)
            {
                currentTags = hostileTags;
            }
        }

        #endregion

    }
}