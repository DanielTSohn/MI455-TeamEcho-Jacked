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

    [Header("Required Components")]
    [SerializeField]
    [Tooltip("The player on top of the jackhammer")]
    private GameObject player;
    [SerializeField]
    [Tooltip("The player's rigidbody for controlling movement")]
    private Rigidbody playerRB;
    [SerializeField]
    [Tooltip("The collider of the jackhammer, used to scale spherecast (jumping and destruction checks)")]
    private CapsuleCollider playerCollider;
    [SerializeField]
    [Tooltip("Basis to move controls too")]
    private Transform relativeAnchor;
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
    #endregion

    #region Status Variables
    /// <summary>
    /// True: Piston is active and moving
    /// False: Piston is not active and not moving
    /// </summary>
    public bool PistonActive { get; private set; } = false;
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
    /// True: Player is inputing a jump input
    /// False: Player has no jump input
    /// </summary>
    public bool Jumping { get; private set; } = false;
    #endregion

    #region Private Variables
    private Vector2 balanceInput;
    // arrange with component setter singleton later
    private bool sceneActive = true;
    private bool jumpHoldIterrupt;
    private Collider[] terrains = new Collider[10];
    private float initialRadius;
    #endregion

    #region Misc Variables
    [SerializeField] public Color playerColorIdentifier { get; private set; } = Color.red;
    #endregion

    void Awake()
    {
        #region Component handling
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("No player object attatched or found!");
            }
            else
            {
                if (playerRB == null)
                {
                    if (!player.TryGetComponent(out playerRB))
                    {
                        Debug.LogError("No rigidbody attached found on the player object!");
                    }
                }
            }
        }
        if (playerCollider == null)
        {
            if (player != null)
            {
                if (!player.TryGetComponent(out playerCollider))
                {
                    Debug.LogError("No capsule collider attached found on the player object!");
                }
            }
        }

        if (playerCollider != null) { initialRadius = playerCollider.radius; }
        
        if(relativeAnchor == null) { relativeAnchor = Camera.main.transform; }
        #endregion
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        if (!sceneActive)
        {
            yield return new WaitUntil(() => sceneActive);
        }
        PistonActive = true;
        StartCoroutine(MoveCycle());
        StartCoroutine(CyclePiston());
    }

    public bool ToggleMove() { return CanMove = !CanMove; }

    private void MovePlayer()
    {
        playerRB.angularVelocity = Vector3.zero;
        Vector3 relativeForward = relativeAnchor.forward;
        relativeForward.y = 0;
        relativeForward.Normalize();
        Vector3 relativeRight = relativeAnchor.right;
        relativeRight.y = 0;
        relativeRight.Normalize();

        playerRB.AddTorque((-balanceInput.x * relativeForward + balanceInput.y * relativeRight) * BalanceMultiplier, ForceMode.VelocityChange);
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
        for (cooldown = jumpCooldown; cooldown >= 0; cooldown -= Time.fixedDeltaTime)
        {
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            yield return new WaitForFixedUpdate();
        }
        Jumping = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(player.transform.TransformPoint(new Vector3(0, sphereCastVerticalOffset, 0)), initialRadius + 0.05f);
    }
}
