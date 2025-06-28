using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    // MessageBox 标志位常量
    private const uint MB_OK = 0x00000000;
    private const uint MB_TOPMOST = 0x00040000;
    private const uint MB_SYSTEMMODAL = 0x00001000;

    // 全局输入控制状态
    public static bool InputEnabled { get; private set; } = true;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ExitGame()
    {
        // 立即禁用全局输入
        InputEnabled = false;
        
        // 使用 MB_TOPMOST | MB_SYSTEMMODAL 确保弹窗显示在最前面
        MessageBox(new IntPtr(0), "Demo is over, thanks for playing", "Game exit", 
                   MB_OK | MB_TOPMOST | MB_SYSTEMMODAL);

        // 启动协程，在短暂延迟后退出游戏
        StartCoroutine(ExitGameCoroutine());
    }

    private IEnumerator ExitGameCoroutine()
    {
        // 等待2秒让玩家看到消息
        yield return new WaitForSeconds(2f);
        
        // 强制退出游戏
        Application.Quit();
        
        // 在编辑器中测试时使用这个
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
