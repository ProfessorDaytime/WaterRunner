using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;
using System;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

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



    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode camKey = KeyCode.Mouse2;

    
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;

    
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


    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    
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



    //SLIDING PARAMETERS
    private Vector3 hitPointNormal;

    private bool IsSliding{
        get{
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f)){
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


    private Camera playerCamera;
    private CharacterController characterController;

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
    }


    void Update(){
        if(CanMove){
            HandleMovementInput();
            HandleMouseLook();

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

            ApplyFinalMovements();
        }
    }


    private void HandleMovementInput(){
        curInput = new Vector2((IsSprinting ? sprintSpeed: isCrouching ? crouchSpeed : walkSpeed) * Input.GetAxis("Vertical"), (IsSprinting ? sprintSpeed: isCrouching ? crouchSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * curInput.x) + (transform.TransformDirection(Vector3.right) * curInput.y);
        moveDirection.y = moveDirectionY;
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


    private void HandleHeadbob(){
        if(!characterController.isGrounded) return;

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


    private void KillPlayer(){
        curHealth = 0;

        if(regeneratingHealth != null){
            StopCoroutine(regeneratingHealth);
        }

        print("DEAD");
    }


    private void HandleStamina(){
        if(IsSprinting && curInput != Vector2.zero){

            if(regeneratingStamina != null){
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }

            curStamina -= staminaUseMultiplier * Time.deltaTime;

            if(curStamina < 0){
                curStamina = 0;
            }

            OnStaminaChange?.Invoke(curStamina);

            if(curStamina <= 0){
                canSprint = false;
            }
        }

        if(!IsSprinting && curStamina < maxStamina && regeneratingStamina == null){
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }


    private void ApplyFinalMovements(){
        if(!characterController.isGrounded){
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if(willSlideOnSlopes && IsSliding){
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }


    private void HandleFootsteps(){
        if(!characterController.isGrounded) return;

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

    private IEnumerator ToggleCamera(){
        yield return new WaitForSeconds(timeBeforeCamChange);

        switch(camMode){
            case 0:
                thirdCamera.SetActive(false);
                firstCamera.SetActive(true);
                break;
            case 1:
                firstCamera.SetActive(false);
                thirdCamera.SetActive(true);
                break;
        }
    }


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
