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
    public Vector3 spawnPosition3;
    public Vector3 spawnPosition4;
    private Vector3 spawnPosition;
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Get spawning point
            // See if another player exists
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            switch (playerCount)
            {
                case 0:
                    spawnPosition = spawnPosition1;
                    break;
                case 1:
                    spawnPosition = spawnPosition2;
                    break;
                case 2:
                    spawnPosition = spawnPosition3;
                    break;
                case 3:
                    spawnPosition = spawnPosition4;
                    break;
            }

            PhotonNetwork.Instantiate(MetaAvatarPrefab.name, spawnPosition, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
