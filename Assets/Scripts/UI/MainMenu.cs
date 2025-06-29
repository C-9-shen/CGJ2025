using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("主菜单设置")]
    [SerializeField] private GameObject mainMenuUI; // 主菜单UI对象
    [SerializeField] private bool hideOnAnyKey = true; // 是否按任意键隐藏
    [SerializeField] private bool disableInputWhenActive = true; // 菜单激活时是否禁用角色输入
    
    [Header("按键提示")]
    [SerializeField] private bool enableDebugLog = true; // 是否启用调试日志
    
    private bool isMainMenuActive = true; // 主菜单是否激活
    private bool wasInputEnabled; // 记录之前的输入状态
    
    void Start()
    {
        // 初始化主菜单状态
        ShowMainMenu();
    }

    void Update()
    {
        // 只有在主菜单激活且启用任意键隐藏时才检测按键
        if (isMainMenuActive && hideOnAnyKey)
        {
            CheckForAnyKeyInput();
        }
    }
    
    /// <summary>
    /// 检测任意键输入
    /// </summary>
    private void CheckForAnyKeyInput()
    {
        // 检测鼠标点击
        if (Input.anyKeyDown)
        {
            // 检查是否是被排除的按键
            if (!IsExcludedKey())
            {
                HideMainMenu();
            }
        }
    }
    
    /// <summary>
    /// 检查当前按下的键是否在排除列表中
    /// </summary>
    /// <returns>如果是排除的键返回true</returns>
    private bool IsExcludedKey()
    {
        // 检查功能键和特殊键
        if (Input.GetKeyDown(KeyCode.Escape) || 
            Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F2) || 
            Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.F4) || 
            Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.F6) || 
            Input.GetKeyDown(KeyCode.F7) || Input.GetKeyDown(KeyCode.F8) || 
            Input.GetKeyDown(KeyCode.F9) || Input.GetKeyDown(KeyCode.F10) || 
            Input.GetKeyDown(KeyCode.F11) || Input.GetKeyDown(KeyCode.F12) ||
            Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) ||
            Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) ||
            Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) ||
            Input.GetKeyDown(KeyCode.LeftCommand) || Input.GetKeyDown(KeyCode.RightCommand) ||
            Input.GetKeyDown(KeyCode.CapsLock) || Input.GetKeyDown(KeyCode.Numlock) ||
            Input.GetKeyDown(KeyCode.ScrollLock) || Input.GetKeyDown(KeyCode.Print) ||
            Input.GetKeyDown(KeyCode.Pause) || Input.GetKeyDown(KeyCode.Insert) ||
            Input.GetKeyDown(KeyCode.Home) || Input.GetKeyDown(KeyCode.End) ||
            Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 显示主菜单
    /// </summary>
    public void ShowMainMenu()
    {
        isMainMenuActive = true;
        
        // 激活主菜单UI
        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(true);
        }
        
        // 禁用角色输入
        if (disableInputWhenActive)
        {
            wasInputEnabled = GameManager.InputEnabled;
            SetInputEnabled(false);
        }
        
        if (enableDebugLog) Debug.Log("主菜单已显示，按任意键继续游戏");
    }
    
    /// <summary>
    /// 隐藏主菜单
    /// </summary>
    public void HideMainMenu()
    {
        isMainMenuActive = false;
        
        // 隐藏主菜单UI
        if (mainMenuUI != null)
        {
            mainMenuUI.SetActive(false);
        }
        
        // 恢复角色输入
        if (disableInputWhenActive)
        {
            SetInputEnabled(true);
        }
        
        if (enableDebugLog) Debug.Log("主菜单已隐藏，游戏开始");
    }
    
    /// <summary>
    /// 设置输入启用状态
    /// </summary>
    /// <param name="enabled">是否启用输入</param>
    private void SetInputEnabled(bool enabled)
    {
        // 直接调用GameManager的静态方法
        GameManager.SetInputEnabled(enabled);
    }
    
    /// <summary>
    /// 切换主菜单显示状态
    /// </summary>
    public void ToggleMainMenu()
    {
        if (isMainMenuActive)
        {
            HideMainMenu();
        }
        else
        {
            ShowMainMenu();
        }
    }
    
    /// <summary>
    /// 检查主菜单是否激活
    /// </summary>
    /// <returns>主菜单是否激活</returns>
    public bool IsMainMenuActive()
    {
        return isMainMenuActive;
    }
    
    /// <summary>
    /// 设置是否按任意键隐藏菜单
    /// </summary>
    /// <param name="enable">是否启用任意键隐藏</param>
    public void SetHideOnAnyKey(bool enable)
    {
        hideOnAnyKey = enable;
    }
    
    /// <summary>
    /// 设置菜单激活时是否禁用输入
    /// </summary>
    /// <param name="disable">是否禁用输入</param>
    public void SetDisableInputWhenActive(bool disable)
    {
        disableInputWhenActive = disable;
    }
}
