using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadClientButton : MonoBehaviour
{
    public void HandleButtonClick()
    {
        Debug.Log("Load client button clicked");
        
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Client");
    }
}
