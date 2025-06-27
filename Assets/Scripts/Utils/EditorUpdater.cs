using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class EditorUpdater : MonoBehaviour
{
    [Tooltip("在编辑器中实时更新的事件")]
    public UnityEvent onEditorUpdate;

    [Tooltip("是否启用在编辑器中的实时更新")]
    public bool enableEditorUpdate = true;

    private void Awake()
    {
#if UNITY_EDITOR
        // 在编辑器模式下注册更新事件
        if (!Application.isPlaying)
        {
            EditorApplication.update += OnEditorUpdate;
        }
#endif
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        // 确保在组件启用时重新注册
        if (!Application.isPlaying)
        {
            EditorApplication.update += OnEditorUpdate;
        }
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        // 在组件禁用时取消注册
        if (!Application.isPlaying)
        {
            EditorApplication.update -= OnEditorUpdate;
        }
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        // 确保在销毁时取消注册
        if (!Application.isPlaying)
        {
            EditorApplication.update -= OnEditorUpdate;
        }
#endif
    }

#if UNITY_EDITOR
    private void OnEditorUpdate()
    {
        // 仅在启用时触发事件
        if (enableEditorUpdate && onEditorUpdate != null)
        {
            onEditorUpdate.Invoke();
        }
    }
#endif

    // 运行时的 Update 方法
    private void Update()
    {
        // 运行时也可以使用相同的事件
        if (Application.isPlaying && enableEditorUpdate && onEditorUpdate != null)
        {
            onEditorUpdate.Invoke();
        }
    }
}
