using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerComponents : MonoBehaviour
{
    public PlayerModelSwapper ModelSwapper { get { return modelSwapper; } set { modelSwapper = value; } }
    [SerializeField]
    private PlayerModelSwapper modelSwapper;

    public Camera PlayerCamera { get { return playerCamera; } set { playerCamera = value; } }
    [SerializeField]
    private Camera playerCamera;

    public GameObject VirtualCamera { get { return virtualCamera; } set { virtualCamera = value; } }
    [SerializeField]
    private GameObject virtualCamera;

    public PlayerMovement PlayerMovement { get { return playerMovement; } set { playerMovement = value; } }
    [SerializeField]
    private PlayerMovement playerMovement;

    public Rigidbody JackhammerRB { get { return jackhammerRB; } set { jackhammerRB = value; } }
    [SerializeField]
    private Rigidbody jackhammerRB;

    public GameObject PlayerRoot { get { return playerRoot; } set { playerRoot = value; } }
    [SerializeField]
    private GameObject playerRoot;

    public GameObject TopRoot { get { return topRoot; } set { topRoot = value; } }
    [SerializeField]
    private GameObject topRoot;

    public CapsuleCollider PlayerCollider { get { return playerCollider; } set { playerCollider = value; } }
    [SerializeField]
    private CapsuleCollider playerCollider;

    public PlayerInput InputManager { get { return inputManager; } set { InputManager = value; } }
    [SerializeField]
    private PlayerInput inputManager;

    public GameUIManager PlayerUI { get { return playerUI; } set { playerUI = value; } }
    [SerializeField]
    private GameUIManager playerUI;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (topRoot == null) { topRoot = gameObject; }
        if(topRoot != null)
        {
            if(!topRoot.TryGetComponent(out inputManager))
            {
                Debug.LogError("No Player Input in the top root!");
            }
        }
        else { inputManager = GetComponentInChildren<PlayerInput>(); }
        if(inputManager == null) { Debug.LogError("No Player Input found!"); }

        if (playerRoot == null)
        {
            playerRoot = topRoot.transform.Find("Player").gameObject;
            if (playerRoot == null)
            {
                Debug.LogError("No object called 'Player' under object root!");
            }
            else
            {
                if (jackhammerRB == null)
                {
                    if (!playerRoot.TryGetComponent(out jackhammerRB))
                    {
                        Debug.LogError("No rigidbody attached found on the player object!");
                    }
                }
                if (playerCollider == null)
                {
                    if (!playerRoot.TryGetComponent(out playerCollider))
                    {
                        Debug.LogError("No capsule collider attached found on the player object!");
                    }
                }
                if(modelSwapper == null)
                {
                    if (!playerRoot.TryGetComponent(out modelSwapper))
                    {
                        Debug.LogError("No model swapper script attatched to player!");
                    }
                }
                if(playerMovement == null)
                {
                    if(!playerRoot.TryGetComponent(out playerMovement))
                    {
                        Debug.LogError("No movement script attatched to player!");
                    }
                }
            }
        }
    }
}
