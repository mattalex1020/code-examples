using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using EasyButtons;
using LibrarySystem;
using EntitySystem;
using GameSystem;
using BlueprintSystem;
using MyBox;

namespace BlueprintSystem
{
    public class Blueprint : MonoBehaviour
    {
        [System.Serializable]
        public class Module
        {
            #region ACTION VARIABLES

            internal enum Action
            {
                Visual,
                Damaging,
                Healing,
                Buffing,
                Debuffing,
                Special
            }

            [Header("Action Variables")]

            [Tooltip("The action is the core fundamental of the module.")]
            [SerializeField] private Action action;

            //ACTION VARIABLES

            [ConditionalField(nameof(action), false, Action.Damaging)]
            [Tooltip("This is the damage ADDED to the damage value derived from the entityCore.")]
            [SerializeField] private float damageAmount;

            [ConditionalField(nameof(action), false, Action.Healing)]
            [Tooltip("This is the amount healed PER SECOND.")]
            [SerializeField] private float healingAmount;

            [ConditionalField(nameof(action), false, Action.Buffing)]
            [Tooltip("This is the amount buffed PER SECOND.")]
            [SerializeField] private float buffingAmount;

            [ConditionalField(nameof(action), false, Action.Debuffing)]
            [Tooltip("This is the amount debuffed PER SECOND.")]
            [SerializeField] private float debuffingAmount;

            [ConditionalField(nameof(action), false, Action.Buffing, Action.Debuffing)]
            [Tooltip("This is the stat that gets buffed or debuffed.")]
            [SerializeField] private EnumLibrary.EnhancerType targetStat;

            [ConditionalField(nameof(action), false, Action.Damaging, Action.Healing, Action.Buffing, Action.Debuffing)]
            [Tooltip("Enables or disables AOE mechanics.")]
            [SerializeField] private bool isUsingAreaOfEffect;

            //AOE VARIABLES

            [ConditionalField(nameof(isUsingAreaOfEffect))]
            [Tooltip("The spherical radius of the AOE.")]
            [SerializeField] private float areaOfEffectRadius;

            [ConditionalField(nameof(isUsingAreaOfEffect))]
            [Tooltip("Enables or disables impact falloff. Makes the damage/heal/buff/debuff decrease the farther an entity is from it's origin.")]
            [SerializeField] private bool distanceAffectsValue; //Enabling this makes the damage decrease the farther away from the AOE an entity is.

            [ConditionalField(nameof(isUsingAreaOfEffect))]
            [Tooltip("Modifies the impact value based on this percent. Useful for doing extra or reduced secondary damage/healing etc.")]
            [SerializeField] private float percentOfValueAsAreaOfEffect; //Perform a % of the damage (> or < 100%) within the AOE. 

            //SPECIAL VARIABLES

            private enum SpecialActionType
            {
                Teleportation,
                Invisibility
            }

            [ConditionalField(nameof(action), false, Action.Special)]
            [Tooltip("Specially coded actions can be designated with this.")]
            [SerializeField] private SpecialActionType specialType;

            [ConditionalField(nameof(specialType), false, SpecialActionType.Invisibility)]
            [Tooltip("How long should the inivisibility last.")]
            [SerializeField] private float invisbilityTime;

            #endregion

            #region SPAWN TYPE VARIABLES

            private enum SpawnType
            {
                Origin,
                Outpoint,
                Target
            }

            [Header("Spawn Variables")]

            [Tooltip("Origin makes the blueprint do it's thing at the entity using it. Outpoint uses the outpoint array. Target uses the target's position.")]
            [SerializeField] private SpawnType spawnType;

            [ConditionalField(nameof(spawnType), false, SpawnType.Origin, SpawnType.Target)]
            [Tooltip("Useful for creating projectiles and other effects near/above/far from the origin or target.")]
            [SerializeField] private Vector3 positionOffset;

            [ConditionalField(nameof(spawnType), false, SpawnType.Origin, SpawnType.Target)]
            [Tooltip("Really only used for pointing projectiles and other effects at something in particular.")]
            [SerializeField] private Vector3 rotationOffset; //Converts it to Quaternion using Euler.

            #endregion

            #region STYLE VARIABLES

            internal enum Style
            {
                Stationary,
                Flash,
                Pulse,
                Expand,
                Forward,
                Directional,
                Tracking,
                Random
            }

            [Header("Style Variables")]

            [Tooltip("Stationary: Created object doesn't move. Stays at origin. Flash: Makes a brief flash/momentary effect. Pulse: Makes a repeated flash with an interval. Expand: Continously expanding, useful for explosions etc. Forward: Used to make projectiles. Directional: Lasers, chain lightning, etc. Tracking: Same as forward but projectile/etc follows target. Random: Makes erratic movement for a projectile or otherwise. ")]
            [SerializeField] private Style style;

            [ConditionalField(nameof(style), false, Style.Stationary, Style.Flash, Style.Pulse, Style.Expand, Style.Forward, Style.Tracking, Style.Random)]
            [Tooltip("How long should the created object last? Directly impacts healing, buffs, debuffs, and so on. Projectile DECAY is specified here.")]
            [SerializeField] private float lifetime;

            [ConditionalField(nameof(style), false, Style.Pulse)]
            [Tooltip("How long in between pulses?")]
            [SerializeField] private float pulseInterval;

            [ConditionalField(nameof(style), false, Style.Expand)]
            [Tooltip("How fast does this effect expand?")]
            [SerializeField] private float expansionRate;

            [ConditionalField(nameof(style), false, Style.Forward, Style.Tracking, Style.Random)]
            [Tooltip("How fast a projectile moves.")]
            [SerializeField] private float projectileSpeed;

            [ConditionalField(nameof(style), false, Style.Tracking, Style.Random)]
            [Tooltip("How fast a projectile rotates; used for seeking targets and randomized directions.")]
            [SerializeField] private float rotationSpeed;

            //Directional Variables

            [ConditionalField(nameof(style), false, Style.Directional)]
            [Tooltip("Must be added to the blueprint as a separate object! MUST BE SERIALIZED HERE! Shit is complicated, yo.")]
            [SerializeField] private LineRenderer lineRenderer; //

            [ConditionalField(nameof(style), false, Style.Directional)]
            [Tooltip("How far does this laser/lightning/thing go?")]
            [SerializeField] private float lineDistance;

            [ConditionalField(nameof(style), false, Style.Directional)]
            [Tooltip("The diameter of a laser or 'spread width' of lightning, if that's a thing.")]
            [SerializeField] private float lineDiamter;

            [ConditionalField(nameof(style), false, Style.Directional)]
            [Tooltip("If true, the line/laser will track the target instead of just going forward * the distance.")]
            [SerializeField] private bool isSmart;

            #endregion

            #region OBJECT VARIABLES

            private enum ObjectType
            {
                None,
                Empty,
                Primitive,
                Custom
            }

            [Header("Object Variables")]

            [Tooltip("Somewhat unneeded, but cleans things up a bit.")]
            [SerializeField] private ObjectType objectType;

            private enum PrimitiveType
            {
                Sphere,
                Cube,
                Cone
            }

            [ConditionalField(nameof(objectType), false, ObjectType.Primitive)]
            [Tooltip("Uses a basic primitive chosen here.")]
            [SerializeField] private PrimitiveType primitiveType; //Will pull from the prefab library.

            [ConditionalField(nameof(objectType), false, ObjectType.Custom)]
            [Tooltip("Uses the prefab serialized in this variables.")]
            [SerializeField] private GameObject customObject;

            [ConditionalField(nameof(objectType), false, ObjectType.Empty, ObjectType.Primitive, ObjectType.Custom)]
            [Tooltip("This sets the scale of the object created. Used to enlarge or shrink primitives, prefabs, and even effects on emptys.")]
            [SerializeField] private Vector3 customScale;

            #endregion

            #region SHADER VARIABLES

            [Header("Shader Variables")]
            private string thisisherebecauseunityissilly;

            [ConditionalField(nameof(objectType), false, ObjectType.Primitive, ObjectType.Custom)]
            [Tooltip("Enabling this makes the module add a shader to the object chosen above.")]
            [SerializeField] private bool needsShader;

            [ConditionalField(nameof(needsShader))]
            [Tooltip("This is the shader used by the object chosen above.")]
            [SerializeField] private Shader objectShader;

            [ConditionalField(nameof(objectType), false, ObjectType.Primitive, ObjectType.Custom)]
            [Tooltip("Enabling this makes the module apply a material to the object chosen above. Note: Will replace ALL materials on the object.")]
            [SerializeField] private bool needsMaterialChanged;

            [ConditionalField(nameof(needsMaterialChanged))]
            [Tooltip("This is the material used by the object chosen above. It will replace all materials on the object with this one.")]
            [SerializeField] private Material objectMaterial;

            #endregion

            #region EFFECT VARIABLES

            [Header("Effect Variables")]

            [Tooltip("This/these effects are added to the object above.")]
            [SerializeField] private GameObject[] effectPrefabs;

            #endregion

            #region LIGHTING VARIABLES

            [Tooltip("This/these lights are added to the object above.")]
            [SerializeField] private GameObject[] lightPrefabs;


            #endregion

            // -----------

            #region INITIALIZATION CONFIGURATION

            [SerializeField]
            private bool usesPooling;

            [SerializeField]
            private int poolAmount;

            private Queue<GameObject> essencePool;

            [SerializeField]
            private GameObject completedEssence; //The compiled object ready for pooling/usage.


            internal EntitySystem_Core entityCore; //Set on initalization.

            #endregion






            #region ACTIVATION TEMPVARS

            private Transform origin; //Set on activation.
            private Transform[] outPoints; //Set on activation.
            private Transform target; //Set on activation.
            private GameObject activeEssenceObject; //Referenced with activation.
            private Essence activeEssence;

            #endregion

            #region INTIALIZATION TEMPVARS

            private Transform blueprintObject;
            private GameObject blankEssence; 
            private float optional_bossDamageBoost;

            #endregion

            internal void ActivateModule(Transform _origin, Transform[] _outPoints, Transform _target)
            {
                SetInitialTempVars(_origin, _outPoints, _target);
                UseEssence();
            }

            private void SetInitialTempVars(Transform _origin, Transform[] _outPoints, Transform _target)
            {
                origin = _origin;
                outPoints = _outPoints;
                target = _target;
            }


            #region INITIALIZATION SEGMENT

            internal void Initialization(Transform _blueprintObject) //Creates the physical aspect of the blueprint and pools it for activation. Is called when picked up.
            {
                //Module
                //entityCore = originCore; //Sets the module's entity core to the origin entity.
                blueprintObject = _blueprintObject;

                //Essence
                blankEssence = Instantiate(PrefabLibrary.instance.emptyEssence); //Blank essence contains an empty gameobject with an essence script.

                Essence newEssence = blankEssence.GetComponent<Essence>();

                BuildEssence(blankEssence); //Compile the physical side of our module.
            }

            private void BuildEssence(GameObject buildingEssence)
            {
                GameObject newObj = null;

                //Build physical object.

                if (objectType == ObjectType.Empty || objectType == ObjectType.None)
                {
                    //Next phase.
                }

                if (objectType == ObjectType.Primitive)
                {
                    if (primitiveType == PrimitiveType.Sphere)
                    {
                        newObj = Instantiate(PrefabLibrary.instance.primitive_sphere, buildingEssence.transform);
                    }

                    if (primitiveType == PrimitiveType.Cube)
                    {
                        newObj = Instantiate(PrefabLibrary.instance.primitive_cube, buildingEssence.transform);
                    }

                    if (primitiveType == PrimitiveType.Cone)
                    {
                        newObj = Instantiate(PrefabLibrary.instance.primitive_cone, buildingEssence.transform);
                    }
                }

                if (objectType == ObjectType.Custom)
                {
                    newObj = Instantiate(customObject, buildingEssence.transform);
                }

                newObj.transform.localPosition = Vector3.zero;
                newObj.SetActive(true);

                //Apply shader or material

                Renderer objectRenderer = newObj.GetComponent<Renderer>();

                if(objectRenderer == null)
                {
                    Debug.Log("Object renderer null.");
                    objectRenderer = newObj.GetComponent<MeshRenderer>();
                }

                if (objectRenderer == null)
                {
                    Debug.Log("Object renderer still null.");
                }

                if (needsShader == true)
                {
                    objectRenderer.material.shader = objectShader;
                }

                if(needsMaterialChanged == true)
                {
                    Material[] newMats = objectRenderer.materials;

                    for (int i = 0; i < newMats.Length; i++)
                    {
                        newMats[i] = objectMaterial;
                    }

                    objectRenderer.materials = newMats;
                }

                //Add all effects.

                if (effectPrefabs.Length > 0)
                {
                    for (int i = 0; i < effectPrefabs.Length; i++)
                    {
                        GameObject newEffect = Instantiate(effectPrefabs[i], buildingEssence.transform);
                        newEffect.transform.localPosition = Vector3.zero;
                        newEffect.SetActive(true);
                    }
                }

                //Add all lights

                if(lightPrefabs.Length > 0)
                {
                    for(int i = 0; i < lightPrefabs.Length; i++)
                    {
                        GameObject newLight = Instantiate(lightPrefabs[i], buildingEssence.transform);
                        newLight.transform.localPosition = Vector3.zero;
                        newLight.SetActive(true);
                    }
                }

                buildingEssence.transform.localScale = customScale;

                completedEssence = buildingEssence;

                Pool(); //Create a pool of our finished essence.
            }


            private void Pool()
            {
                essencePool = new Queue<GameObject>();

                for(int i = 0; i < poolAmount; i++)
                {
                    GameObject obj = Instantiate(completedEssence, blueprintObject);
                    obj.SetActive(false);
                    essencePool.Enqueue(obj);
                }
            }

            #endregion


            #region ACTIVATION SEGMENT

            private void UseEssence()
            {
                activeEssenceObject = essencePool.Dequeue();
                activeEssence = activeEssenceObject.GetComponent<Essence>();
                activeEssence.originCore = entityCore;
                activeEssenceObject.transform.SetParent(null);

                essencePool.Enqueue(activeEssenceObject);

                ActionPhase(); //Sets the stats of our essence.
            }

            private void ActionPhase()
            {
                activeEssence.essenceAction = action;

                if (action == Action.Visual)
                {
                    
                }
                
                if(action == Action.Damaging)
                {
                    //Damage is calculated on contact!
                }

                if(action == Action.Healing)
                {
                    activeEssence.valueOfAction = healingAmount;
                }

                if(action == Action.Buffing)
                {
                    activeEssence.valueOfAction = buffingAmount;
                    activeEssence.essenceTargetStat = targetStat;
                }

                if(action == Action.Debuffing)
                {
                    activeEssence.valueOfAction = debuffingAmount;
                    activeEssence.essenceTargetStat = targetStat;
                }

                if(action == Action.Special)
                {

                }

                SpawnPhase();
            }

            private void SpawnPhase() //Sets position of our pulled essence.
            {
                if(spawnType == SpawnType.Origin)
                {
                    activeEssence.transform.position = origin.position;
                }

                if(spawnType == SpawnType.Outpoint)
                {
                    int random = Random.Range(0, outPoints.Length - 1);
                    activeEssence.transform.localPosition = outPoints[random].position;
                    activeEssence.transform.localRotation = outPoints[random].rotation;
                }

                if(spawnType == SpawnType.Target)
                {
                    activeEssence.transform.position = target.position;
                }

                StylePhase();
            }

            private void StylePhase()
            {
                activeEssence.essenceStyle = style;

                activeEssence.lifetime = lifetime;

                /*
                if (style == Style.Stationary)
                {
                    
                }

                if(style == Style.Flash)
                {

                }

                if(style == Style.Pulse)
                {

                }

                if(style == Style.Expand)
                {

                }

                if(style == Style.Forward)
                {
                    
                }

                if(style == Style.Directional)
                {

                }

                if(style == Style.Tracking)
                {

                }

                if(style == Style.Random)
                {

                }*/

                AssignRemainingVariables();
            }

            private void AssignRemainingVariables()
            {
                activeEssence.movementSpeed = projectileSpeed;
                activeEssence.originCore = entityCore;
                activeEssence.target = entityCore.activeTarget;

                Finish();
            }

            private void Finish()
            {
                activeEssenceObject.SetActive(true);
            }

            #endregion








            internal void UpgradeBlueprintModule()
            {

            }

        }

        // END MODULE CLASS -----------------

        #region ENUMS
        public enum BlueprintType
        {
            Action,
            Sentry,
            Proxy
        }
        #endregion
        #region Configuration

        [Space(3)]
        [Header("Visual Configuration")]

        [Space]
        [Tooltip("This is the name of the Blueprint aka Spell that will appear on the HUD, etc...")]
        [SerializeField]
        internal string blueprintName;
        [Tooltip("This is the description of the Blueprint's abilities and usage.")]
        [SerializeField]
        internal string blueprintDescription;
        [Tooltip("This is the HOTBAR image of the Blueprint.")]
        [SerializeField]
        internal Sprite blueprintImage;

        [Space(3)]
        [Header("Fundamentals")]

        [Space]
        [Tooltip("Delay before executing the Blueprint modules.")]
        [SerializeField]
        private float castingDelay; //Time after activation that we wait to actually fire. NEED TO BE MADE PER BLUEPRINT.
        private bool isCasting;

        [Tooltip("This is the nanite cost of the Blueprint.")]
        [SerializeField]
        internal int naniteCost;

        [Tooltip("Time until the Blueprint can be used again.")]
        [SerializeField]
        internal float cooldown;//Time until we can use this again
        [ReadOnly] internal float cooldownRemaining;
        private bool canUse;

        [Space(3)]
        [Header("Usage and Stats")]

        [Space]

        [Tooltip("This sets our entities targeting tags when this is active!")]
        internal EnumLibrary.TargetingType targetingType;

        [Tooltip("To enable 'rapid fire' by holding the trigger, enable this for the blueprint.")]
        [SerializeField]
        private bool constantInput;
        
        [Tooltip("This is incremented up when the blueprint has a stack.")]
        [SerializeField]
        internal int stackValue;

        [Space(3)]
        [Header("Type Management")]
        
        [Space]
        [Tooltip("Designates the purpose of this blueprint.")]
        [SerializeField]
        private BlueprintType blueprintType;

        #endregion
        #region Type: Action

        [ConditionalField(nameof(blueprintType), false, BlueprintType.Action)]
        [Tooltip("Just a warning, since ConditionalFields currently do not support arrays.")]
        [SerializeField]
        [ReadOnly] public string actionNotice = "Note: This is required for ACTION blueprint types.";

        [Tooltip("Every module in this array will be activated if the blueprint type is 'Action'.")]
        [SerializeField]
        private Module[] blueprintModules;

        #endregion
        #region Type: Sentry

        [ConditionalField(nameof(blueprintType), false, BlueprintType.Sentry)]
        [Tooltip("Place the sentry prefab here.")]
        [SerializeField] private GameObject sentryPrefab;

        private bool canPlaceSentry;

        #endregion
        #region Type: Proxy

        [ConditionalField(nameof(blueprintType), false, BlueprintType.Proxy)]
        [Tooltip("Place the proxy prefab here.")]
        [SerializeField] private GameObject proxyPrefab;

        private bool canPlaceProxy;

        #endregion








        [Space(3)]
        [Header("Read Only Variables")]

        [Space]
        [Tooltip("This boolean determines if the Blueprint is active or not.")]
        [SerializeField]
        [ReadOnly] internal bool isActive;

        #region EXTERNALLY ACCESSED VARIABLES
        internal int slotNumber;
        internal EntitySystem_Core entityCore;

        #endregion

        void Start()
        {
            SetupModules();
        }

        private void SetupModules()
        {
            for(int i = 0; i < blueprintModules.Length; i++)
            {
                blueprintModules[i].Initialization(this.transform);
            }
        }

        void Update()
        {
            Cooldown();
            InputChecking();
        }
        

        private void InputChecking()
        {
            if (isActive == false)
            {
                return;
            }
            //Right click to cast.
            if (constantInput == true)
            {
                if (Input.GetMouseButton(1) && entityCore.player.active_currentNanites >= naniteCost && isCasting == false && canUse == true) //If we click the mouse and can afford the cost, we do the thing.
                {
                    canUse = false;
                    cooldownRemaining = cooldown;
                    GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].image_blueprintPicture.color = GameSystem_UIManager.Instance.color_cooldown;
                    GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_cooldownCounter.text = cooldown.ToString();
                    
                    StartCoroutine(CastingDelay(entityCore.blueprintControlSystem));
                    isCasting = true;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(1) && entityCore.player.active_currentNanites >= naniteCost && isCasting == false && canUse == true) //If we click the mouse and can afford the cost, we do the thing.
                {
                    canUse = false;
                    cooldownRemaining = cooldown;
                    GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].image_blueprintPicture.color = GameSystem_UIManager.Instance.color_cooldown;
                    GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_cooldownCounter.text = cooldown.ToString();

                    
                    isCasting = true;
                    ActivateBlueprint(entityCore.blueprintControlSystem);
                }
            }
        }

        #region ACTIVATION AND USE
        internal void ActivateBlueprint(BlueprintControlSystem blueprintControlSystem)
        {
            if(blueprintModules[0].entityCore == null)
            {
                foreach(Module m in blueprintModules)
                {
                    m.entityCore = entityCore;
                }
            }

            entityCore.player.active_currentNanites -= naniteCost;

            if (blueprintType == BlueprintType.Action)
            {
                StartCoroutine(CastingDelay(blueprintControlSystem));
            }
            else if(blueprintType == BlueprintType.Sentry)
            {
                PlaceSentry();
            }
            else if(blueprintType == BlueprintType.Proxy)
            {
                PlaceProxy();
            }
        }

        private void ActivateModules(Transform origin, Transform[] outPoints, Transform target)
        {
            for(int i = 0; i < blueprintModules.Length; i++)
            {
                blueprintModules[i].ActivateModule(origin, outPoints, target);
            }
        }

        private void PlaceSentry()
        {

        }

        private void PlaceProxy()
        {

        }

        #endregion

        #region FUNCTIONALITY
        internal void DuplicatePickup()
        {
            stackValue++;
            for (int i = 0; i < blueprintModules.Length; i++)
            {
                blueprintModules[i].UpgradeBlueprintModule();
            }
        }

        private IEnumerator CastingDelay(BlueprintControlSystem blueprintControlSystem)
        {
            //entityCore.thisAnimator.SetTrigger("cast");
            yield return new WaitForSeconds(castingDelay);
            ActivateModules(entityCore.transform, blueprintControlSystem.outPoints, entityCore.activeTarget);
            isCasting = false;
            yield break;
        }

        private void Cooldown()
        {
            if (canUse == true)
            {
                return;
            }
            if (cooldownRemaining > 0)
            {
                cooldownRemaining -= 1 * Time.deltaTime;
                GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_cooldownCounter.text = Mathf.RoundToInt(cooldownRemaining).ToString();

            }
            else
            {
                canUse = true;
                GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].image_blueprintPicture.color = Color.white;
                GameSystem_UIManager.Instance.list_hotbarSlots[slotNumber].text_cooldownCounter.text = "";
            }
        }


        #endregion

    }
}
