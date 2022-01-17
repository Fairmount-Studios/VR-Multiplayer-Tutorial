using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class MultiplayerVRSynchronization : MonoBehaviour, IPunObservable
{

    private PhotonView m_PhotonView;


    //Main VRPlayer Transform Synch
    [Header("Main VRPlayer Transform Synch")]
    public Transform generalVRPlayerTransform;

    //Position
    private float m_Distance_GeneralVRPlayer;
    private Vector3 m_Direction_GeneralVRPlayer;
    private Vector3 m_NetworkPosition_GeneralVRPlayer;
    private Vector3 m_StoredPosition_GeneralVRPlayer;

    //Rotation
    private Quaternion m_NetworkRotation_GeneralVRPlayer;
    private float m_Angle_GeneralVRPlayer;


    //Main Avatar Transform Synch
    [Header("Main Avatar Transform Synch")]
    public Transform mainAvatarTransform;



    //Position
    private float m_Distance_MainAvatar;
    private Vector3 m_Direction_MainAvatar;
    private Vector3 m_NetworkPosition_MainAvatar;
    private Vector3 m_StoredPosition_MainAvatar;

    //Rotation
    private Quaternion m_NetworkRotation_MainAvatar;
    private float m_Angle_MainAvatar;




    bool m_firstTake = false;

    public void Awake()
    {
        m_PhotonView = GetComponent<PhotonView>();

        //Main VRPlayer Synch Init
        m_StoredPosition_GeneralVRPlayer = generalVRPlayerTransform.position;
        m_NetworkPosition_GeneralVRPlayer = Vector3.zero;
        m_NetworkRotation_GeneralVRPlayer = Quaternion.identity;

        //Main Avatar Synch Init
        m_StoredPosition_MainAvatar = mainAvatarTransform.localPosition;
        m_NetworkPosition_MainAvatar = Vector3.zero;
        m_NetworkRotation_MainAvatar = Quaternion.identity;
    }

    void OnEnable()
    {
        m_firstTake = true;
    }

    public void Update()
    {
        if (!this.m_PhotonView.IsMine)
        {

            generalVRPlayerTransform.position = Vector3.MoveTowards(generalVRPlayerTransform.position, this.m_NetworkPosition_GeneralVRPlayer, this.m_Distance_GeneralVRPlayer * (1.0f / PhotonNetwork.SerializationRate));
            generalVRPlayerTransform.rotation = Quaternion.RotateTowards(generalVRPlayerTransform.rotation, this.m_NetworkRotation_GeneralVRPlayer, this.m_Angle_GeneralVRPlayer * (1.0f / PhotonNetwork.SerializationRate));

            mainAvatarTransform.localPosition = Vector3.MoveTowards(mainAvatarTransform.localPosition, this.m_NetworkPosition_MainAvatar, this.m_Distance_MainAvatar * (1.0f / PhotonNetwork.SerializationRate));
            mainAvatarTransform.localRotation = Quaternion.RotateTowards(mainAvatarTransform.localRotation, this.m_NetworkRotation_MainAvatar, this.m_Angle_MainAvatar * (1.0f / PhotonNetwork.SerializationRate));

        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //////////////////////////////////////////////////////////////////
            //General VRPlayer Transform Synch

            //Send Main Avatar position data
            this.m_Direction_GeneralVRPlayer = generalVRPlayerTransform.position - this.m_StoredPosition_GeneralVRPlayer;
            this.m_StoredPosition_GeneralVRPlayer = generalVRPlayerTransform.position;

            stream.SendNext(generalVRPlayerTransform.position);
            stream.SendNext(this.m_Direction_GeneralVRPlayer);

            //Send Main Avatar rotation data
            stream.SendNext(generalVRPlayerTransform.rotation);


            //////////////////////////////////////////////////////////////////
            //Main Avatar Transform Synch

            //Send Main Avatar position data
            this.m_Direction_MainAvatar = mainAvatarTransform.localPosition - this.m_StoredPosition_MainAvatar;
            this.m_StoredPosition_MainAvatar = mainAvatarTransform.localPosition;

            stream.SendNext(mainAvatarTransform.localPosition);
            stream.SendNext(this.m_Direction_MainAvatar);

            //Send Main Avatar rotation data
            stream.SendNext(mainAvatarTransform.localRotation);


        }
        else
        {
            ///////////////////////////////////////////////////////////////////
            //Ganeral VR Player Transform Synch

            //Get VR Player position data
            this.m_NetworkPosition_GeneralVRPlayer = (Vector3)stream.ReceiveNext();
            this.m_Direction_GeneralVRPlayer = (Vector3)stream.ReceiveNext();

            if (m_firstTake)
            {
                generalVRPlayerTransform.position = this.m_NetworkPosition_GeneralVRPlayer;
                this.m_Distance_GeneralVRPlayer = 0f;
            }
            else
            {
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                this.m_NetworkPosition_GeneralVRPlayer += this.m_Direction_GeneralVRPlayer * lag;
                this.m_Distance_GeneralVRPlayer = Vector3.Distance(generalVRPlayerTransform.position, this.m_NetworkPosition_GeneralVRPlayer);
            }

            //Get Main Avatar rotation data
            this.m_NetworkRotation_GeneralVRPlayer = (Quaternion)stream.ReceiveNext();
            if (m_firstTake)
            {
                this.m_Angle_GeneralVRPlayer = 0f;
                generalVRPlayerTransform.rotation = this.m_NetworkRotation_GeneralVRPlayer;
            }
            else
            {
                this.m_Angle_GeneralVRPlayer = Quaternion.Angle(generalVRPlayerTransform.rotation, this.m_NetworkRotation_GeneralVRPlayer);
            }

            ///////////////////////////////////////////////////////////////////
            //Main Avatar Transform Synch

            //Get Main Avatar position data
            this.m_NetworkPosition_MainAvatar = (Vector3)stream.ReceiveNext();
            this.m_Direction_MainAvatar = (Vector3)stream.ReceiveNext();

            if (m_firstTake)
            {
                mainAvatarTransform.localPosition = this.m_NetworkPosition_MainAvatar;
                this.m_Distance_MainAvatar = 0f;
            }
            else
            {
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                this.m_NetworkPosition_MainAvatar += this.m_Direction_MainAvatar * lag;
                this.m_Distance_MainAvatar = Vector3.Distance(mainAvatarTransform.localPosition, this.m_NetworkPosition_MainAvatar);
            }

            //Get Main Avatar rotation data
            this.m_NetworkRotation_MainAvatar = (Quaternion)stream.ReceiveNext();
            if (m_firstTake)
            {
                this.m_Angle_MainAvatar = 0f;
                mainAvatarTransform.rotation = this.m_NetworkRotation_MainAvatar;
            }
            else
            {
                this.m_Angle_MainAvatar = Quaternion.Angle(mainAvatarTransform.rotation, this.m_NetworkRotation_MainAvatar);
            }

            if (m_firstTake)
            {
                m_firstTake = false;
            }
        }
    }

}
