using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Oculus.Avatar2;

public class AvatarsMultiplayerSync : MonoBehaviour, IPunObservable
{
    private PhotonView m_PhotonView;

    public OvrAvatarEntity localAvatar;
    public OvrAvatarEntity remoteAvatar;

    private byte[] streamData;

    public void Awake()
    {
        m_PhotonView = GetComponent<PhotonView>();

    }

    // Update is called once per frame
    public void Update()
    {
        if (!this.m_PhotonView.IsMine)
        {
            remoteAvatar.ApplyStreamData(this.streamData);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            byte[] data = localAvatar.RecordStreamData(OvrAvatarEntity.StreamLOD.Medium);
            stream.SendNext(data);
        }
        else
        {
            this.streamData = (byte[])stream.ReceiveNext();
        }
    }
}

