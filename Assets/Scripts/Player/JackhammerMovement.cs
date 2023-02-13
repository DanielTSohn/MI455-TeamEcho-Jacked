using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DestroyIt;

public class JackhammerMovement : MonoBehaviour
{
    #region Components
    [Header("Game Events")]
    [SerializeField]
    private GameEvent jumpUpdate;
    [SerializeField]
    private GameEvent jumpCDUpdate;
    [SerializeField]
    private GameEvent onJump;

    [Header("Component Holder")]
    [SerializeField]
    private PlayerComponents componentHolder;

    private GameObject player;
    private Rigidbody playerRB;
    private CapsuleCollider playerCollider;
    #endregion

    #region Piston Parameters
    [SerializeField]
    [Tooltip("Offset of the spherecast downwards")]
    private float sphereCastVerticalOffset = 1.5f;

    [SerializeField]
    private LayerMask groundLayers;
    public float JumpHeight { get { return jumpHeight; } private set { jumpHeight = value; } }
    [Header("Piston Parameters")]
    [SerializeField]
    [Tooltip("The distance in meters the player jumps")]
    private float jumpHeight = 0.1f;

    public float BigJumpHeight { get { return bigJumpHeight; } private set { bigJumpHeight = value; } }
    [SerializeField]
    [Tooltip("The distance in meters the player jumps from input")]
    private float bigJumpHeight = 0.5f;

    public float PistonCycleTime { get { return pistonCycleTime; } set { pistonCycleTime = value; } }
    [SerializeField]
    [Tooltip("Time for one full piston cycle")]
    private float pistonCycleTime = 0.04f;

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
    public float BalanceMultiplier { get { return balanceMultiplier; } set { balanceMultiplier = value; } }
    [SerializeField]
    [Header("Control Parameters")]
    [Tooltip("The multiplier to the input 'balance'")]
    private float balanceMultiplier = 1;

    public float RotateMultiplier { get { return rotateMultiplier; } set { rotateMultiplier = value; } }
    [SerializeField]
    [Header("Control Parameters")]
    [Tooltip("The multiplier to the input 'balance'")]
    private float rotateMultiplier = 1;
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
    /// True: Player is inputing a rotate input
    /// False: Player has no rotate input
    /// </summary>
    public bool Rotating { get; private set; } = false;

    /// <summary>
    /// True: Player is inputing a jump input
    /// False: Player has no jump input
    /// </summary>
    public bool Jumping { get; private set; } = false;
    #endregion

    #region Private Variables
    private Vector2 balanceInput;
    private float rotateInput;
    // arrange with component setter singleton later
    private bool sceneActive = true;
    private bool jumpHoldIterrupt;
    private Collider[] terrains = new Collider[10];
    [HideInInspector]
    public float jumpProportion = 0;
    [HideInInspector]
    public float jumpCDProportion = 0;
    private float initialRadius;
    #endregion

    #region Misc Variables
    [SerializeField] public Color playerColorIdentifier { get; private set; } = Color.red;
    [SerializeField] private PlayerGridInteraction pgi;
    #endregion

    void Awake()
    {
        #region Component handling
        player = componentHolder.PlayerRoot;
        playerRB = componentHolder.JackhammerRB;
        playerCollider = componentHolder.PlayerCollider;
        #endregion
        if (playerCollider != null) { initialRadius = playerCollider.radius; }
        pgi = GetComponent<PlayerGridInteraction>();

        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        if (!sceneActive)
        {
            yield return new WaitUntil(() => sceneActive);
        }
        StartCoroutine(MoveCycle());
        StartCoroutine(PlayerRotate());
        StartCoroutine(CyclePiston());
    }

    public bool ToggleMove() { return CanMove = !CanMove; }

    private void MovePlayer()
    {
        playerRB.angularVelocity = Vector3.zero;
        playerRB.AddRelativeTorque(new Vector3(balanceInput.y, 0, -balanceInput.x) * BalanceMultiplier, ForceMode.VelocityChange);
    }

    private void RotatePlayer()
    {
        playerRB.angularVelocity = new Vector3(playerRB.angularVelocity.x, 0, playerRB.angularVelocity.z);
        playerRB.AddRelativeTorque(new Vector3(0, rotateInput, 0) * rotateMultiplier, ForceMode.VelocityChange);
    }

    public void ReadBalance(InputAction.CallbackContext balance)
    {
        if (balance.performed)
        {
            balanceInput = balance.ReadValue<Vector2>();
            Moving = true;
        }
        else
        {
            Moving = false;
        }
    }

    public void ReadRotate(InputAction.CallbackContext rotate)
    {
        if (rotate.performed)
        {
            rotateInput = rotate.ReadValue<float>();
            Rotating = true;
        }
        else
        {
            Rotating = false;
        }
    }

    private IEnumerator MoveCycle()
    {
        while (sceneActive)
        {
            if (!CanMove) { yield return new WaitUntil(() => CanMove); }
            if (!Moving) { yield return new WaitUntil(() => Moving); }

            MovePlayer();

            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator PlayerRotate()
    {
        while (sceneActive)
        {
            if (!CanMove) { yield return new WaitUntil(() => CanMove); }
            if (!Rotating) { yield return new WaitUntil(() => Rotating); }

            RotatePlayer();

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator CyclePiston()
    {
        while (sceneActive)
        {
            if (!PistonActive) { yield return new WaitUntil(() => PistonActive); }
            terrains = Physics.OverlapSphere(player.transform.TransformPoint(new Vector3(0, sphereCastVerticalOffset, 0)), initialRadius + 0.05f, groundLayers);
            bool validTerrain = false;
            foreach (Collider tile in terrains)
            {
                if (tile != null)
                {
                    if(tile.TryGetComponent(out Cell cell))
                    {
                        pgi.OnCellHit(cell.gridLocation);
                    }

                    validTerrain = true;
                    if (tile.TryGetComponent(out Destructible tileDestroy))
                    {
                        tileDestroy.ApplyDamage(1);
                    }
                }
            }
            if (validTerrain)
            {
                playerRB.velocity = Vector3.ClampMagnitude(playerRB.velocity, Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight) * 6.5f);
                playerRB.AddRelativeForce(Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight) * Vector3.up, ForceMode.VelocityChange);
            }
            yield return new WaitForSeconds(pistonCycleTime);
        }
    }

    public void ReadJump(InputAction.CallbackContext jump)
    {
        if (jump.performed && !Jumping)
        {
            PistonActive = false;
            jumpHoldIterrupt = false;
            StartCoroutine(JumpCounter());
            playerRB.rotation.SetLookRotation(new Vector3(0, playerRB.rotation.y, 0));
            playerRB.velocity = Vector3.zero;
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
            jumpProportion = jumpHoldTime / maxJumpHoldTime;
            jumpUpdate.TriggerEvent(gameObject);
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            if (jumpHoldIterrupt) { break; }
            yield return new WaitForFixedUpdate();
        }

        jumpUpdate.TriggerEvent(gameObject);
        onJump.TriggerEvent();
        playerRB.AddRelativeForce(Mathf.Sqrt(-2 * Physics.gravity.y * bigJumpHeight * (jumpHoldTime / maxJumpHoldTime)) * Vector3.up, ForceMode.VelocityChange);
        PistonActive = true;
        float cooldown;
        jumpCDProportion = 1;
        for (cooldown = jumpCooldown; cooldown >= 0; cooldown -= Time.fixedDeltaTime)
        {
            if (cooldown < jumpCooldown / 2) { jumpProportion = 0; }
            jumpCDProportion = cooldown / jumpCooldown;
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            yield return new WaitForFixedUpdate();
        }
        jumpCDProportion = 0;
        Jumping = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(player.transform.TransformPoint(new Vector3(0, sphereCastVerticalOffset, 0)), initialRadius + 0.05f);
    }
}
