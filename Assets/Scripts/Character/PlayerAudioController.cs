using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioController : MonoBehaviour
{
    [Header("跑步音频")]
    public AudioClip runningSound;
    public float runningVolume = 0.5f;
    
    [Header("跳跃音频")]
    public AudioClip jumpSound;
    public float jumpVolume = 0.7f;
    
    [Header("落地音频")]
    public AudioClip landSound;
    public float landVolume = 0.7f;
    
    private AudioSource audioSource;
    private Animator animator;
    private bool wasRunning;
    private bool jumpTriggered;
    private bool landTriggered;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 获取当前动画状态信息
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 检测跑步状态
        bool isRunning = stateInfo.IsTag("Running");
        
        // 跑步音频处理
        if (isRunning)
        {
            if (!wasRunning || !audioSource.isPlaying)
            {
                PlayRunningSound();
            }
            wasRunning = true;
        }
        else
        {
            if (wasRunning)
            {
                StopRunningSound();
            }
            wasRunning = false;
        }
        
        // 跳跃音频处理（通过动画事件触发）
        if (stateInfo.IsTag("Jumping") && !jumpTriggered)
        {
            PlayJumpSound();
            jumpTriggered = true;
        }
        else if (!stateInfo.IsTag("Jumping"))
        {
            jumpTriggered = false;
        }
        
        // 落地音频处理（通过动画事件触发）
        if (stateInfo.IsTag("Landing") && !landTriggered)
        {
            PlayLandSound();
            landTriggered = true;
        }
        else if (!stateInfo.IsTag("Landing"))
        {
            landTriggered = false;
        }
    }
    
    private void PlayRunningSound()
    {
        audioSource.clip = runningSound;
        audioSource.volume = runningVolume;
        audioSource.loop = true;
        audioSource.Play();
    }
    
    private void StopRunningSound()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }
    
    // 由动画事件调用的方法
    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(jumpSound, jumpVolume);
    }
    
    // 由动画事件调用的方法
    public void PlayLandSound()
    {
        audioSource.PlayOneShot(landSound, landVolume);
    }
}