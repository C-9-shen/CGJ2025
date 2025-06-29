using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraShake : MonoBehaviour
{
    [Header("抖动设置")]
    [SerializeField] private float shakeDuration = 0.5f; // 抖动持续时间
    [SerializeField] private float shakeIntensity = 1.0f; // 抖动强度
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 抖动衰减曲线
    
    [Header("撞击感设置")]
    [SerializeField] private float impactStrength = 2.0f; // 撞击瞬间的强度倍数
    [SerializeField] private float dampingRatio = 0.8f; // 阻尼比例，控制震荡衰减
    
    [Header("音效设置")]
    [SerializeField] private AudioClip shakeAudioClip; // 抖动音效
    [SerializeField] [Range(0f, 1f)] private float audioVolume = 1.0f; // 音效音量
    [SerializeField] private bool playAudioOnShake = true; // 是否在抖动时播放音效
    
    [Header("事件")]
    public UnityEvent OnShakeStart; // 抖动开始事件
    public UnityEvent OnShakeEnd; // 抖动结束事件
    
    private Vector3 originalPosition; // 相机原始位置
    private bool isShaking = false; // 是否正在抖动
    private Camera cam; // 相机组件引用
    private AudioSource audioSource; // 音频源组件
    
    // Start is called before the first frame update
    void Start()
    {
        // 获取相机组件
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // 获取或添加AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 配置AudioSource
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // 记录原始位置
        
    }

    // Update is called once per frame
    void Update()
    {
        // 按F键触发测试抖动
        if (Input.GetKeyDown(KeyCode.F))
        {
            TriggerShake();
        }
    }
    
    /// <summary>
    /// 触发相机抖动
    /// </summary>
    public void TriggerShake()
    {
        if (!isShaking)
        {
            if (cam != null)
            {
                originalPosition = cam.transform.localPosition;
            }
            
            // 播放音效
            PlayShakeAudio();
            
            StartCoroutine(ShakeCamera());
            
        }
    }
    
    /// <summary>
    /// 触发相机抖动（可自定义参数）
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <param name="intensity">强度</param>
    public void TriggerShake(float duration, float intensity)
    {
        if (!isShaking)
        {
            // 播放音效
            PlayShakeAudio();
            
            StartCoroutine(ShakeCamera(duration, intensity));
        }
    }
    
    /// <summary>
    /// 播放抖动音效
    /// </summary>
    private void PlayShakeAudio()
    {
        if (playAudioOnShake && shakeAudioClip != null && audioSource != null)
        {
            audioSource.clip = shakeAudioClip;
            audioSource.volume = audioVolume;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// 设置抖动音效
    /// </summary>
    /// <param name="audioClip">音效片段</param>
    public void SetShakeAudio(AudioClip audioClip)
    {
        shakeAudioClip = audioClip;
    }
    
    /// <summary>
    /// 设置音效音量
    /// </summary>
    /// <param name="volume">音量 (0-1)</param>
    public void SetAudioVolume(float volume)
    {
        audioVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// 设置是否播放音效
    /// </summary>
    /// <param name="play">是否播放</param>
    public void SetPlayAudioOnShake(bool play)
    {
        playAudioOnShake = play;
    }
    
    /// <summary>
    /// 相机抖动协程
    /// </summary>
    private IEnumerator ShakeCamera(float duration = -1, float intensity = -1)
    {
        isShaking = true;
        OnShakeStart?.Invoke(); // 触发抖动开始事件
        
        // 使用默认值或传入的值
        float currentDuration = duration > 0 ? duration : shakeDuration;
        float currentIntensity = intensity > 0 ? intensity : shakeIntensity;
        
        float timer = 0f;
        Vector3 lastOffset = Vector3.zero;
        
        while (timer < currentDuration)
        {
            timer += Time.deltaTime;
            
            // 计算当前时间在抖动周期中的进度
            float progress = timer / currentDuration;
            
            // 使用动画曲线计算衰减
            float curveValue = shakeCurve.Evaluate(progress);
            
            // 添加撞击感：在开始时有一个强烈的冲击
            float impactMultiplier = 1f;
            if (progress < 0.1f) // 前10%的时间内加强撞击感
            {
                impactMultiplier = Mathf.Lerp(impactStrength, 1f, progress / 0.1f);
            }
            
            // 生成随机偏移
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f
            ) * currentIntensity * curveValue * impactMultiplier;
            
            // 添加阻尼效果，让抖动更自然
            randomOffset = Vector3.Lerp(lastOffset, randomOffset, dampingRatio);
            lastOffset = randomOffset;
            
            // 应用偏移
            if (cam != null)
            {
                cam.transform.localPosition = originalPosition + randomOffset;
            }
            
            yield return null;
        }
        
        // 抖动结束，恢复原始位置
        if (cam != null)
        {
            cam.transform.localPosition = originalPosition;
        }
        
        isShaking = false;
        OnShakeEnd?.Invoke(); // 触发抖动结束事件
    }
    
    /// <summary>
    /// 重置相机位置
    /// </summary>
    public void ResetPosition()
    {
        if (cam != null)
        {
            cam.transform.localPosition = originalPosition;
        }
        isShaking = false;
    }
    
    /// <summary>
    /// 停止抖动
    /// </summary>
    public void StopShake()
    {
        StopAllCoroutines();
        ResetPosition();
        OnShakeEnd?.Invoke(); // 强制停止时也触发结束事件
    }
}
