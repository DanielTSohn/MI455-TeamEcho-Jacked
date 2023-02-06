using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JackhammerStandard : MonoBehaviour
{
 #region Components
    [Header("Required Components")]
    [SerializeField]
    [Tooltip("The player on top of the jackhammer")]
    private GameObject player;
    [SerializeField]
    [Tooltip("The player's rigidbody for controlling movement")]
    private Rigidbody playerRB;
    [SerializeField]
    private CapsuleCollider playerCollider;
    #endregion

    #region Piston Parameters
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
    private Collider[] terrain = new Collider[1];
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
        #endregion
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        if(!sceneActive)
        {
            yield return new WaitUntil(() => sceneActive);
        }
        StartCoroutine(PlayerMovement());
        StartCoroutine(PlayerRotate());
        StartCoroutine(CyclePiston());
    }

    public bool ToggleMove(){ return CanMove = !CanMove; }

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
        if(rotate.performed)
        {
            rotateInput = rotate.ReadValue<float>();
            Rotating = true;
        }
        else
        {
            Rotating = false;
        }
    }

    private IEnumerator PlayerMovement()
    {
        while(sceneActive)
        {
            if (!CanMove) { yield return new WaitUntil(()=>CanMove); }
            if (!Moving) { yield return new WaitUntil(()=>Moving); }

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
        while(sceneActive)
        {
            if (!PistonActive) { yield return new WaitUntil(() => PistonActive); }
            Physics.OverlapSphereNonAlloc(player.transform.TransformPoint(Vector3.down / 2), 0.55f, terrain, groundLayers);
            if (terrain[0] != null)
            {
                playerRB.velocity = Vector3.ClampMagnitude(playerRB.velocity, Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight)*6.5f);
                playerRB.AddRelativeForce(Mathf.Sqrt(-2*Physics.gravity.y*jumpHeight) * Vector3.up, ForceMode.VelocityChange);
                terrain[0] = null;
            }
            yield return new WaitForSeconds(pistonCycleTime);
        }
    }

    public void ReadJump(InputAction.CallbackContext jump)
    {
        if(jump.performed && !Jumping)
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
            if(TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            if (jumpHoldIterrupt) { break; }
            yield return new WaitForFixedUpdate();
        }

        playerRB.AddRelativeForce(Mathf.Sqrt(-2 * Physics.gravity.y * bigJumpHeight * (jumpHoldTime / maxJumpHoldTime)) * Vector3.up, ForceMode.VelocityChange);
        PistonActive = true;
        yield return new WaitForSeconds(jumpCooldown);
        Jumping = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(player.transform.TransformPoint(Vector3.down / 2), 0.52f);
    }
}