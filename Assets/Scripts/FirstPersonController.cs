using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;
using System;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && onSurface;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && onSurface;
    private bool ShouldClimb => Input.GetKeyDown(climbKey) && onWall; //not sure if this'll be used


    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool willSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool useThirdPersonCamera = true;
    [SerializeField] private bool useSurfaceChecks = true;
    [SerializeField] private bool canClimb = true;



    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode camKey = KeyCode.Mouse2;
    [SerializeField] private KeyCode climbKey = KeyCode.R;

    
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;
    [SerializeField] private float climbSpeed = 2.5f;

    
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    
    [Header("Camera Parameters")]
    [SerializeField] private GameObject firstCamera;
    [SerializeField] private GameObject thirdCamera;
    [SerializeField] private int camMode;
    [SerializeField] private float timeBeforeCamChange = 0.01f;
    private Coroutine togglingCamera;

    
    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float timeBeforeRegenStarts = 3f;
    [SerializeField] private float healthValueIncrement = 1f;
    [SerializeField] private float healthTimeIncrement = 0.1f;
    private float curHealth;
    private Coroutine regeneratingHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;


    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaUseMultiplier = 5;
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5f;
    [SerializeField] private float staminaValueIncrement = 2f;
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private float curStamina;
    private Coroutine regeneratingStamina;
    public static Action<float> OnStaminaChange;


    [Header("Collision & Gravity")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 2.0f;
    [SerializeField] private float surfaceCheckRadius = 0.3f;
    [SerializeField] private Vector3 surfaceCheckOffset;
    [SerializeField] private LayerMask surfaceLayer;
    private bool onSurface;

    
    [Header("Crouch Parameters")]
    [SerializeField] private float crouchingHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0,0,0);
    private bool isCrouching;
    private bool duringCrouchAnimation;


    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;


    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;


    [Header("Climb Parameters")]
    private bool isClimbing;
    private bool onWall = false;
    private bool onWallTop = false;


    [Header("UI Parameters")]
    [SerializeField] private string contextText;
    public static Action<string> OnContext;

    
    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] metalClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    private float footstepTimer = 0;
    private float GetCurOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : IsSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    [Header("Player Animator")]
    [SerializeField] private Animator animator;

    //SLIDING PARAMETERS
    private Vector3 hitPointNormal;

    private bool IsSliding{
        get{
            if(onSurface && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f)){
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            } else {
                return false;
            }
        }
    }

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable curInteractable;

    [Header("States")]
    [SerializeField] public PlayerState state = PlayerState.CLIMBING;
    public enum PlayerState{
        WALKING,
        FALLING,
        CLIMBING,
        RUNNING,
        CROUCHING,
        GLIDING,
        TALKING
    }

    [Header("Body Meshes")]
    [SerializeField] private GameObject headMesh;
    [SerializeField] private GameObject shirtMesh;

    private Camera playerCamera;
    private CharacterController characterController;

    

    private Vector3 startPos;


    private Vector3 moveDirection;
    private Vector2 curInput;

    private float rotationX = 0;


    private void OnEnable(){
        OnTakeDamage += ApplyDamage;
    }

    private void OnDisable(){
        OnTakeDamage -= ApplyDamage;
    }

    void Awake(){
        // playerCamera = GetComponentInChildren<Camera>();
        playerCamera = firstCamera.GetComponent<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        curHealth = maxHealth;
        curStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        startPos = transform.position;
    }


    void Update(){

        

        if(CanMove){
            HandleMovementInput();
            HandleMouseLook();

            if(useSurfaceChecks){
                HandleSurfaceChecks();
            }

            if(canJump){
                HandleJump();
            }

            if(canCrouch){
                HandleCrouch();
            }

            if(canUseHeadbob){
                HandleHeadbob();
            }

            if(canZoom){
                HandleZoom();
            }

            if(useFootsteps){
                HandleFootsteps();
            }

            if(canInteract){
                HandleInteractionCheck();
                HandleInteractionInput();
            }

            if(useStamina){
                HandleStamina();
            }

            if(useThirdPersonCamera){
                HandleCameraToggle();
            }

            if(canClimb){
                HandleClimb();
            }

            if(animator){
                animator.SetInteger("State", (int)state);
            }

            ApplyFinalMovements();
        }
    }



    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "respawn"){
            characterController.enabled = false;
            transform.position = startPos;
            characterController.enabled = true;
            print("RESPAWN");
        }

        if(other.gameObject.tag == "PickUp"){
            other.gameObject.GetComponent<AudioSource>().Play();
            other.gameObject.GetComponent<ParticleSystem>().Play();
            other.gameObject.GetComponent<MeshRenderer>().enabled = false;
            other.gameObject.GetComponent<Light>().enabled = false;
            other.gameObject.GetComponent<BoxCollider>().enabled = false;

            // other.gameObject.SetActive(false);
            // pickupCount++;
            // countText.text = "Count: " + pickupCount.ToString();
            ApplyDamage(-20f);

            
        }
    }


    //movementAmount is used for animation of the player 
    //Sets moveDirection as a mix of
    private void HandleMovementInput(){
        curInput = new Vector2((IsSprinting ? sprintSpeed: isCrouching ? crouchSpeed : isClimbing ? climbSpeed : walkSpeed) * Input.GetAxis("Vertical"), (IsSprinting ? sprintSpeed: isCrouching ? crouchSpeed : isClimbing ? climbSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float movementAmount = Mathf.Clamp01(Mathf.Abs(curInput.x) + Mathf.Abs(curInput.y));

        
        if(!isClimbing){
            float moveDirectionY = moveDirection.y;
            moveDirection = (transform.TransformDirection(Vector3.forward) * curInput.x) + (transform.TransformDirection(Vector3.right) * curInput.y);
            moveDirection.y = moveDirectionY;
        } 
        
        // print("Move Direction1: " + moveDirection);
        animator.SetFloat("movementValue", movementAmount, 0.2f, Time.deltaTime);
    }


    private void HandleMouseLook(){
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }


    private void HandleCameraToggle(){

        if(Input.GetKeyDown(camKey)){
            switch(camMode){
                case 0: 
                    camMode = 1;
                    break;
                case 1:
                    camMode = 0;
                    break;
            }

            togglingCamera = StartCoroutine(ToggleCamera());
        }

        // if(camMode == 0){
            
        //     firstCamera.transform.position = headMesh.transform.position;
        // }
    }

    private void HandleJump(){
        if(ShouldJump){
            moveDirection.y = jumpForce;
        }
    }


    private void HandleCrouch(){
        if(ShouldCrouch){
            StartCoroutine(CrouchStand());
        }
    }


    //TO DO Add animations
    //TO DO Check if at the top of the wall with another raycast
    //To do, check if the player is trying to go down and OnSurface == true, make them not climb and just put them down on the ground, not below it.
    //TO DO Check if there are any obstacles that would stop from moving on the wall on the sides.
    private void HandleClimb(){

        // print("Handle Climb");

        

        //do clumb stuff
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        animator.SetFloat("H", h);
        animator.SetFloat("V", v);

        Vector2 input = SquareToCircle(new Vector2(h, v));

        Vector3 offset = transform.TransformDirection(Vector2.one * 0.5f);
        Vector3 checkDirection = Vector3.zero;
        int k = 0;

        for(int i = 0; i < 4; i++){
            RaycastHit checkHit;
            if(Physics.Raycast(transform.position + offset, transform.forward, out checkHit)){
                checkDirection += checkHit.normal;
                k++;
            }

            //Rotate Offset by 90 degrees
            offset = Quaternion.AngleAxis(90f, transform.forward) * offset;
        }
        checkDirection /= k;


        //Check wall directly in front
        RaycastHit hit;
        if(Physics.Raycast(transform.position, -checkDirection, out hit, 1f)){
            float dot = Vector3.Dot(transform.forward, -hit.normal);

            Debug.DrawRay(transform.position, transform.forward, Color.green,3);

            // print("Wall In Front");

            if(isClimbing){
                

                transform.forward = Vector3.Lerp(transform.forward, -hit.normal, 10f * Time.fixedDeltaTime);

                characterController.enabled = false;

                // transform.position = Vector3.Lerp(transform.position, hit.point + hit.normal * 0.55f, 5f * Time.fixedDeltaTime);
                transform.position += new Vector3(h * Time.fixedDeltaTime, v * Time.fixedDeltaTime, 0f);
                
                // print("Lerp: " + Vector3.Lerp(transform.position, hit.point + hit.normal * 0.55f, 5f * Time.fixedDeltaTime));
                // print("Plus Equals" + h * Time.fixedDeltaTime + ", " + v * Time.fixedDeltaTime);


                //Checking if the player is at the top of a wall
                RaycastHit headHit;
                if(Physics.Raycast(transform.position + new Vector3(0, 1, 0), -checkDirection, out headHit, 1f)){

                    Debug.DrawRay(transform.position + new Vector3(0, 1, 0), -checkDirection, Color.blue,3);

                    onWallTop = false;
                } else{

                    Debug.DrawRay(transform.position + new Vector3(0, 1, 0), -checkDirection, Color.red,3);
                    
                    if(onWallTop){
                        RaycastHit diagDownHit;
                        if(Physics.Raycast(transform.position + new Vector3(0, 1.5f, 0), -checkDirection + new Vector3(0,-0.66f,0), out diagDownHit, 1f)){
                            Debug.DrawRay(transform.position + new Vector3(0, 1.5f, 0), -checkDirection + new Vector3(0,-0.66f,0), Color.cyan,3);
                            print(v);
                            if(v >= 0.25f){
                                transform.position += new Vector3(0,2,1.5f);
                                isClimbing = false;
                                characterController.enabled = true;
                            }
                        } else{
                            Debug.DrawRay(transform.position + new Vector3(0, 1.5f, 0), -checkDirection + new Vector3(0,-0.66f,0), Color.yellow,3);
                        }
                    }

                    onWallTop = true;

                    
                }


                
                if(Input.GetKeyDown(climbKey)){
                    isClimbing = false;
                    moveDirection = Vector3.zero;
                    characterController.enabled = true;
                }
            } else{
                if(Input.GetKeyDown(climbKey)){
                    isClimbing = true;
                }
            }

            

            
        } else {
            // state = PlayerState.FALLING;
        }

        
        
    }

    


    private void HandleHeadbob(){
        if(!onSurface) return;

        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f){
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z
            );
        }
    }
    

    private void HandleZoom(){
        if(Input.GetKeyDown(zoomKey)){
            if(zoomRoutine != null){
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(true));
        }

        if(Input.GetKeyUp(zoomKey)){
            if(zoomRoutine != null){
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
    }


    private void HandleInteractionCheck(){
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance)){
            if(hit.collider.gameObject.layer == 9 && (curInteractable == null || hit.collider.gameObject.GetInstanceID() != curInteractable.GetInstanceID())){
                hit.collider.TryGetComponent(out curInteractable);

                if(curInteractable){
                    curInteractable.OnFocus();
                }
            } else if (curInteractable){
                curInteractable.OnLoseFocus();
                curInteractable = null;
            }
        }
    }


    private void HandleInteractionInput(){
        if(Input.GetKeyDown(interactKey) && curInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer)){
            curInteractable.OnInteract();
        }
    }


    //Sets health lower when the player takes damage
    //takes float dmg 
    private void ApplyDamage(float dmg){
        curHealth -= dmg;
        OnDamage?.Invoke(curHealth);

        if(curHealth <= 0){
            KillPlayer();
        } else if (regeneratingHealth != null){
            StopCoroutine(regeneratingHealth);
        }

        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }


    //TO DO Do some other stuff when the player dies.
    //Set a respawn when the character dies
    //Also make a plane or something that kills the player when they fall off the edge
    private void KillPlayer(){
        curHealth = 0;

        if(regeneratingHealth != null){
            StopCoroutine(regeneratingHealth);
        }

        print("DEAD");
    }


    //handles everything Stamina related
    private void HandleStamina(){
        //if the player is sprinting and moving
        if((IsSprinting || isClimbing) && curInput != Vector2.zero){

            //stops stamina regen
            if(regeneratingStamina != null){
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }

            //I think staminaUseMultiplier can be changed by different types of movement
            //TO DO Make running, jumping, climbing... have different stamina multipliers
            curStamina -= staminaUseMultiplier * Time.deltaTime;

            //makes sure stamina doesn't go below 0
            if(curStamina < 0){
                curStamina = 0;
            }

            //Sets stamina in UI
            OnStaminaChange?.Invoke(curStamina);

            //Turns off sprint if out of stamina
            if(curStamina <= 0){
                canSprint = false;
            }
        }

        if(!IsSprinting && !isClimbing && curStamina < maxStamina && regeneratingStamina == null){
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }


    //Calls the Move function of the characterController
    private void ApplyFinalMovements(){
        
        if(isClimbing){
            // moveDirection.y += climbSpeed * Time.deltaTime;
        } else if(!onSurface){
            // print("IS NOT GROUNDED, SHOULD BE FALLING");
            moveDirection.y -=   gravity * Time.deltaTime;
        }


        if(willSlideOnSlopes && IsSliding){
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        // print("Move Direction2: " + moveDirection);
        characterController.Move(moveDirection * Time.deltaTime);
    }


    //This triggers sounds for footsteps
    private void HandleFootsteps(){
        if(!onSurface) return;

        if(curInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if(footstepTimer <=0){
            if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3)){
                switch(hit.collider.tag){
                    case "Footsteps/WOOD":
                        footstepAudioSource.PlayOneShot(woodClips[Random.Range(0, woodClips.Length)]);
                        break;
                    case "Footsteps/METAL":
                        footstepAudioSource.PlayOneShot(metalClips[Random.Range(0, metalClips.Length)]);
                        break;
                    case "Footsteps/GRASS":
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length)]);
                        break;
                    default:
                        break;
                }
            }

            footstepTimer = GetCurOffset;
        }
    }

    //Check if the player is on a surface with the layer being surfaceLayer
    private void HandleSurfaceChecks(){
        onSurface = Physics.CheckSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius, surfaceLayer);
        // print("Player on surface: " + onSurface);


        //TO DO make this state setting a new function
        if(onSurface){
            state = PlayerState.WALKING;
        } else if(isClimbing){
            state = PlayerState.CLIMBING;
        } else{
            state = PlayerState.FALLING;
        }
    }

    //This is pretty cool, it draws in the editor, not the game
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius);
    }

    //Maps input for joy sticks for climbing
    Vector2 SquareToCircle(Vector2 input){
        return (input.sqrMagnitude >= 1f) ? input.normalized : input;
    }

    //Toggles crouching and standing
    //Def will need work with the full humanoid model --currently clips model through ground
    private IEnumerator CrouchStand(){
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f)){
            yield break;
        }


        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchingHeight;
        float curHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 curCenter = characterController.center;

        while(timeElapsed < timeToCrouch){
            characterController.height = Mathf.Lerp(curHeight, targetHeight, timeElapsed/timeToCrouch);
            characterController.center = Vector3.Lerp(curCenter, targetCenter, timeElapsed/timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;

    }

    //Toggles between first and third person camera
    private IEnumerator ToggleCamera(){
    yield return new WaitForSeconds(timeBeforeCamChange);

    SkinnedMeshRenderer headRend = headMesh.GetComponent<SkinnedMeshRenderer>();
    SkinnedMeshRenderer shirtRend = shirtMesh.GetComponent<SkinnedMeshRenderer>();

    switch(camMode){
        case 0:
            thirdCamera.SetActive(false);
            firstCamera.SetActive(true);

            headRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            shirtRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            print("Set to First Person");
            break;
        case 1:
            firstCamera.SetActive(false);
            thirdCamera.SetActive(true);

            headRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            shirtRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            print("Set to Third Person");
            break;
    }
}


    //Takes in value isEnter to determine if it is zooming in or out
    //doesn't currently work in 3rd person
    private IEnumerator ToggleZoom(bool isEnter){
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while (timeElapsed < timeToZoom){
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        zoomRoutine = null;
    }

    
    //Regenerates Health after waiting a bit of time
    //updates UI with current health value
    private IEnumerator RegenerateHealth(){

        yield return new WaitForSeconds(timeBeforeRegenStarts);

        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while(curHealth < maxHealth){
            curHealth += healthValueIncrement;

            if(curHealth > maxHealth){
                curHealth = maxHealth;
            }

            OnHeal?.Invoke(curHealth);

            yield return timeToWait;
        }

        regeneratingHealth = null;

    }

    //Regenerates stamina after waiting a bit of time
    //updates UI with current stamina value
    private IEnumerator RegenerateStamina(){
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while(curStamina < maxStamina){
            if(curStamina > 0){
                canSprint = true;
            }

            curStamina += staminaValueIncrement;

            if(curStamina > maxStamina){
                curStamina = maxStamina;
            }

            OnStaminaChange?.Invoke(curStamina);

            yield return timeToWait;
        }

        regeneratingStamina = null;
    }

}
