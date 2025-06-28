using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class LimitUI_Drag : MonoBehaviour, IDragHandler, IBeginDragHandler
{

    public Canvas canvas;
    [Header("Follow Objects")]
    public List<GameObject> followObjects = new List<GameObject>(); // 跟随移动的物体列表
    
    [Header("Drag Events")]
    public UnityEvent OnStartDrag; // 开始拖拽事件
    public UnityEvent OnEndDrag; // 结束拖拽事件
    
    private RectTransform rectTransform;
    private Vector2 dragOffset;
    private bool isDragging = false;
    private Vector2 lastPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        lastPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 检测鼠标按下状态，如果正在拖拽且鼠标仍按下，继续拖拽
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                Input.mousePosition, 
                canvas.worldCamera, 
                out mousePosition);
            
            Vector2 newPosition = mousePosition + dragOffset;
            Vector2 deltaMovement = newPosition - lastPosition;
            
            rectTransform.anchoredPosition = newPosition;
            
            // 移动跟随物体
            MoveFollowObjects(deltaMovement);
            
            lastPosition = newPosition;
        }
        else if (isDragging && !Input.GetMouseButton(0))
        {
            // 鼠标松开，停止拖拽
            isDragging = false;
            OnEndDrag?.Invoke(); // 触发结束拖拽事件
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            isDragging = true;
            // 记录鼠标点击位置与物体中心的偏移量
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, 
                eventData.position, 
                canvas.worldCamera, 
                out mousePosition);
            dragOffset = rectTransform.anchoredPosition - mousePosition;
            lastPosition = rectTransform.anchoredPosition;
            
            OnStartDrag?.Invoke(); // 触发开始拖拽事件
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        // Update方法已经处理拖拽逻辑，这里可以保持为空或作为备用
    }
    
    private void MoveFollowObjects(Vector2 deltaMovement)
    {
        foreach (GameObject obj in followObjects)
        {
            if (obj != null)
            {
                // 如果是UI元素，使用RectTransform
                RectTransform objRect = obj.GetComponent<RectTransform>();
                if (objRect != null)
                {
                    objRect.anchoredPosition += deltaMovement;
                }
                else
                {
                    // 如果是普通GameObject，转换为世界坐标移动
                    Vector3 worldDelta = canvas.transform.TransformVector(deltaMovement);
                    obj.transform.position += worldDelta;
                }
            }
        }
    }
    

}
