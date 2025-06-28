using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollWithMove : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform targetObject; // 要监测移动的物体
    public Transform backgroundObject; // 背景物体
    
    [Header("Scroll Settings")]
    public bool invertScrollDirection = true; // 是否反向滚动背景
    public Vector2 scrollMultiplier = Vector2.one; // 滚动倍数
    
    [Header("Boundary Settings")]
    public bool enableBoundary = true; // 是否启用边界
    public Vector2 boundaryMin = new Vector2(-50f, -50f); // 背景滚动边界最小值
    public Vector2 boundaryMax = new Vector2(50f, 50f);   // 背景滚动边界最大值
    
    [Header("Buffer Settings")]
    public float maxBufferDistance = 2f; // 最大缓冲距离
    public float velocityToDistanceMultiplier = 0.5f; // 速度到距离的转换倍数
    public float minBufferDistance = 0.1f; // 最小缓冲距离
    public float returnSpeed = 5f; // 回归速度
    public float velocityThreshold = 0.1f; // 速度阈值，低于此值开始回归
    public float smoothTime = 0.3f; // 平滑时间
    
    private Vector3 lastPosition;
    private Vector3 initialBackgroundPosition;
    private Vector3 velocity;
    private Vector3 smoothVelocity;
    private bool isReturning = false;
    private float currentAllowedDistance = 0f;
    private bool isAtBoundaryX = false; // X轴边界状态
    private bool isAtBoundaryY = false; // Y轴边界状态

    void Start()
    {
        if (targetObject == null)
            targetObject = transform;
            
        lastPosition = targetObject.localPosition;
        
        if (backgroundObject != null)
            initialBackgroundPosition = backgroundObject.localPosition;
    }

    void Update()
    {
        if (targetObject == null) return;
        
        // 计算物体的移动量和速度
        Vector3 currentPosition = targetObject.localPosition;
        Vector3 deltaMovement = currentPosition - lastPosition;
        velocity = deltaMovement / Time.deltaTime;
        
        // 根据速度计算目标缓冲距离，并平滑过渡
        float currentSpeed = velocity.magnitude;
        float targetBufferDistance = Mathf.Clamp(
            currentSpeed * velocityToDistanceMultiplier, 
            minBufferDistance, 
            maxBufferDistance
        );
        
        // 在边界状态下不更新缓冲距离
        if (!isAtBoundaryX && !isAtBoundaryY)
        {
            currentAllowedDistance = Mathf.Lerp(currentAllowedDistance, targetBufferDistance, Time.deltaTime * 5f);
        }
        
        // 检查是否在移动
        bool isMoving = currentSpeed > velocityThreshold;
        
        if (isMoving)
        {
            isReturning = false;
            
            // 如果有移动，处理背景滚动
            if (deltaMovement.magnitude > 0.001f)
            {
                Vector3 backgroundDelta = Vector3.zero;
                Vector3 newPosition = currentPosition;
                
                // 分别处理X和Y方向
                HandleAxisMovement(currentPosition, deltaMovement, ref newPosition, ref backgroundDelta);
                
                // 应用物体位置
                targetObject.localPosition = newPosition;
                
                // 移动背景
                if (backgroundObject != null && backgroundDelta.magnitude > 0.001f)
                {
                    if (invertScrollDirection)
                        backgroundDelta = -backgroundDelta;
                        
                    backgroundObject.localPosition += backgroundDelta;
                }
            }
        }
        else if (!isReturning && currentPosition.magnitude > 0.01f && !isAtBoundaryX && !isAtBoundaryY)
        {
            // 只有在非边界状态下才开始回归
            isReturning = true;
        }
        
        // 平滑回归到原点（仅在非边界状态）
        if (isReturning && !isAtBoundaryX && !isAtBoundaryY)
        {
            Vector3 targetPos = Vector3.zero;
            Vector3 newPosition = Vector3.SmoothDamp(currentPosition, targetPos, ref smoothVelocity, smoothTime);
            targetObject.localPosition = newPosition;
            
            // 同时让允许距离也回归到最小值
            currentAllowedDistance = Mathf.Lerp(currentAllowedDistance, minBufferDistance, Time.deltaTime * 3f);
            
            // 检查是否已经回到原点
            if (newPosition.magnitude < 0.01f)
            {
                targetObject.localPosition = Vector3.zero;
                isReturning = false;
                smoothVelocity = Vector3.zero;
                currentAllowedDistance = minBufferDistance;
            }
        }
        
        // 如果在边界状态下停止回归
        if ((isAtBoundaryX || isAtBoundaryY) && isReturning)
        {
            isReturning = false;
            smoothVelocity = Vector3.zero;
        }
        
        lastPosition = targetObject.localPosition;
    }
    
    private void HandleAxisMovement(Vector3 currentPosition, Vector3 deltaMovement, ref Vector3 newPosition, ref Vector3 backgroundDelta)
    {
        Vector3 processedMovement = Vector3.zero;
        bool wasAtBoundaryX = isAtBoundaryX;
        bool wasAtBoundaryY = isAtBoundaryY;
        
        // 处理X轴
        processedMovement.x = HandleSingleAxis(currentPosition.x, deltaMovement.x, ref isAtBoundaryX, true);
        // 如果从边界状态恢复到滚动状态，需要重新计算背景移动
        if (wasAtBoundaryX && !isAtBoundaryX)
        {
            // 刚从边界状态恢复，物体当前位置超出缓冲范围的部分应移动到背景
            float newPosX = lastPosition.x + processedMovement.x;
            if (Mathf.Abs(newPosX) > currentAllowedDistance)
            {
                float clampedX = Mathf.Clamp(newPosX, -currentAllowedDistance, currentAllowedDistance);
                float excessX = newPosX - clampedX;
                processedMovement.x = clampedX - lastPosition.x;
                backgroundDelta.x = excessX * scrollMultiplier.x;
            }
        }
        else if (!isAtBoundaryX && Mathf.Abs(processedMovement.x) < Mathf.Abs(deltaMovement.x))
        {
            backgroundDelta.x = (deltaMovement.x - processedMovement.x) * scrollMultiplier.x;
        }
        
        // 处理Y轴
        processedMovement.y = HandleSingleAxis(currentPosition.y, deltaMovement.y, ref isAtBoundaryY, false);
        // 如果从边界状态恢复到滚动状态，需要重新计算背景移动
        if (wasAtBoundaryY && !isAtBoundaryY)
        {
            // 刚从边界状态恢复，物体当前位置超出缓冲范围的部分应移动到背景
            float newPosY = lastPosition.y + processedMovement.y;
            if (Mathf.Abs(newPosY) > currentAllowedDistance)
            {
                float clampedY = Mathf.Clamp(newPosY, -currentAllowedDistance, currentAllowedDistance);
                float excessY = newPosY - clampedY;
                processedMovement.y = clampedY - lastPosition.y;
                backgroundDelta.y = excessY * scrollMultiplier.y;
            }
        }
        else if (!isAtBoundaryY && Mathf.Abs(processedMovement.y) < Mathf.Abs(deltaMovement.y))
        {
            backgroundDelta.y = (deltaMovement.y - processedMovement.y) * scrollMultiplier.y;
        }
        
        processedMovement.z = deltaMovement.z;
        newPosition = lastPosition + processedMovement;
    }
    
    private float HandleSingleAxis(float currentPos, float deltaMove, ref bool isAtBoundaryAxis, bool isXAxis)
    {
        if (isAtBoundaryAxis)
        {
            // 边界状态：完全自由移动
            // 检查是否可以重新开始滚动
            if (CanResumeScrollingAxis(currentPos, deltaMove, isXAxis))
            {
                isAtBoundaryAxis = false;
                // 恢复滚动状态，物体正常移动，不做限制
                return deltaMove;
            }
            
            return deltaMove; // 完全自由移动
        }
        else
        {
            // 正常滚动状态
            float newPos = currentPos + deltaMove;
            
            if (Mathf.Abs(newPos) <= currentAllowedDistance)
            {
                return deltaMove; // 在缓冲范围内
            }
            else
            {
                // 超出缓冲范围
                float clampedLast = Mathf.Clamp(currentPos, -currentAllowedDistance, currentAllowedDistance);
                float clampedNew = Mathf.Clamp(newPos, -currentAllowedDistance, currentAllowedDistance);
                float allowedMove = clampedNew - clampedLast;
                float excessMove = deltaMove - allowedMove;
                
                // 检查是否到达边界
                if (enableBoundary && IsAtScrollBoundaryAxis(excessMove, isXAxis))
                {
                    isAtBoundaryAxis = true;
                    return deltaMove; // 切换到边界模式，允许完全移动
                }
                
                return allowedMove; // 限制在缓冲范围
            }
        }
    }
    
    private bool CanResumeScrollingAxis(float currentPos, float deltaMove, bool isXAxis)
    {
        if (!backgroundObject) return true;
        
        Vector3 currentBackgroundPos = backgroundObject.localPosition;
        float moveThreshold = 0.01f; // 降低移动阈值
        
        // 使用物体与零点的距离来判断是否可以恢复滚动
        float distanceFromZero = Mathf.Abs(currentPos);
        float resumeThreshold = currentAllowedDistance * 2f; // 增大恢复范围
        
        // 检查物体是否在可恢复范围内
        bool inResumeRange = distanceFromZero <= resumeThreshold;
        
        // 检查移动方向和背景边界状态
        bool movingInCorrectDirection = false;
        bool atCorrectBoundary = false;
        
        if (isXAxis)
        {
            // 检查X轴边界
            bool atLeftBoundary = currentBackgroundPos.x <= boundaryMin.x + 0.5f;
            bool atRightBoundary = currentBackgroundPos.x >= boundaryMax.x - 0.5f;
            
            if (atLeftBoundary && deltaMove < -moveThreshold)
            {
                // 背景在左边界，物体向右移动
                movingInCorrectDirection = true;
                atCorrectBoundary = true;
            }
            else if (atRightBoundary && deltaMove > moveThreshold)
            {
                // 背景在右边界，物体向左移动
                movingInCorrectDirection = true;
                atCorrectBoundary = true;
            }
        }
        else
        {
            // 检查Y轴边界
            bool atBottomBoundary = currentBackgroundPos.y <= boundaryMin.y + 0.5f;
            bool atTopBoundary = currentBackgroundPos.y >= boundaryMax.y - 0.5f;
            
            if (atBottomBoundary && deltaMove < -moveThreshold)
            {
                // 背景在下边界，物体向上移动
                movingInCorrectDirection = true;
                atCorrectBoundary = true;
            }
            else if (atTopBoundary && deltaMove > moveThreshold)
            {
                // 背景在上边界，物体向下移动
                movingInCorrectDirection = true;
                atCorrectBoundary = true;
            }
        }


        
        return inResumeRange && movingInCorrectDirection && atCorrectBoundary;
    }
    
    private bool IsAtScrollBoundaryAxis(float movement, bool isXAxis)
    {
        if (!backgroundObject) return false;
        
        Vector3 newBackgroundPos = backgroundObject.localPosition;
        
        if (isXAxis)
        {
            if (invertScrollDirection)
                newBackgroundPos.x -= movement * scrollMultiplier.x;
            else
                newBackgroundPos.x += movement * scrollMultiplier.x;
                
            return newBackgroundPos.x < boundaryMin.x || newBackgroundPos.x > boundaryMax.x;
        }
        else
        {
            if (invertScrollDirection)
                newBackgroundPos.y -= movement * scrollMultiplier.y;
            else
                newBackgroundPos.y += movement * scrollMultiplier.y;
                
            return newBackgroundPos.y < boundaryMin.y || newBackgroundPos.y > boundaryMax.y;
        }
    }
    
    [ContextMenu("Reset Background Position")]
    public void ResetBackgroundPosition()
    {
        if (backgroundObject != null)
            backgroundObject.localPosition = initialBackgroundPosition;
    }
}





