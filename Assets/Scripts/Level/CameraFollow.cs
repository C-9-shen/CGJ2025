using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // 目标物体
    public List<GameObject> followObjects = new List<GameObject>(); // 跟随的GameObject列表
    
    [Header("Follow Settings")]
    public Vector3 offset = Vector3.zero; // 跟随偏移量
    public float smoothTime = 0.3f; // 平滑时间
    public bool followX = true; // 是否在X轴跟随
    public bool followY = true; // 是否在Y轴跟随
    public bool followZ = false; // 是否在Z轴跟随
    private bool _pauseFollow = false; // 是否暂停跟随
    public bool pauseFollow
    {
        get => _pauseFollow;
        set {_pauseFollow = value;}
    }
    
    [Header("Buffer Settings")]
    public float bufferDistance = 2f; // 缓冲距离
    public bool useBuffer = true; // 是否使用缓冲
    
    [Header("Boundary Settings")]
    public bool useBoundary = true; // 是否使用边界限制
    public Vector2 boundaryMin = new Vector2(-10f, -10f); // 边界最小值
    public Vector2 boundaryMax = new Vector2(10f, 10f); // 边界最大值
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 lastTargetPosition;
    private Vector3 desiredPosition;
    private bool isInitialized = false;
    public Vector3 pauseOffset = Vector3.zero; // 暂停时的偏移量
    public Vector3 tempPauseOffset = Vector3.zero; // 临时暂停偏移量
    public Vector3 pauseBasePosition = Vector3.zero; // 暂停时的基准位置
    private bool wasPaused = false; // 上一帧是否处于暂停状态

    void Start()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
            desiredPosition = CalculateDesiredPosition();
            isInitialized = true;
        }
    }

    void LateUpdate()
    {
        if (target == null || followObjects.Count == 0) return;
        
        if (!isInitialized)
        {
            lastTargetPosition = target.position;
            desiredPosition = CalculateDesiredPosition();
            isInitialized = true;
        }
        
        // 检查暂停状态变化
        if (pauseFollow && !wasPaused)
        {
            // 刚进入暂停状态，记录基准位置
            if (followObjects.Count > 0)
            {
                pauseBasePosition = transform.position;
            }
        }

        if(!pauseFollow && wasPaused){
            pauseOffset += tempPauseOffset; // 恢复暂停时的偏移量
            tempPauseOffset = Vector3.zero; // 清除临时偏移量
        }
        
        // 如果暂停跟随，计算当前位置相对于基准位置的偏移
        if (pauseFollow)
        {
            if (followObjects.Count > 0)
            {
                Vector3 currentFollowPos = transform.position;
                tempPauseOffset = -currentFollowPos + pauseBasePosition;
            }
            wasPaused = true;
            return;
        }
        
        wasPaused = false;
        
        Vector3 currentTargetPosition = target.position;
        Vector3 targetMovement = currentTargetPosition - lastTargetPosition;
        
        // 计算期望位置，包含暂停时的偏移量
        Vector3 newDesiredPosition = desiredPosition;
        
        if (useBuffer)
        {
            // 使用缓冲距离
            Vector3 currentFollowPosition = followObjects[0].transform.position;
            Vector3 targetWithOffset = currentTargetPosition + offset + pauseOffset;
            Vector3 distanceToTarget = targetWithOffset - currentFollowPosition;
            
            // 只有当距离超过缓冲距离时才移动
            if (distanceToTarget.magnitude > bufferDistance)
            {
                Vector3 moveDirection = distanceToTarget.normalized;
                float moveDistance = distanceToTarget.magnitude - bufferDistance;
                newDesiredPosition = currentFollowPosition + moveDirection * moveDistance;
            }
            else
            {
                newDesiredPosition = currentFollowPosition;
            }
        }
        else
        {
            // 直接跟随，包含暂停偏移量
            newDesiredPosition = currentTargetPosition + offset + pauseOffset;
        }
        
        // 应用轴向限制
        if (!followX) newDesiredPosition.x = desiredPosition.x;
        if (!followY) newDesiredPosition.y = desiredPosition.y;
        if (!followZ) newDesiredPosition.z = desiredPosition.z;
        
        // 应用边界限制
        if (useBoundary)
        {
            // 使用目标的localPosition计算边界
            Vector3 targetLocalPos = target.localPosition;
            
            // 检查目标是否在边界内
            bool targetInBoundsX = targetLocalPos.x >= boundaryMin.x && targetLocalPos.x <= boundaryMax.x;
            bool targetInBoundsY = targetLocalPos.y >= boundaryMin.y && targetLocalPos.y <= boundaryMax.y;
            
            // 计算当前跟随对象位置
            Vector3 currentFollowPos = followObjects[0].transform.position;
            
            // 如果目标超出边界，停止在边界位置
            if (!targetInBoundsX)
            {
                // 目标超出X轴边界，保持跟随对象的X位置不变
                newDesiredPosition.x = currentFollowPos.x;
            }
            
            if (!targetInBoundsY)
            {
                // 目标超出Y轴边界，保持跟随对象的Y位置不变
                newDesiredPosition.y = currentFollowPos.y;
            }
        }
        
        // 平滑移动到目标位置
        Vector3 smoothedPosition = Vector3.SmoothDamp(followObjects[0].transform.position, newDesiredPosition, ref velocity, smoothTime);
        
        // 更新所有跟随对象的位置
        UpdateFollowObjects(smoothedPosition);
        
        // 更新记录
        lastTargetPosition = currentTargetPosition;
        desiredPosition = smoothedPosition;
        

    }
    
    private Vector3 CalculateDesiredPosition()
    {
        if (target == null) return transform.position;
        return target.position + offset;
    }
    
    private void UpdateFollowObjects(Vector3 newPosition)
    {
        Vector3 deltaMovement = newPosition - (followObjects.Count > 0 ? followObjects[0].transform.position : Vector3.zero);
        
        foreach (GameObject obj in followObjects)
        {
            if (obj != null)
            {
                Vector3 currentPos = obj.transform.position;
                Vector3 targetPos = currentPos + deltaMovement;
                
                // 应用轴向限制
                if (!followX) targetPos.x = currentPos.x;
                if (!followY) targetPos.y = currentPos.y;
                if (!followZ) targetPos.z = currentPos.z;
                
                obj.transform.position = targetPos;
            }
        }
    }
    
    [ContextMenu("Add Current Camera")]
    public void AddCurrentCamera()
    {
        Camera cam = Camera.main;
        if (cam != null && !followObjects.Contains(cam.gameObject))
        {
            followObjects.Add(cam.gameObject);
        }
    }
    
    [ContextMenu("Clear Follow List")]
    public void ClearFollowList()
    {
        followObjects.Clear();
    }
    
    [ContextMenu("Toggle Pause Follow")]
    public void TogglePauseFollow()
    {
        pauseFollow = !pauseFollow;
    }
    
    [ContextMenu("Reset Pause Offset")]
    public void ResetPauseOffset()
    {
        pauseOffset = Vector3.zero;
    }
    
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // 绘制缓冲区域
        if (useBuffer)
        {
            Gizmos.color = Color.yellow;
            Vector3 targetPos = target.position + offset;
            Gizmos.DrawWireSphere(targetPos, bufferDistance);
        }
        
        // 绘制边界（基于目标的父级坐标系）
        if (useBoundary)
        {
            Gizmos.color = Color.red;
            Transform targetParent = target.parent != null ? target.parent : target;
            Vector3 worldBoundaryMin = targetParent.TransformPoint(new Vector3(boundaryMin.x, boundaryMin.y, 0));
            Vector3 worldBoundaryMax = targetParent.TransformPoint(new Vector3(boundaryMax.x, boundaryMax.y, 0));
            Vector3 center = (worldBoundaryMin + worldBoundaryMax) * 0.5f;
            Vector3 size = worldBoundaryMax - worldBoundaryMin;
            size.z = 0.1f;
            Gizmos.DrawWireCube(center, size);
        }
        
        // 绘制目标位置
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(target.position + offset, 0.2f);
    }
}
