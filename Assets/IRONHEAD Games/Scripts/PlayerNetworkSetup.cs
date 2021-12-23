using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
public class PlayerNetworkSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalXRRigGameobject;
    public GameObject MainAvatarGameobject;

    public GameObject AvatarHeadGameobject;
    public GameObject AvatarBodyGameobject;


    public GameObject[] AvatarModelPrefabs;

    public TextMeshProUGUI PlayerName_Text;

    // Start is called before the first frame update
    void Start()
    {

        if (photonView.IsMine)
        {
            //The player is local
            LocalXRRigGameobject.SetActive(true);

            SetLayerRecursively(AvatarHeadGameobject, 6);
            SetLayerRecursively(AvatarBodyGameobject, 6);
        }
        else
        {
            //The player is remote
            LocalXRRigGameobject.SetActive(false);
            SetLayerRecursively(AvatarHeadGameobject, 0);
            SetLayerRecursively(AvatarBodyGameobject, 0);
        }

    }        
    // Update is called once per frame
    void Update()
    {

    }


    void SetLayerRecursively(GameObject go, int layerNumber)
    {
        if (go == null) return;
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }

}
