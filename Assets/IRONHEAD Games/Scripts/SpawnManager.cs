using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class SpawnManager : MonoBehaviour
{

    [SerializeField]
    GameObject MetaAvatarPrefab;

    [SerializeField]
    GameObject GenericVRPlayerPrefab;

    public Vector3 spawnPosition1;
    public Vector3 spawnPosition2;
    private Vector3 spawnPosition;
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Get spawning point
            // See if another player exists
            if (GameObject.Find("Network Avatar(Clone)"))
            {
                spawnPosition = spawnPosition2;
                
            }
            else spawnPosition = spawnPosition1;

            PhotonNetwork.Instantiate(MetaAvatarPrefab.name, spawnPosition, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
