using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogDisplayer : MonoBehaviour
{
    [SerializeField]
    Text text;
    static string myLog = "";
    private string output;

    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
    }

    void OnGUI()
    {
        if (!Application.isEditor) //Do not display in editor ( or you can use the UNITY_EDITOR macro to also disable the rest)
        {
            text.text = myLog;
        }
    }
}
