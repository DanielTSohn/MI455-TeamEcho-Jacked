using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JankHammer : MonoBehaviour
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
    [Tooltip("The piston game object, will be moved up and down")]
    private GameObject piston;
    [SerializeField]
    [Tooltip("The rigidbody of the piston, used to move the game object through physics")]
    private Rigidbody pistonRB;
    #endregion

    #region Piston Parameters
    public float PistonMoveDistance { get { return pistonMoveDistance; } private set { pistonMoveDistance = value; } }
    [Header("Piston Parameters")]
    [SerializeField]
    [Tooltip("The distance in meters the piston moves in the jackhammer")]
    private float pistonMoveDistance = 0.1f;

    public float PistonCycleTime { get { return pistonCycleTime; } set { pistonCycleTime = value; } }
    [SerializeField]
    [Tooltip("Time for one full piston cycle")]
    private float pistonCycleTime = 0.04f;
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
    #endregion

    #region Private Variables
    private Vector2 balanceInput;
    // arrange with component setter singleton later
    private bool sceneActive = true;
    #endregion

    void Awake()
    {
        #region Component handling
        if (piston == null)
        {
            piston = GameObject.Find("Piston");
            if (piston == null)
            {
                Debug.LogError("No piston object attatched or found!");
            }
            else
            {
                if (pistonRB == null)
                {
                    if (!piston.TryGetComponent(out pistonRB))
                    {
                        Debug.LogError("No rigidbody attached found on the piston object!");
                    }
                }
            }
        }

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
        #endregion
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        if(!sceneActive)
        {
            yield return new WaitUntil(() => sceneActive);
        }
        StartCoroutine(CyclePiston());
        StartCoroutine(PlayerMovement());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        
    }

    public bool ToggleMove(){ return CanMove = !CanMove; }

    private IEnumerator CyclePiston()
    {
        Vector3 targetPosition = Vector3.down * pistonMoveDistance;
        while(sceneActive)
        {
            if (!PistonActive) { yield return new WaitUntil(()=>PistonActive); }
            MovePiston(targetPosition);

            targetPosition *= -1;
            yield return new WaitForSeconds(pistonCycleTime / 2);
        }
    }

    private void MovePiston(Vector3 targetPosition)
    {
        transform.Translate(targetPosition);
    }

    private void MovePlayer()
    {
        playerRB.AddRelativeForce(balanceInput * balanceMultiplier);
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
}
