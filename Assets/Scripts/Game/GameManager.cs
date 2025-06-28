using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ExitGame()
    {
        MessageBox(new IntPtr(0), "Demo is over, thanks for playing", "Game exit", 0);

        Application.Quit();
    }
}
