using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
public class NetworkAvatarSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalAvatar;
    public GameObject RemoteLoopbackAvatar;
    public GameObject AvatarSdkManager;
    public GameObject LipSyncInput;

    public TextMeshProUGUI PlayerName_Text;

    // Start is called before the first frame update
    void Start()
    {

        if (photonView.IsMine)
        {
            //The player is local
            LocalAvatar.SetActive(true);
            RemoteLoopbackAvatar.SetActive(false);
            AvatarSdkManager.SetActive(true);
            LipSyncInput.SetActive(true);

        }
        else
        {
            //The player is remote
            LocalAvatar.SetActive(false);
            RemoteLoopbackAvatar.SetActive(true);
            AvatarSdkManager.SetActive(false);
            LipSyncInput.SetActive(false);

        }

    }
    // Update is called once per frame
    void Update()
    {

    }


}
