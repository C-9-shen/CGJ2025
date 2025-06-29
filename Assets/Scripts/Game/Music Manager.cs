using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("音乐播放设置")]
    [SerializeField] public List<AudioClip> musicPlaylist = new List<AudioClip>(); // 音乐播放列表
    [SerializeField] private int loopCount = 1; // 循环次数 (0表示无限循环)
    [SerializeField] private bool playOnStart = true; // 是否在开始时自动播放
    [SerializeField] private bool shuffleMode = false; // 是否随机播放
    
    [Header("音量设置")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 1.0f; // 音乐音量
    [SerializeField] private bool fadeInOut = true; // 是否启用淡入淡出
    [SerializeField] private float fadeDuration = 1.0f; // 淡入淡出时长
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true; // 是否显示调试信息
    
    private AudioSource audioSource; // 音频源组件
    private int currentTrackIndex = 0; // 当前播放的音乐索引
    private int currentLoopCount = 0; // 当前已循环次数
    private bool isPlaying = false; // 是否正在播放
    private bool isPaused = false; // 是否暂停
    private Coroutine fadeCoroutine; // 淡入淡出协程
    
    // 单例模式
    private static MusicManager instance;
    public static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MusicManager>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        // 单例模式处理
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
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
        audioSource.volume = musicVolume;
    }
    
    void Start()
    {
        if (playOnStart && musicPlaylist.Count > 0)
        {
            PlayMusic();
        }
    }

    void Update()
    {
        // 检查当前音乐是否播放结束
        if (isPlaying && !audioSource.isPlaying && !isPaused)
        {
            OnTrackFinished();
        }
        
        // 同步音量设置
        if (audioSource.volume != musicVolume)
        {
            audioSource.volume = musicVolume;
        }
    }
    
    /// <summary>
    /// 当前音乐播放结束时的处理
    /// </summary>
    private void OnTrackFinished()
    {
        if (showDebugInfo) Debug.Log($"音乐播放结束: {GetCurrentTrackName()}");
        
        // 检查是否需要播放下一首
        if (currentTrackIndex < musicPlaylist.Count - 1)
        {
            // 播放下一首
            NextTrack();
        }
        else
        {
            // 已经是最后一首，检查循环设置
            currentLoopCount++;
            
            if (loopCount == 0 || currentLoopCount < loopCount)
            {
                // 继续循环播放
                currentTrackIndex = 0;
                PlayCurrentTrack();
                if (showDebugInfo) Debug.Log($"开始第 {currentLoopCount + 1} 轮循环播放");
            }
            else
            {
                // 停止播放
                StopMusic();
                if (showDebugInfo) Debug.Log("播放列表播放完成，停止音乐");
            }
        }
    }
    
    /// <summary>
    /// 开始播放音乐
    /// </summary>
    public void PlayMusic()
    {
        if (musicPlaylist.Count == 0)
        {
            Debug.LogWarning("音乐播放列表为空！");
            return;
        }
        
        if (isPaused)
        {
            // 恢复播放
            ResumeMusic();
            return;
        }
        
        currentTrackIndex = 0;
        currentLoopCount = 0;
        PlayCurrentTrack();
        
        if (showDebugInfo) Debug.Log("开始播放音乐列表");
    }
    
    /// <summary>
    /// 播放当前索引的音乐
    /// </summary>
    private void PlayCurrentTrack()
    {
        if (musicPlaylist.Count == 0 || currentTrackIndex >= musicPlaylist.Count)
        {
            return;
        }
        
        AudioClip currentClip = musicPlaylist[currentTrackIndex];
        if (currentClip == null)
        {
            Debug.LogWarning($"音乐列表索引 {currentTrackIndex} 的音频片段为空！");
            NextTrack();
            return;
        }
        
        audioSource.clip = currentClip;
        
        if (fadeInOut && fadeDuration > 0)
        {
            StartFadeIn();
        }
        else
        {
            audioSource.volume = musicVolume;
            audioSource.Play();
        }
        
        isPlaying = true;
        isPaused = false;
        
        if (showDebugInfo) Debug.Log($"正在播放: {currentClip.name} (索引: {currentTrackIndex})");
    }
    
    /// <summary>
    /// 切换到下一首音乐
    /// </summary>
    public void NextTrack()
    {
        if (musicPlaylist.Count == 0) return;
        
        if (currentTrackIndex < musicPlaylist.Count - 1)
        {
            currentTrackIndex++;
            PlayCurrentTrack();
        }
        else
        {
            // 已经是最后一首，停止播放
            StopMusic();
            if (showDebugInfo) Debug.Log("已经是最后一首音乐，停止播放");
        }
    }
    
    /// <summary>
    /// 切换到上一首音乐
    /// </summary>
    public void PreviousTrack()
    {
        if (musicPlaylist.Count == 0) return;
        
        if (currentTrackIndex > 0)
        {
            currentTrackIndex--;
            PlayCurrentTrack();
        }
        else
        {
            // 已经是第一首，重新播放当前首
            PlayCurrentTrack();
        }
    }
    
    /// <summary>
    /// 暂停音乐
    /// </summary>
    public void PauseMusic()
    {
        if (isPlaying && !isPaused)
        {
            audioSource.Pause();
            isPaused = true;
            if (showDebugInfo) Debug.Log("音乐已暂停");
        }
    }
    
    /// <summary>
    /// 恢复播放
    /// </summary>
    public void ResumeMusic()
    {
        if (isPaused)
        {
            audioSource.UnPause();
            isPaused = false;
            if (showDebugInfo) Debug.Log("音乐已恢复播放");
        }
    }
    
    /// <summary>
    /// 停止音乐
    /// </summary>
    public void StopMusic()
    {
        if (fadeInOut && fadeDuration > 0 && isPlaying)
        {
            StartFadeOut(() => {
                audioSource.Stop();
                isPlaying = false;
                isPaused = false;
            });
        }
        else
        {
            audioSource.Stop();
            isPlaying = false;
            isPaused = false;
        }
        
        if (showDebugInfo) Debug.Log("音乐已停止");
    }
    
    /// <summary>
    /// 设置循环次数
    /// </summary>
    /// <param name="count">循环次数 (0表示无限循环)</param>
    public void SetLoopCount(int count)
    {
        loopCount = Mathf.Max(0, count);
        if (showDebugInfo) Debug.Log($"循环次数设置为: {(loopCount == 0 ? "无限循环" : loopCount.ToString())}");
    }
    
    /// <summary>
    /// 设置音乐音量
    /// </summary>
    /// <param name="volume">音量 (0-1)</param>
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        audioSource.volume = musicVolume;
    }
    
    /// <summary>
    /// 添加音乐到播放列表
    /// </summary>
    /// <param name="clip">音频片段</param>
    public void AddToPlaylist(AudioClip clip)
    {
        if (clip != null)
        {
            musicPlaylist.Add(clip);
            if (showDebugInfo) Debug.Log($"已添加音乐到播放列表: {clip.name}");
        }
    }
    
    /// <summary>
    /// 从播放列表移除音乐
    /// </summary>
    /// <param name="index">索引</param>
    public void RemoveFromPlaylist(int index)
    {
        if (index >= 0 && index < musicPlaylist.Count)
        {
            string clipName = musicPlaylist[index]?.name ?? "Unknown";
            musicPlaylist.RemoveAt(index);
            
            // 如果移除的是当前播放的音乐之前的音乐，需要调整索引
            if (index <= currentTrackIndex && currentTrackIndex > 0)
            {
                currentTrackIndex--;
            }
            
            if (showDebugInfo) Debug.Log($"已从播放列表移除音乐: {clipName}");
        }
    }
    
    /// <summary>
    /// 清空播放列表
    /// </summary>
    public void ClearPlaylist()
    {
        StopMusic();
        musicPlaylist.Clear();
        currentTrackIndex = 0;
        currentLoopCount = 0;
        if (showDebugInfo) Debug.Log("播放列表已清空");
    }
    
    /// <summary>
    /// 获取当前播放的音乐名称
    /// </summary>
    /// <returns>音乐名称</returns>
    public string GetCurrentTrackName()
    {
        if (musicPlaylist.Count > 0 && currentTrackIndex < musicPlaylist.Count && musicPlaylist[currentTrackIndex] != null)
        {
            return musicPlaylist[currentTrackIndex].name;
        }
        return "无音乐";
    }
    
    /// <summary>
    /// 获取当前播放状态
    /// </summary>
    /// <returns>播放状态信息</returns>
    public string GetPlaybackInfo()
    {
        return $"当前: {GetCurrentTrackName()} ({currentTrackIndex + 1}/{musicPlaylist.Count}) - 循环: {currentLoopCount + 1}/{(loopCount == 0 ? "∞" : loopCount.ToString())}";
    }
    
    /// <summary>
    /// 开始淡入
    /// </summary>
    private void StartFadeIn()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// 开始淡出
    /// </summary>
    /// <param name="onComplete">淡出完成时的回调</param>
    private void StartFadeOut(System.Action onComplete = null)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOut(onComplete));
    }
    
    /// <summary>
    /// 淡入协程
    /// </summary>
    private IEnumerator FadeIn()
    {
        audioSource.volume = 0f;
        audioSource.Play();
        
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, musicVolume, timer / fadeDuration);
            yield return null;
        }
        
        audioSource.volume = musicVolume;
    }
    
    /// <summary>
    /// 淡出协程
    /// </summary>
    /// <param name="onComplete">完成时的回调</param>
    private IEnumerator FadeOut(System.Action onComplete = null)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }
        
        audioSource.volume = 0f;
        onComplete?.Invoke();
    }
    
    // 属性访问器
    public bool IsPlaying => isPlaying && !isPaused;
    public bool IsPaused => isPaused;
    public int CurrentTrackIndex => currentTrackIndex;
    public int PlaylistCount => musicPlaylist.Count;
    public int CurrentLoopCount => currentLoopCount;
    public int LoopCount => loopCount;
}
