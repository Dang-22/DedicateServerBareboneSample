using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    public string Client;
    public string Server;

    public int target = 60;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }
    void Start()
    {
        if (System.Environment.GetCommandLineArgs().Any(arg => arg == "-port"))
        {
            Debug.Log("Starting server");
            SceneManager.LoadScene(Server);
        }
        
    }
}