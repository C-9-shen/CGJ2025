using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using JetBrains.Annotations;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static int Disobey = 0;

    public static bool DirectlyFallen = false;

    public List<GMEvent> NotFallEvents = new List<GMEvent>();

    public List<GMEvent> DisobeyFinalEvents = new List<GMEvent>();

    // MessageBox 标志位常量
    private const uint MB_OK = 0x00000000;
    private const uint MB_TOPMOST = 0x00040000;
    private const uint MB_SYSTEMMODAL = 0x00001000;

    // 全局输入控制状态
    public static bool InputEnabled { get; private set; } = true;
    
    /// <summary>
    /// 设置全局输入启用状态
    /// </summary>
    /// <param name="enabled">是否启用输入</param>
    public static void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;
        Debug.Log($"全局输入状态设置为: {enabled}");
    }

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

    public void DisobeyIncrease()
    {
        Disobey++;
        Debug.Log("Disobey = " + Disobey);
    }

    public void SetDirectlyFallen(bool val)
    {
        DirectlyFallen = val;
    }

    public void DirectlyFallenEvent()
    {
        if (!DirectlyFallen)
        {
            if (NotFallEvents.Count > 0)
            {
                foreach (var item in NotFallEvents)
                {
                    if (item.EventIndex != -1)
                    {
                        if (item.TriggerOnce == false || item.Triggered == false)
                        {
                            item.Triggered = true;
                            item.TriggerEvent.Invoke();
                        }
                    }
                }
            }
        }
    }

    public void DisobeyFinalEvent()
    {
        if (Disobey >= 3)
        {
            foreach (var item in DisobeyFinalEvents)
            {
                if (item.EventIndex != -1)
                {
                    if (item.TriggerOnce == false || item.Triggered == false)
                    {
                        item.Triggered = true;
                        item.TriggerEvent.Invoke();
                    }
                }
            }
        }
    }

    public void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}

[Serializable]
public class GMEvent
{
    public int EventIndex = -1;
    public bool TriggerOnce = false;
    public bool Triggered = false;
    public UnityEvent TriggerEvent = new UnityEvent();
}