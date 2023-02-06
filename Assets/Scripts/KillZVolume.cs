using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillZVolume : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The UI game object to spawn")]
    [SerializeField] private GameObject UIWinScreen;
    [Tooltip("Default win message, will be overriden from C# script")]
    [SerializeField] private string winMessage = "You win!";
    [Tooltip("The textmesh to print the winstring to")]
    [SerializeField] private TextMeshProUGUI tm;

    private HashSet<JackhammerStandard> players = new HashSet<JackhammerStandard>();
    //private TextMeshPro tm;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] ps = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject p in ps)
        {
            JackhammerStandard player = p.GetComponent<JackhammerStandard>();
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
        JackhammerStandard player = other.GetComponent<JackhammerStandard>();
        if (player)
        {
            players.Remove(player);
            // When only one player remains, print win screen
            if (players.Count == 1)
            {
                JackhammerStandard finalPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<JackhammerStandard>();
                winMessage = finalPlayer.playerColorIdentifier.ToString() + " wins!";
                tm.text = winMessage;
                print(winMessage);
                UIWinScreen.SetActive(true);
            }
            
        }
    }
}
