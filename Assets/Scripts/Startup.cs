using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    public string Client;
    public string Server;

    private void Start()
    {
        if (System.Environment.GetCommandLineArgs().Any(arg => arg == "-port"))
        {
            Debug.Log("Starting server");
            SceneManager.LoadScene(Server);
        }
    }

    public void StartFind()
    {
        Debug.Log("Starting client");
        SceneManager.LoadScene(Client);
    }
}