 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectPlayers : MonoBehaviour
{
    [SerializeField]
    private Transform leftBound;
    [SerializeField]
    private Transform rightBound;

    private void Awake()
    {
        PlayerManagerData.Instance.InputManager.onPlayerJoined += AddPlayer;
        PlayerManagerData.Instance.InputManager.onPlayerJoined += RemovePlayer;
        PlayerManagerData.Instance.InputManager.EnableJoining();
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
            player.Key.transform.position = newPositions[player.Value];
            player.Key.gameObject.GetComponent<PlayerComponents>().JackhammerRB.constraints = RigidbodyConstraints.FreezePosition;
        }
    }
}