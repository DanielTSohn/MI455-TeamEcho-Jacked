using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField]
    private PlayerInputManager inputManager;

    [SerializeField]
    [Tooltip("The radius the players spawn around the object this is attatched to")]
    private float spawnRadius = 5;

    public static SpawnPlayers Instance { get; private set; }
    public bool SceneReady { get; private set; } = false;

    private void Awake()
    {
        // Singleton handling
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            SetupPlayers();
        }
    }

    private void SetupPlayers()
    {
        Debug.Log(PlayerManagerData.Instance.PlayerCount);
        foreach(KeyValuePair<PlayerInput, int> player in PlayerManagerData.Instance.Players)
        {
            inputManager.JoinPlayer(player.Value, player.Value, player.Key.currentControlScheme, player.Key.devices[0]);
            Debug.Log("Joined player " + player.Value);
        }
    }
}
