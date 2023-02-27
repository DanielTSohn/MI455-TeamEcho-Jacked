using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KillZVolume : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The UI game object to spawn")]
    [SerializeField] private GameObject UIWinScreen;
    [Tooltip("Default win message, will be overriden from C# script")]
    [SerializeField] private string winMessage = "You win!";
    [Tooltip("The textmesh to print the winstring to")]
    [SerializeField] private TextMeshProUGUI tm;
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
        //tm = tmObject.GetComponent<TextMeshPro>();
        tm.text = winMessage;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player) { RemovePlayer(player); }
        
    }

    public void RemovePlayer(PlayerMovement player)
    {
        players.Remove(player);
        // When only one player remains, print win screen
        if (players.Count == 1)
        {
            PlayerInput finalPlayer = GameObject.FindObjectOfType<PlayerInput>();
            onGameOver.TriggerEvent(finalPlayer);
        }
    }
}
