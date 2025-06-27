using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTransformer : MonoBehaviour
{
    public bool AutoStart = false; 
    public bool UseWorldSpace = false; // 是否使用世界坐标系
    public Transform tar;
    public Vector3 Opos = Vector3.zero;
    public Vector3 Orot = Vector3.zero;
    public Vector3 Oscale = Vector3.one;
    public float Oalpha = 1f;
    public float Oorder = 0f;
    public Vector3 EndPos = Vector3.zero;
    public Vector3 EndRot = Vector3.zero;
    public Vector3 EndScale = Vector3.one;
    public float EndAlpha = 1f;
    public float EndOrder = 0f;
    public List<SimpleTransform> transforms = new List<SimpleTransform>();
    public bool Transforming = false;
    public bool Inverse = false;
    public float StartTime = 0f;
    public float EndTime = -1f;
    public bool DisableT = false;
    public bool cooldown = false;
    bool init = false;
    bool delay2run = false;
    bool delay2runinv = false;

    void Awake()
    {
        bool state = gameObject.activeSelf;
        gameObject.SetActive(true);
        if (UseWorldSpace)
        {
            Opos = transform.position;
            Orot = transform.rotation.eulerAngles;
            Transform t = transform.parent;
            transform.SetParent(null);
            Oscale = transform.localScale;
            transform.SetParent(t);
        }else{
            Opos = transform.localPosition;
            Orot = transform.localRotation.eulerAngles;
            Oscale = transform.localScale;
        }
        GetEndPos();
        gameObject.SetActive(state);
        init = true;
    }

    void Start(){
        if (AutoStart) StartTrans();
    }

    public void SetTransform(Vector3 pos, Vector3 rot, Vector3 scale){
        if (UseWorldSpace)
        {
            transform.position = pos;
            transform.rotation = Quaternion.Euler(rot);
            Transform t = transform.parent;
            transform.SetParent(null);
            transform.localScale = scale;
            transform.SetParent(t);
        }else{
            transform.localPosition = pos;
            transform.localRotation = Quaternion.Euler(rot);
            transform.localScale = scale;
        }
    }

    public void GetOTransform(out Vector3 pos, out Vector3 rot, out Vector3 scale){
        if (Inverse)
        {
            pos = EndPos;
            rot = EndRot;
            scale = EndScale;
        }else{  
            pos = Opos;
            rot = Orot;
            scale = Oscale;
        }
    }

    public void StartTrans()
    {
        Inverse = false;
        if (!init){
            delay2run = true;
            delay2runinv = false;
            return;
        }
        tar = transform;
        SetTransform(Opos, Orot, Oscale);
        StartTime = Time.time;
        EndTime = -1f;
        foreach(var tr in transforms)
            EndTime = Mathf.Max(EndTime, tr.duration + tr.delay);
        foreach(var str in transforms)
            str.Init();
        Transforming = true;
    }

    public void StartInvTrans(){
        if (!init){
            delay2runinv = true;
            delay2run = false;
            return;
        }
        tar = transform;
        SetTransform(EndPos, EndRot, EndScale);
        StartTime = Time.time;
        Inverse = true;
        EndTime = -1f;
        foreach(var tr in transforms)
            EndTime = Mathf.Max(EndTime, tr.duration + tr.delay);
        foreach(var str in transforms)
            str.Init();
        Transforming = true;
    }

    public void GetEndPos(){
        EndPos = Opos;
        EndRot = Orot;
        EndScale = Oscale;
        foreach(var tr in transforms)
        {
            if (tr.TransType == TransformType.Translate) EndPos += tr.endPosition - tr.startPosition;
            else if (tr.TransType == TransformType.Rotate) EndRot += tr.endRotation - tr.startRotation;
            else if (tr.TransType == TransformType.Scale) EndScale += tr.endScale - tr.startScale;
            else if (tr.TransType == TransformType.Alpha) {Oalpha = tr.startAlpha;EndAlpha = tr.endAlpha;}
            else if (tr.TransType == TransformType.Order) {EndOrder = tr.TargetOrder;}
        }
    }

    void Update(){
        if (init == false) return;
        if (delay2run)
        {
            delay2run = false;
            StartTrans();
        }
        if (delay2runinv)
        {
            delay2runinv = false;
            StartInvTrans();
        }
        if (Transforming)
        {
            Transform tmp = new GameObject("tmp").transform;
            Vector3 tempPosition, tempRotation, tempScale;
            GetOTransform(out tempPosition, out tempRotation, out tempScale);
            if (UseWorldSpace)
            {
                tmp.position = tempPosition;
                tmp.rotation = Quaternion.Euler(tempRotation);
                tmp.localScale = tempScale;
            }else{
                tmp.localPosition = tempPosition;
                tmp.localRotation = Quaternion.Euler(tempRotation);
                tmp.localScale = tempScale;
            }
            foreach(var tr in transforms)
                tmp = Inverse ? tr.GetInvResult(tmp,Time.time-StartTime) : tr.GetResult(tmp, Time.time - StartTime);
            SetTransform(tmp.position, tmp.rotation.eulerAngles, tmp.localScale);
            Destroy(tmp.gameObject);
            // Debug.Log("Transforming: " + (Time.time - StartTime) + " / " + EndTime);
            if (Time.time - StartTime > EndTime){ Transforming = false;cooldown = false; }
            if(Transforming == false&&DisableT) gameObject.SetActive(false);
        }
    }
}

[Serializable]
public class SimpleTransform
{
    [SerializeField]
    public TransformType TransType;

    [SerializeField]
    public bool UseWorldSpace = false; // 是否使用世界坐标系

    [SerializeField]
    public Vector3 startPosition;

    [SerializeField]
    public Vector3 endPosition;

    [SerializeField]
    public Vector3 startRotation;

    [SerializeField]
    public Vector3 endRotation;

    [SerializeField]
    public Vector3 startScale;

    [SerializeField]
    public Vector3 endScale;

    [SerializeField]
    public float startAlpha = 1f;

    [SerializeField]
    public float endAlpha = 1f;
    
    [SerializeField]
    public UnityEvent<float> alphaEvent = new UnityEvent<float>();

    [SerializeField]
    public UnityEvent activeEvent = new UnityEvent();

    [SerializeField]
    public int startOrder = 0; // 初始排序

    [SerializeField]
    public int TargetOrder = 0; // 目标排序

    [SerializeField]
    public UpdateType updateType;

    [SerializeField]
    public float duration;

    [SerializeField]
    public float delay = 0f; // 延迟时间

    [SerializeField]
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1); // 默认线性曲线

    private bool actived = false;

    public void Init(){
        actived = false;
    }

    public Transform GetResult(Transform target,float time){
        float pct = 0f;
        if (time < delay)
            return target;
        if ( time > delay + duration )
            pct = 1f;
        else
            if(updateType==UpdateType.Linear)
                pct = (time - delay) / duration;
            else if (updateType == UpdateType.Sin)
                pct = 0.5f*(Mathf.Sin(((time - delay) / duration*2 -1)* Mathf.PI * 0.5f)+1);
            else if (updateType == UpdateType.Expontial)
                pct = 1-Mathf.Pow(0.9f,60*(time-delay)/duration);
            else if (updateType == UpdateType.Spline)
                pct = curve.Evaluate((time - delay) / duration);
            else if (updateType == UpdateType.None)
                pct = 0f;
            else Debug.LogError("SimpleTransformer: Unknown UpdateType!");
        if (UseWorldSpace)
        {
            if (TransType == TransformType.Translate)
                target.position += pct* (endPosition - startPosition);
            else if (TransType == TransformType.Rotate)
                target.rotation *= Quaternion.Euler(pct * (endRotation - startRotation));
            else if (TransType == TransformType.Scale)
                {Transform t = target.parent;target.parent=null;target.localScale += pct * (endScale - startScale);target.parent = t;}
            else if (TransType == TransformType.Alpha)
                alphaEvent.Invoke(Mathf.Lerp(startAlpha, endAlpha, pct));
            else if (TransType == TransformType.Active)
                {if (pct > 0f&&!actived) {activeEvent.Invoke();actived=true;}}
            else if (TransType == TransformType.Order)
                Utils.SetSortOrder(target.gameObject, TargetOrder);
            else if (TransType == TransformType.None) {}
            else Debug.LogError("SimpleTransformer: Unknown TransformType!");
        }
        else
        {
            if (TransType == TransformType.Translate)
                target.localPosition += pct* (endPosition - startPosition);
            else if (TransType == TransformType.Rotate)
                target.localRotation *= Quaternion.Euler(pct * (endRotation - startRotation));
            else if (TransType == TransformType.Scale)
                target.localScale += pct * (endScale - startScale);
            else if (TransType == TransformType.Alpha)
                alphaEvent.Invoke(Mathf.Lerp(startAlpha, endAlpha, pct));
            else if (TransType == TransformType.Active)
                {if (pct > 0f&&!actived) {activeEvent.Invoke();actived=true;}}
            else if (TransType == TransformType.Order)
                Utils.SetSortOrder(target.gameObject, TargetOrder);
            else if (TransType == TransformType.None) { }
            else Debug.LogError("SimpleTransformer: Unknown TransformType!");
        }
        return target;
    }

    public Transform GetInvResult(Transform target,float time){
        float pct = 0f;
        if (time < delay)
            return target;
        if (time > delay + duration )
            pct = 1f;
        else
            if(updateType==UpdateType.Linear)
                pct = (time - delay) / duration;
            else if (updateType == UpdateType.Sin)
                pct = 0.5f*(Mathf.Sin(((time - delay) / duration*2 -1)* Mathf.PI * 0.5f)+1);
            else if (updateType == UpdateType.Expontial)
                pct = 1-Mathf.Pow(0.9f,60*(time-delay)/duration);
            else if (updateType == UpdateType.Spline)
                pct = curve.Evaluate((time - delay) / duration);
            else if (updateType == UpdateType.None)
                pct = 1f;
            else Debug.LogError("SimpleTransformer: Unknown UpdateType!");
        if (UseWorldSpace)
        {
            if (TransType == TransformType.Translate)
                target.position += pct* (startPosition-endPosition);
            else if (TransType == TransformType.Rotate)
                target.rotation *= Quaternion.Euler(pct * (startRotation-endRotation));
            else if (TransType == TransformType.Scale)
                {Transform t = target.parent;target.parent=null;target.localScale += pct * (startScale-endScale);target.parent = t;}
            else if (TransType == TransformType.Alpha)
                alphaEvent.Invoke(Mathf.Lerp(endAlpha, startAlpha, pct));
            else if (TransType == TransformType.Active)
                {if (pct > 0f&&!actived) {activeEvent.Invoke();actived=true;}}
            else if (TransType == TransformType.Order)
                Utils.SetSortOrder(target.gameObject, startOrder);
            else if (TransType == TransformType.None) { }
            else Debug.LogError("SimpleTransformer: Unknown TransformType!");
        }else{
            if (TransType == TransformType.Translate)
                target.localPosition += pct* (startPosition-endPosition);
            else if (TransType == TransformType.Rotate)
                target.localRotation *= Quaternion.Euler(pct * (startRotation-endRotation));
            else if (TransType == TransformType.Scale)
                target.localScale += pct * (startScale-endScale);
            else if (TransType == TransformType.Alpha)
                alphaEvent.Invoke(Mathf.Lerp(endAlpha, startAlpha, pct));
            else if (TransType == TransformType.Active)
                {if (pct > 0f&&!actived) {activeEvent.Invoke();actived=true;}}
            else if (TransType == TransformType.Order)
                Utils.SetSortOrder(target.gameObject, startOrder);
            else if (TransType == TransformType.None) { }
            else Debug.LogError("SimpleTransformer: Unknown TransformType!");
        }
        return target;
    }

}

public enum TransformType
{
    None,
    Translate,
    Rotate,
    Scale,
    Alpha,
    Order,
    Active,
}

public enum UpdateType
{
    None,
    Linear,
    Sin,
    Expontial,
    Spline,
}
