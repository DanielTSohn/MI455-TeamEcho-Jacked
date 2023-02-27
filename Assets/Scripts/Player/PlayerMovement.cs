using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DestroyIt;
using UnityEngine.Events;
public class PlayerMovement : MonoBehaviour
{
    #region Components
    [Header("Game Events")]
    [SerializeField]
    private GameEvent onJump;
    [SerializeField]
    private GameEvent pauseInput;
    [Header("Local Events")]
    public UnityEvent JumpRelaeased;
    public UnityEvent JumpCharged;
    public UnityEvent JumpCDRestored;
    public UnityEvent JumpLand;


    [Header("Component Holder")]
    [SerializeField]
    private PlayerComponents componentHolder;

    private GameObject player;
    private Camera playerCamera;
    #endregion

    #region Piston Parameters
    [SerializeField]
    private Rigidbody movementGuide;

    [SerializeField]
    private LayerMask groundLayers;

    public float BigJumpHeight { get { return bigJumpHeight; } private set { bigJumpHeight = value; } }
    [SerializeField]
    [Tooltip("The distance in meters the player jumps from input")]
    private float bigJumpHeight = 0.5f;

    public float MaxJumpHoldTime { get { return maxJumpHoldTime; } set { maxJumpHoldTime = value; } }
    [SerializeField]
    [Tooltip("Maximmum time, in seconds, the jump charge can be held")]
    private float maxJumpHoldTime = 1f;

    public float JumpCooldown { get { return jumpCooldown; } set { jumpCooldown = value; } }
    [SerializeField]
    [Tooltip("How long, in seconds, between jumps")]
    private float jumpCooldown = 1f;
    #endregion

    #region Control Parameters
    public float MovementMultiplier { get { return movementMultiplier; } set { movementMultiplier = value; } }
    [SerializeField]
    [Header("Control Parameters")]
    [Tooltip("The multiplier to the input 'balance'")]
    private float movementMultiplier = 1;
    [SerializeField] private PlayerGridInteraction pgi;
    #endregion

    #region Status Variables
    /// <summary>
    /// True: Piston is active and moving
    /// False: Piston is not active and not moving
    /// </summary>
    public bool PistonActive { get; private set; } = true;
    /// <summary>
    /// True: Player can move via input
    /// False: Player can not move 
    /// </summary>
    public bool CanMove { get; private set; } = true;
    /// <summary>
    /// True: Player is inputing a move input
    /// False: Player has no move input
    /// </summary>
    public bool Moving { get; private set; } = false;
    /// <summary>
    /// True: Player can aiming a jump via move inputs
    /// False: Player cannot aim a jump currently
    /// </summary>
    public bool JumpAim { get; private set; } = false;

    /// <summary>
    /// True: Player is inputing a jump input
    /// False: Player has no jump input
    /// </summary>
    public bool Jumping { get; private set; } = false;

    /// <summary>
    /// True: Movement guide is touching the ground
    /// False: Movement guide is midair
    /// </summary>
    public bool Grounded { get; private set; } = false;
    #endregion

    #region Private Variables
    private bool spawnerAvailable = false;
    private Vector2 movementInput;
    private bool jumpHoldIterrupt;
    private Collider[] terrains = new Collider[10];
    #endregion

    void Awake()
    {
        #region Component handling
        player = componentHolder.PlayerRoot;
        playerCamera = componentHolder.PlayerCamera;
        #endregion

        spawnerAvailable = SpawnPlayers.Instance != null;
        Debug.Log(spawnerAvailable);
        if(spawnerAvailable)
        {
            StartCoroutine(DelayedStart());
        }
        else
        {
            StartCoroutine(MoveCycle());
        }
    }

    private void FixedUpdate()
    {
        terrains = Physics.OverlapBox(movementGuide.position, movementGuide.transform.lossyScale, movementGuide.rotation, groundLayers);
        if(terrains.Length > 0)
        { 
            Grounded = true; 
            foreach(Collider collider in terrains)
            {
                if (collider.TryGetComponent(out Cell cell))
                {
                    pgi.OnCellHit(cell.gridLocation, cell.GetComponentInParent<GridSystem>());
                }

                if (collider.TryGetComponent(out Destructible destructible))
                {
                    destructible.ApplyDamage(1);
                }
            }
        }
        else { Grounded = false; }
    }

    private void OrientPlayer()
    {
        Vector3 lookDirection = componentHolder.PlayerCamera.transform.position - player.transform.position;
        lookDirection.y = 0;
        lookDirection.Normalize();
        player.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    private IEnumerator DelayedStart()
    {
        if (!SpawnPlayers.Instance.SceneReady)
        {
            yield return new WaitUntil(() => SpawnPlayers.Instance.SceneReady);
        }
        StartCoroutine(MoveCycle());
    }

    public bool ToggleMove() { return CanMove = !CanMove; }

    private void MovePlayer()
    {
        OrientPlayer();
        movementGuide.velocity = new Vector3(0, movementGuide.velocity.y, 0);
        Vector3 relativeForward = playerCamera.transform.forward;
        relativeForward.y = 0;
        relativeForward.Normalize();
        Vector3 relativeRight = playerCamera.transform.right;
        relativeRight.y = 0;
        relativeRight.Normalize();

        movementGuide.AddForce(movementInput.x * movementMultiplier * relativeRight + movementInput.y * movementMultiplier * relativeForward, ForceMode.VelocityChange);
    }

    private void RotatePlayer()
    {
        Vector3 relativeForward = playerCamera.transform.forward;
        relativeForward.y = 0;
        relativeForward.Normalize();
        Vector3 relativeRight = playerCamera.transform.right;
        relativeRight.y = 0;
        relativeRight.Normalize();
        player.transform.RotateAround(movementGuide.transform.position, relativeForward * -movementInput.x + relativeRight * movementInput.y, movementMultiplier/7);


    }

    public void ReadBalance(InputAction.CallbackContext balance)
    {
        if (balance.performed)
        {
            movementInput = balance.ReadValue<Vector2>();
            Moving = true;
        }
        else
        {
            Moving = false;
        }
    }

    private IEnumerator MoveCycle()
    {
        bool readyCheck = true;
        if (spawnerAvailable) { readyCheck = SpawnPlayers.Instance.SceneReady; }
        while (readyCheck)
        {
            if (!CanMove) { yield return new WaitUntil(() => CanMove); }
            if (!Moving) { yield return new WaitUntil(() => Moving); }

            if (JumpAim) { RotatePlayer(); }
            else { MovePlayer(); }

            yield return new WaitForFixedUpdate();
        }
    }

    public void ReadJump(InputAction.CallbackContext jump)
    {
        if (jump.performed && !Jumping)
        {
            PistonActive = false;
            jumpHoldIterrupt = false;
            JumpAim = true;
            StartCoroutine(JumpCounter());
        }
        else
        {
            jumpHoldIterrupt = true;
        }
    }

    private IEnumerator JumpCounter()
    {
        Jumping = true;
        float jumpHoldTime;
        for (jumpHoldTime = 0; jumpHoldTime < maxJumpHoldTime; jumpHoldTime += Time.fixedDeltaTime)
        {
            componentHolder.PlayerUI.UpdateJump(jumpHoldTime / maxJumpHoldTime);
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            if (jumpHoldIterrupt) { break; }
            yield return new WaitForFixedUpdate();
        }
        if(jumpHoldTime >= maxJumpHoldTime) { JumpCharged.Invoke(); }
        onJump.TriggerEvent();
        JumpRelaeased.Invoke();

        movementGuide.AddForce(Mathf.Sqrt(-2 * Physics.gravity.y * bigJumpHeight * (jumpHoldTime / maxJumpHoldTime)) * player.transform.up, ForceMode.VelocityChange);
        PistonActive = true;
        StartCoroutine(WaitGrounded());
        
        for (float cooldown = jumpCooldown; cooldown >= 0; cooldown -= Time.fixedDeltaTime)
        {
            componentHolder.PlayerUI.UpdateJumpCD(cooldown / jumpCooldown);
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            yield return new WaitForFixedUpdate();
        }
        JumpCDRestored.Invoke();
        componentHolder.PlayerUI.UpdateJumpCD(0);
        componentHolder.PlayerUI.UpdateJump(0);
        Jumping = false;
    }

    private IEnumerator WaitGrounded()
    {
        if (Grounded) { yield return new WaitWhile(() => Grounded); }
        Debug.Log("Departed");
        if (!Grounded) { yield return new WaitUntil(() => Grounded); }
        Debug.Log("Landed");
        JumpLand.Invoke();
        JumpAim = false;
        terrains = Physics.OverlapBox(movementGuide.position, new Vector3(3, 2, 3), movementGuide.rotation);
        if (terrains.Length > 0)
        {
            foreach (Collider collider in terrains)
            {
                if (collider.TryGetComponent(out Cell cell))
                {
                    pgi.OnCellHit(cell.gridLocation, cell.GetComponentInParent<GridSystem>());
                }

                if (collider.TryGetComponent(out Destructible destructible))
                {
                    destructible.ApplyDamage(1);
                }
            }
        }
    }

    public void ReadPause(InputAction.CallbackContext pause)
    {
        if (pause.performed)
        {
            if(pauseInput != null) { pauseInput.TriggerEvent(); }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(movementGuide.position, movementGuide.transform.lossyScale);
    }
}
