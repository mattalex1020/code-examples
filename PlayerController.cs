using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntitySystem;
using GameSystem;
using AKUtilities;

namespace PlayerSystem
{
    public class PlayerController : MonoBehaviour
    {
        #region CAMERA
        [SerializeField]
        private float verticalSensitivity;
        [SerializeField]
        private float horizontalSensitivity;
        [SerializeField]
        private Transform cameraPivotPoint; //Centered on player.
        [SerializeField]
        internal Camera playerCamera;
        float rotationY;
        float rotationX;
        private float cameraXRotation;
        private float playerRotationOffset; //Changes based on direction.
        [SerializeField]
        private float highestAngle;
        [SerializeField]
        private float lowestAngle;


        #endregion
        #region MOVEMENT
        internal Vector3 movementExternal; //Out of loop movement data that doesn't get reset to 0.
        [SerializeField]
        internal bool sprintIsSetToToggled;
        internal bool isSprinting;
        [SerializeField]
        private float jumpForce;
        #endregion
        #region BODY DIRECTION
        [SerializeField]
        private float bodyRotateSpeed;
        [SerializeField]
        private float bodyYOffset;
        #endregion
        #region GRAVITY
        [SerializeField]
        private bool isUsingGravity;
        [SerializeField]
        private float base_gravityForce;
        private float active_gravityForce;
        #endregion
        #region GROUND CHECKING
        [SerializeField]
        private Vector3 groundCheckStartPoint;
        [SerializeField]
        private float radiusOfGroundCheckRay;
        [SerializeField]
        private float distanceToCheckForGround;
        [SerializeField]
        private LayerMask groundCheckLayerMask;
        internal bool isGrounded;
        #endregion
        #region ENVIRONMENT DETECTION
        [SerializeField]
        private Vector3 traverserStartPoint;
        [SerializeField]
        private float distanceToUseTraverse;
        [SerializeField]
        private LayerMask traverserMask;
        [SerializeField]
        private float distanceOffTheGround; //Step height
        [SerializeField]
        private float liftSpeed;
        #endregion
        #region DASH
        [SerializeField]
        private float base_dashPower;
        private float active_dashPower;
        private float timeUntilCharge;
        [SerializeField]
        private float base_timeBetweenMeleeAttacks;
        private float active_timeBetweenMeleeAttacks;


        #endregion
        #region ABILITIES


        #endregion
        #region BLUEPRINT AND HOTBAR



        #endregion
        #region MELEE
        [SerializeField]
        private float base_meleeDamage;
        private float active_meleeDamage;
        internal bool meleeActive; //Toggle Melee
        internal bool attackingWithMelee;
        [SerializeField]
        private GameObject[] swords;

        #endregion

        private EntitySystem_Core entityCore;
        private Movement entityMovement;
        private Player playerScript;
        private Rigidbody playerRigidbody;
        internal Transform playerVisibleBody;

        [SerializeField]
        private Vector3 killboxSize;
        private bool killboxActive;
        [SerializeField]
        private float base_killboxTime;
        private float active_killboxTime;
        [SerializeField]
        private float killboxDashSpeed;
        private Transform killboxTarget;


        [SerializeField]
        private LayerMask entityScanMask;
        [SerializeField]
        private string[] tagsToScan;


        #region INTERACTABLES
        [SerializeField]
        private GameObject fluxDestabilizerPrompt;
        private bool fluxDestabilizerPromptActive;
        internal GameObject fluxDestabilizer;



        #endregion

        void Start()
        {
            entityMovement = this.GetComponent<Movement>();
            playerScript = this.GetComponent<Player>();
            entityCore = this.GetComponent<EntitySystem_Core>();
            playerRigidbody = this.GetComponent<Rigidbody>();
            playerVisibleBody = entityCore.visibleBody.transform;
            playerVisibleBody.SetParent(null);
            cameraPivotPoint.SetParent(null);
            active_gravityForce = base_gravityForce;
            active_dashPower = base_dashPower;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if(playerScript.isDead == true)
            {
                return;
            }
            Sprinting();
            Jump();
            Dash();
            Abilities();
            CheckGround();
            HotbarInput();
            MeleeInputChecking();
            MeleeAttackCooldown();
            // Vector3 killboxPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + 30);
            //Gizmos.DrawCube(killboxPos, killboxSize);
            KillboxActive();
            InfoScan();
            CheckForInteractableInput();
        }

        void FixedUpdate()
        {
            if(playerScript.isDead == true)
            {
                GroundTraverse();
               //CameraMovement();
               // Movement();
                Gravity();
            }
            else
            {
                GroundTraverse();
                CameraMovement();
                Movement();
                Gravity();
            }
           
        }
        #region CAMERA FUNCTIONS
        private void CameraMovement()
        {
            float mouseX = Input.GetAxis("Mouse X"); //Horizontal mouse movement. Rotate objects on their y axis.
            float mouseY = -Input.GetAxis("Mouse Y"); //Vertical mouse movement. Rotate objects on their X axis. Is negative to have "up" as up, "down" be down.

            rotationY += mouseX * horizontalSensitivity * Time.deltaTime;
            rotationX += mouseY * verticalSensitivity * Time.deltaTime;
            
            if (rotationY > 360)
            {
                rotationY -= 360;
            }
            else if (rotationY < -360)
            {
                rotationY += 360;
            }

            rotationX = Mathf.Clamp(rotationX, lowestAngle, highestAngle);

            Quaternion cameraTargetRotation = Quaternion.Euler(rotationX, rotationY, 0);
            cameraPivotPoint.localRotation = cameraTargetRotation;
            Quaternion playerTargetRotation = Quaternion.Euler(0, rotationY + CameraOffset(), 0);

            playerVisibleBody.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + bodyYOffset, this.transform.position.z);
            playerVisibleBody.transform.rotation = Quaternion.RotateTowards(playerVisibleBody.transform.rotation, this.transform.rotation, bodyRotateSpeed);

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, playerTargetRotation, 150);
        }
        private float CameraOffset()
        {
            float targetRotationOffset = 0;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) //Any movement input, do this.
            {
                if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D)) //All movement at once, go nowhere.
                {
                    //No rotation applied. Either we're moving forward AND backward, or JUST left and right at the same time.
                    targetRotationOffset = 0;
                }
                else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.D))
                {
                    targetRotationOffset = 0;
                }
                else
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        targetRotationOffset = 0;
                        if (Input.GetKey(KeyCode.A))
                        {
                            targetRotationOffset += -45;
                        }
                        if (Input.GetKey(KeyCode.D))
                        {
                            targetRotationOffset += 45;
                        }
                    }
                    if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) //If we're only going LEFT
                    {
                        targetRotationOffset += -90;
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        targetRotationOffset += -180;

                        if (Input.GetKey(KeyCode.A))
                        {
                            targetRotationOffset += 45; //Add 45 to the -180 for a left down orientation.
                        }
                        if (Input.GetKey(KeyCode.D))
                        {
                            targetRotationOffset += 315;
                        }
                    }
                    if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) //If we're only going RIGHT
                    {
                        targetRotationOffset += 90;
                    }
                }
            }
            return targetRotationOffset;
        }
        #endregion
        #region MOVEMENT FUNCTIONS
        private void Sprinting()
        {
            if (sprintIsSetToToggled == true)
            {
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    if (isSprinting == false)
                    {
                        isSprinting = true;
                        GameSystem_UIManager.Instance.ToggleSprintIcon(true);
                    }
                    else
                    {
                        isSprinting = false;
                        GameSystem_UIManager.Instance.ToggleSprintIcon(false);
                    }
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (isSprinting != true)
                    {
                        GameSystem_UIManager.Instance.ToggleSprintIcon(true);
                    }
                    isSprinting = true;


                }
                else
                {
                    if (isSprinting == true)
                    {
                        GameSystem_UIManager.Instance.ToggleSprintIcon(false);
                    }
                    isSprinting = false;
                }

            }
        }
        private void Movement()
        {
            Vector3 movement = Vector3.zero;
            if (isSprinting == true)
            {
                movement = entityMovement.ControlledMovement(playerRigidbody, playerScript.enhanced_movementSpeed + playerScript.enhanced_sprintBoost);
            }
            else
            {
                movement = entityMovement.ControlledMovement(playerRigidbody, playerScript.enhanced_movementSpeed);
            }
            movementExternal = movement; //This variable is needed so that we have a slightly out-of-loop movement variable to call, that doesn't get reset by Vector3.zero before being called.
            playerRigidbody.AddForce(movement, ForceMode.Force); //Normalized movement is then applied to the rigidbody HERE.
        }
        private void CheckGround()
        {
            if(entityMovement.CheckForGround(this.transform, groundCheckStartPoint + this.transform.position, radiusOfGroundCheckRay, distanceToCheckForGround, groundCheckLayerMask) == true)
            {
                isGrounded = true;
                playerScript.availableJumps = playerScript.enhanced_jumpCount;
            }
            else
            {
                isGrounded = false;
            }
        }
        private void Gravity()
        {
            if(isUsingGravity == false)
            {
                return;
            }

            if (isGrounded == false)
            {
                if(active_gravityForce <= 0)
                {
                    active_gravityForce = base_gravityForce;
                }
                active_gravityForce += Mathf.Sqrt(active_gravityForce) / 10; //Exponential increase of gravity when falling.
                active_gravityForce = Mathf.Clamp(active_gravityForce, 0, 600); //Hard limit to prevent issues.
                entityMovement.ApplyGravity(playerRigidbody, active_gravityForce);
            }
            else
            {
                active_gravityForce = 0;
            }
        }
        private void GroundTraverse()
        {
                entityMovement.GroundTraverse(this.transform, traverserStartPoint + this.transform.position, distanceToUseTraverse, traverserMask, distanceOffTheGround, liftSpeed);
        }
        private void Jump()
        {
            if(playerScript.availableJumps > 0 && Input.GetKeyDown(KeyCode.Space))
            {
                entityMovement.ApplyJump(playerRigidbody, jumpForce + active_gravityForce); //We add the y velocity to nullify the current velocity. 
                active_gravityForce = base_gravityForce; //Reset gravity on jump.
                playerScript.availableJumps -= 1;
            }
        }
        private void Dash()
        {
            if(playerScript.availableDashes > 0 && Input.GetKeyDown(KeyCode.LeftShift))
            {
                playerScript.availableDashes -= 1;
                entityMovement.Dash(playerRigidbody, playerCamera.transform, active_dashPower + (playerScript.enhanced_movementSpeed * 2));
            }
            else if(playerScript.availableDashes <= 0)
            {
                if(timeUntilCharge > 0)
                {
                    timeUntilCharge -= 1 * Time.deltaTime;
                }
                else
                {
                    playerScript.availableDashes++;
                    timeUntilCharge = playerScript.enhanced_dashCooldown;
                }
            }
        }

        

        #endregion
        #region BLUEPRINT FUNCTIONS
        private void HotbarInput()
        {

            if (Input.GetAxis("Mouse ScrollWheel") > 0f) //Move left on mousewheel up
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    //Don't use this if we're pressing alt!
                }
                else
                {
                    if ((int)playerScript.hotbarState > 0)
                    {
                        //  (int)playerScript.hotbarState--;
                        playerScript.SelectHotbarSlot((int)playerScript.hotbarState - 1);
                    }
                    else if ((int)playerScript.hotbarState <= 0)
                    {
                        playerScript.SelectHotbarSlot(9);
                    }
                }
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0f) //Move right on mousewheel down
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    //Don't use this if we're pressing alt!
                }
                else
                {
                    if ((int)playerScript.hotbarState < 9)
                    {
                        //(int)playerScript.hotbarState++;
                        playerScript.SelectHotbarSlot((int)playerScript.hotbarState + 1);
                    }
                    else if ((int)playerScript.hotbarState >= 9)
                    {
                        playerScript.SelectHotbarSlot(0);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) //Select slot 1
            {
                playerScript.SelectHotbarSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) //Select slot 2
            {
                playerScript.SelectHotbarSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3)) //Select slot 3
            {
                playerScript.SelectHotbarSlot(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4)) //Select slot 4
            {
                playerScript.SelectHotbarSlot(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5)) //Select slot 5
            {
                playerScript.SelectHotbarSlot(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6)) //Select slot 6
            {
                playerScript.SelectHotbarSlot(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7)) //Select slot 7
            {
                playerScript.SelectHotbarSlot(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8)) //Select slot 8
            {
                playerScript.SelectHotbarSlot(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9)) //Select slot 9
            {
                playerScript.SelectHotbarSlot(8);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0)) //Select slot 10
            {
                playerScript.SelectHotbarSlot(9);
            }

            if (Input.GetKeyDown(KeyCode.Minus)) //Drop selected blueprint
            {
                playerScript.DropBlueprint();
            }
        }



#endregion
        #region UNIQUE FUNCTIONS
        private void Abilities()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (playerScript.canUseAbility == true)
                {
                    playerScript.ActivateAbility();
                }
            }
        }
        #region MELEE
        //Melee is a special and very important mechanic. It's the only type that can kill bosses, open enhancer containers, and has a powerful base damage.
        private void MeleeInputChecking()
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                ToggleMelee();
            }
            if(Input.GetMouseButtonDown(0) && attackingWithMelee == false && meleeActive)
            {
                MeleeAttack();
            }
        }
        private void ToggleMelee()
        {
            if (meleeActive == false)
            {
                Debug.Log("Melee is active)");
                entityCore.thisAnimator.SetLayerWeight(1, 1);
                meleeActive = true;
                foreach (GameObject go in swords)
                {
                    go.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Melee is inactive");
                meleeActive = false;
               entityCore.thisAnimator.SetLayerWeight(1, 0);
                foreach (GameObject go in swords)
                {
                    go.SetActive(false);
                }
            }
        }
        private void MeleeAttack()
        {
            attackingWithMelee = true;
            active_timeBetweenMeleeAttacks = base_timeBetweenMeleeAttacks;
            //Killbox();

           // Debug.Log("Slashing!");
            //Trigger animation slash
            entityCore.thisAnimator.SetTrigger("slash");

            foreach (GameObject go in swords)
            {
                go.GetComponent<Collider>().enabled = true;
            }


        }
        private void MeleeAttackCooldown()
        {
            if(attackingWithMelee == true)
            {
                if (active_timeBetweenMeleeAttacks > 0)
                {
                    active_timeBetweenMeleeAttacks -= 1 * Time.deltaTime;
                }
                else
                {
                    active_timeBetweenMeleeAttacks = 0;
                    attackingWithMelee = false;

                    foreach (GameObject go in swords)
                    {
                        go.GetComponent<Collider>().enabled = false;
                    }
                }
            }
        }
        private void Killbox()
        {
            Debug.Log("KILLBOX");
            Vector3 killboxPos = new Vector3(cameraPivotPoint.position.x, cameraPivotPoint.position.y + 5, cameraPivotPoint.position.z + 30);
            Collider[] killbox = Physics.OverlapBox(cameraPivotPoint.position, new Vector3(15, 15, 60), cameraPivotPoint.transform.rotation);

            Debug.Log("KBL" + killbox.Length);
            float closestEntity = Mathf.Infinity;
            Transform closestTarget = null;
            for(int i = 0; i < killbox.Length; i++)
            {
                if (killbox[i].CompareTag("Machine"))
                {
                    float distanceToTarget = Vector3.Distance(this.transform.position, killbox[i].transform.position);
                    if (distanceToTarget < closestEntity)
                    {
                        closestEntity = distanceToTarget;
                        closestTarget = killbox[i].transform;
                    }
                }

            }

            if (closestTarget != null)
            {
                //closestTarget.GetComponent<EntitySystem_Core>().Contact(entityCore, EntitySystem_Core.InteractionType.Stunning, 3);
               // Transform target = closestTarget.GetComponentInChildren<KillboxFlag>().killboxTarget;
               // killboxTarget = target;
               // target.transform.LookAt(this.transform);
                
               // Debug.Log("closest target: " + closestTarget + " is " + closestEntity + " away.");
                //this.playerRigidbody.AddForce(this.transform.forward * (closestEntity), ForceMode.Acceleration);
               // MoveToKillboxTarget();
            }
        }
        private void MoveToKillboxTarget()
        {
            active_killboxTime = base_killboxTime;
            killboxActive = true;
            this.transform.position = Vector3.MoveTowards(this.transform.position, killboxTarget.position, killboxDashSpeed);
        }
        private void KillboxActive()
        {
            if(active_killboxTime > 0 && killboxActive == true)
            {
                isUsingGravity = false;
                active_killboxTime -= 1 * Time.deltaTime;
                this.transform.position = Vector3.MoveTowards(this.transform.position, killboxTarget.position, killboxDashSpeed);
            }
            else
            {
                isUsingGravity = true;
                killboxActive = false;
            }
        }

        #endregion
        private void InfoScan()
        {
            Ray targetRay = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 15));
            Transform target = Utilities.InfoScanFromViewport(targetRay, 1, 10000, entityScanMask, tagsToScan);
            //  Debug.Log("Target acquired: " + target);

            if (target != null)
            {
                EntitySystem_Core targetCore = target.GetComponent<EntitySystem_Core>();
                if (targetCore == null)
                {
                    GameSystem_UIManager.Instance.window_entityTarget.SetActive(false);
                    GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Neutral);
                }
                else
                {
                    if (GameSystem.GameSystem_Core.Instance.pvpEnabled == true) //If so, we don't run any blocks. What's acting on us isn't ours and anything can act on us.
                    {

                        if (target.CompareTag("Sentry") || target.CompareTag("Proxy") || target.CompareTag("Player"))
                        {
                            if (entityCore.CheckOwnership(targetCore) == true)
                            {
                                GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Friendly);

                            }
                            else
                            {
                                GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Hostile);
                            }

                        }
                        else if (target.CompareTag("Machine"))
                        {
                            GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Hostile);

                        }
                        else
                        {
                            GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Neutral);

                        }
                    }
                    else
                    {
                        if (target.CompareTag("Sentry") || target.CompareTag("Proxy") || target.CompareTag("Player"))
                        {
                            GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Friendly);
                        }
                        else if (target.CompareTag("Machine"))
                        {
                            GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Hostile);

                        }
                        else
                        {
                            GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Neutral);

                        }
                    }
                    //End Crosshair update
                    //Begin updating HUD
                    GameSystem_UIManager.Instance.window_entityTarget.SetActive(true);
                    GameSystem_UIManager.Instance.text_entityName.text = "Lv: " + targetCore.entityInfo.active_entityLevel + " " + targetCore.entityInfo.entityName;
                    GameSystem_UIManager.Instance.text_entityHealth.text = targetCore.active_entityVitality + " / " + targetCore.active_entityMaxVitality;
                    GameSystem_UIManager.Instance.image_entityHealthBar.fillAmount = targetCore.active_entityVitality / targetCore.active_entityMaxVitality;
                    GameSystem_UIManager.Instance.image_entityPicture.sprite = targetCore.entityInfo.entityImage;
                }
            }
            else
            {
                GameSystem_UIManager.Instance.window_entityTarget.SetActive(false);
                GameSystem_UIManager.Instance.UpdateCrosshair(GameSystem_UIManager.ScanType.Neutral);
            }
        }

        #endregion

        #region INTERACTABLES
        private void CheckForInteractableInput()
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                if(fluxDestabilizerPromptActive == true && GameSystem_EntityManager.Instance.currentThreat >= GameSystem_EntityManager.Instance.currentThreatNeededToTriggerHive)
                {
                    ToggleFluxDestabilizer(null);
                    ActivateFluxDestabilizer();
                }
            }

        }
        internal void ToggleFluxDestabilizer(GameObject targetDestabilizer)
        {
            if(fluxDestabilizerPromptActive == true)
            {
                fluxDestabilizerPromptActive = false;
                fluxDestabilizerPrompt.SetActive(false);
            }
            else
            {
                fluxDestabilizerPromptActive = true;
                fluxDestabilizerPrompt.SetActive(true);
                fluxDestabilizer = targetDestabilizer;
            }
        }

        private void ActivateFluxDestabilizer()
        {
            fluxDestabilizer.GetComponent<FluxDestabilizer>().Activate();
            fluxDestabilizer.GetComponent<FluxDestabilizer>().playerTrigger.enabled = false;
            fluxDestabilizer = null;
        }



        #endregion
    }
}
