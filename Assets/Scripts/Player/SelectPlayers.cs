 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
public class SelectPlayers : MonoBehaviour
{
    [SerializeField]
    private Transform leftBound;
    [SerializeField]
    private Transform rightBound;
    public static SelectPlayers Instance { get; private set; }

    private void Awake()
    {
        PlayerManagerData.Instance.InputManager.onPlayerJoined += AddPlayer;
        PlayerManagerData.Instance.InputManager.onPlayerJoined += RemovePlayer;
        // Singleton handling
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            PlayerManagerData.Instance.InputManager.EnableJoining();
        }
    }

    private void OnDisable()
    {
        PlayerManagerData.Instance.InputManager.onPlayerJoined -= AddPlayer;
        PlayerManagerData.Instance.InputManager.onPlayerJoined -= RemovePlayer;
    }

    public void AddPlayer(PlayerInput playerInput)
    {
        AlignPlayers();
    }
    
    public void RemovePlayer(PlayerInput playerInput)
    {
        AlignPlayers();
    }

    private void AlignPlayers()
    {
        List<Vector3> newPositions = PlayerManagerData.Instance.GenerateSpawnPoints(leftBound.position, rightBound.position);
        foreach(KeyValuePair<PlayerInput, int> player in PlayerManagerData.Instance.Players)
        {
            PlayerComponents components = player.Key.gameObject.GetComponent<PlayerComponents>();
            components.JackhammerRB.constraints = RigidbodyConstraints.FreezeRotation;
            components.JackhammerRB.isKinematic = false;
            components.JackhammerRB.useGravity = true;
            components.JackhammerRB.transform.position = transform.position + newPositions[player.Value];
            LayerMask viewLayers = new();
            viewLayers |= (1 << 0);
            viewLayers |= (1 << 1);
            viewLayers |= (1 << 2);
            viewLayers |= (1 << 4);
            viewLayers |= (1 << 5);
            viewLayers |= (1 << 6);
            viewLayers |= (1 << 7);
            viewLayers |= (1 << 8);
            viewLayers |= (1 << 9);
            viewLayers |= (1 << 10 + player.Value);
            components.PlayerCamera.cullingMask = viewLayers;
            components.VirtualCamera.GetComponent<CinemachineInputProvider>().PlayerIndex = player.Value;
            components.VirtualCamera.layer = player.Value + 10;
        }
    }
}