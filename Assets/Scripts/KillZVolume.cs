using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KillZVolume : MonoBehaviour
{
    [SerializeField] private GameEvent onGameOver;

    private HashSet<PlayerMovement> players = new HashSet<PlayerMovement>();
    //private TextMeshPro tm;

    // Start is called before the first frame update
    void Start()
    {
        PlayerInput[] ps = GameObject.FindObjectsOfType<PlayerInput>();
        foreach(PlayerInput p in ps)
        {
            PlayerMovement player = p.GetComponent<PlayerMovement>();
            players.Add(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("MovementGuide"))
        {
            RemovePlayer(other.transform.parent.GetComponent<PlayerComponents>());
        }
    }

    public void RemovePlayer(PlayerComponents playerComponents)
    {
        Debug.Log("Player Removed");
        players.Remove(playerComponents.PlayerMovement);
        // When only one player remains, print win screen
        if (players.Count == 1)
        {
            Debug.Log("Player Win!");
            PlayerInput finalPlayer = GameObject.FindObjectOfType<PlayerInput>();
            onGameOver.TriggerEvent(finalPlayer);
        }
    }
}
