using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ClipProxy : MaskableGraphic
{
    //手动设定转发给哪几个MaskableGraphic, 如果不指定将获取所有子孙上的MaskableGraphic组件
    [SerializeField] private MaskableGraphic[] _manualMaskables;
    private List<MaskableGraphic> _maskableList;

    public class ClippableEvent : UnityEvent<Rect, bool> {}

    private ClippableEvent _onCull = new ClippableEvent();
    public ClippableEvent onCull
    {
        get { return _onCull; }
        set { _onCull = value; }
    }

    private ClippableEvent _onSetClipRect = new ClippableEvent();
    public ClippableEvent onSetClipRect
    {
        get { return _onSetClipRect; }
        set { _onSetClipRect = value; }
    }

    #region Empty4Raycast的功能

    protected ClipProxy()
    {
        useLegacyMeshGeneration = false;
    }
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
    }

    #endregion


    #if UNITY_EDITOR
        [ContextMenu("Refresh Maskable List")]
    #endif
        public void RefreshMaskableList()
        {
            if (null == _manualMaskables || 0 == _manualMaskables.Length)
            {
                _maskableList = new List<MaskableGraphic>();
                GetComponentsInChildren(false, _maskableList);
            }
            else
            {
                _maskableList = new List<MaskableGraphic>(_manualMaskables);
            }
        }


        protected override void Awake()
        {
            base.Awake();
            if (GetComponent<CanvasRenderer>() == null)
                gameObject.AddComponent<CanvasRenderer>();


                RefreshMaskableList();

        }

    public override void Cull(Rect clipRect, bool validRect)
    {
        base.Cull(clipRect, validRect);
        if (null != _maskableList)
        {
            for (var j = 0; j < _maskableList.Count; ++j)
            {
                var maskable = _maskableList[j];
                if (null != maskable & maskable != this)
                    maskable.Cull(clipRect, validRect);
            }
        }
        _onCull.Invoke(clipRect, validRect);
    }

    public override void SetClipRect(Rect value, bool validRect)
    {
        base.SetClipRect(value, validRect);
        if (null != _maskableList)
        {
            for (var j = 0; j < _maskableList.Count; ++j)
            {
                var maskable = _maskableList[j];
                if (null != maskable && maskable != this)
                    maskable.SetClipRect(value, validRect);
            }
        }

        _onSetClipRect.Invoke(value, validRect);
    }

    public void RefreshMask()
    {

            RefreshMaskableList();

        transform.parent.GetComponent<RectMask2D>().UpdateClipSoftness();
    }
}