using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Oculus.Avatar2;

public class AvatarsMultiplayerSync : MonoBehaviour, IPunObservable
{
    private PhotonView m_PhotonView;
    public GameObject localAvatarObject;
    public GameObject remoteAvatarObject;
    private SampleAvatarEntity localAvatar;
    private SampleAvatarEntity remoteAvatar;
    private Transform localAvatarTransform;
    private Transform remoteAvatarTransform;

    private byte[] streamData;

    bool m_firstTake = false;

    public void Awake()
    {
        m_PhotonView = GetComponent<PhotonView>();
        localAvatar = localAvatarObject.GetComponent<SampleAvatarEntity>();
        remoteAvatar = remoteAvatarObject.GetComponent<SampleAvatarEntity>();
        localAvatarTransform = localAvatarObject.GetComponent<Transform>();
        remoteAvatarTransform = remoteAvatarObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    public void Update()
    {
        if (!this.m_PhotonView.IsMine)
        {
            
            //remoteAvatar.ApplyStreamData(this.streamData);
            this.remoteAvatarTransform.position = Vector3.Lerp(this.localAvatarTransform.position, this.remoteAvatarTransform.position, 0.2f);
        }

    }

    void OnEnable()
    {
        m_firstTake = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            byte[] data = localAvatar.RecordStreamData(SampleAvatarEntity.StreamLOD.Medium);
            stream.SendNext(data);
            stream.SendNext(this.localAvatarTransform.position);
        }
        else
        {
            this.streamData = (byte[])stream.ReceiveNext();
            this.localAvatarTransform.position = (Vector3)stream.ReceiveNext();
        }
    }
}

