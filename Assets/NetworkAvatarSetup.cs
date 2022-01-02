using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Oculus.Avatar2;
public class NetworkAvatarSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalAvatar;
    public GameObject RemoteLoopbackAvatar;
    //    public GameObject AvatarSdkManager;
    //    public GameObject LipSyncInput;
    //public GameObject XROrigin;

    public TextMeshProUGUI PlayerName_Text;

    //Meta Avatars Tracking Input
    private OvrAvatarEntity _localAvatarEntity;
    private OvrAvatarBodyTrackingBehavior _bodyTracking;
    private OvrAvatarLipSyncBehavior _lipSync;

    // Start is called before the first frame update
    void Start()
    {

        if (photonView.IsMine)
        {
            //The player is local
            LocalAvatar.SetActive(true);
            RemoteLoopbackAvatar.SetActive(false);
            //XROrigin.SetActive(true);


            _localAvatarEntity = LocalAvatar.GetComponent<SampleAvatarEntity>();
            _bodyTracking = GameObject.Find("AvatarSdkManagerHorizon").GetComponent<SampleInputManager>();
            if (_bodyTracking == null)
            { Debug.LogError("Body Tracking is null"); }
            _lipSync = GameObject.Find("LipSyncInput").GetComponent<OvrAvatarLipSyncContext>();
            _localAvatarEntity.SetBodyTracking(_bodyTracking);
            _localAvatarEntity.SetLipSync(_lipSync);

        }
        else
        {
            //The player is remote
            LocalAvatar.SetActive(false);
            RemoteLoopbackAvatar.SetActive(true);
            //XROrigin.SetActive(false);


        }

    }
    // Update is called once per frame
    void Update()
    {

    }


}
