using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Utils
{
    public static int BinaryBoolenToInt(bool[] bools)
    {
        int result = 0;
        for (int i = 0; i < bools.Length; i++)
            if (bools[i]) result |= 1 << i;
        return result;
    }

    public static bool[] IntToBinaryBoolen(int value, int length)
    {
        bool[] result = new bool[length];
        for (int i = 0; i < length; i++) result[i] = (value & (1 << i)) != 0;
        return result;
    }

    public static bool GetBoolenWithIndex(int value, int index)
    {
        return (value & (1 << index)) != 0;
    }

    public static int SetBoolenWithIndex(int value, int index, bool set)
    {
        if (set)return value | (1 << index);
        else return value & ~(1 << index);
    }
    

    public static void SetSortOrder(GameObject tar,int queue)
    {
        SpriteRenderer[] renderers = tar.GetComponentsInChildren<SpriteRenderer>();
        Canvas[] canvases = tar.GetComponentsInChildren<Canvas>();
        ParticleSystem[] particles = tar.GetComponentsInChildren<ParticleSystem>();
        List<int> pos = new();
        foreach (SpriteRenderer r in renderers) pos.Add(r.sortingOrder);
        foreach (Canvas c in canvases) pos.Add(c.sortingOrder);
        foreach (ParticleSystem p in particles) pos.Add(p.gameObject.GetComponent<ParticleSystemRenderer>().sortingOrder);
        int min = int.MaxValue;
        foreach (int i in pos) if (i < min) min = i;
        foreach (SpriteRenderer r in renderers) r.sortingOrder = queue + r.sortingOrder - min;
        foreach (Canvas c in canvases) c.sortingOrder = queue + c.sortingOrder - min;
        foreach (ParticleSystem p in particles) p.gameObject.GetComponent<ParticleSystemRenderer>().sortingOrder = queue + p.gameObject.GetComponent<ParticleSystemRenderer>().sortingOrder - min;
    }

    public static void GetSortOrder(GameObject tar, out int queue)
    {
        SpriteRenderer[] renderers = tar.GetComponentsInChildren<SpriteRenderer>();
        Canvas[] canvases = tar.GetComponentsInChildren<Canvas>();
        ParticleSystem[] particles = tar.GetComponentsInChildren<ParticleSystem>();
        List<int> pos = new();
        foreach (SpriteRenderer r in renderers) pos.Add(r.sortingOrder);
        foreach (Canvas c in canvases) pos.Add(c.sortingOrder);
        foreach (ParticleSystem p in particles) pos.Add(p.gameObject.GetComponent<ParticleSystemRenderer>().sortingOrder);
        int min = int.MaxValue;
        foreach (int i in pos) if (i < min) min = i;
        queue = min;
    }

    public static int StringToEnumInt<TEnum>(string str) where TEnum : struct
    {
        if (System.Enum.TryParse<TEnum>(str, out var result))
        {
            return (int)(object)result;
        }
        return -1; // 转换失败返回-1
    }

    public static Color GetRainbowColor(float percentage)
    {
        // 确保百分比在0到1之间
        percentage = Mathf.Clamp01(percentage);

        // 使用HSV颜色模型，H从0（红）到0.833（紫），S和V固定为1
        float hue = percentage * 0.833f;
        return Color.HSVToRGB(hue, 1f, 1f);
    }
}

[Serializable]
class SimpleParams
{
    public int IntValue;
    public List<int> IntList;
    public float FloatValue;
    public List<float> FloatList;
    public string StringValue;
    public List<string> StringList;
    public bool BoolValue;
    public List<bool> BoolList;
    public Vector3 Vector3Value;
    public List<Vector3> Vector3List;
    public Quaternion QuaternionValue;
    public List<Quaternion> QuaternionList;
    public Color ColorValue;
    public List<Color> ColorList;
    public GameObject GameObjectValue;
    public List<GameObject> GameObjectList;
    public List<SimpleParams> SimpleParamsList;
}

[Serializable]
public class SerializableList<T>
{

    public List<T> VecList;

    public SerializableList() {
        VecList = new List<T>();
    }

    public T this[int index] {
        get { return VecList[index]; }
        set { VecList[index] = value; }
    }

    public void Add(T item)
    {
        VecList.Add(item);
    }

    public void Remove(T item)
    {
        VecList.Remove(item);
    }

    public void Clear()
    {
        VecList.Clear();
    }

    public int Count
    {
        get { return VecList.Count; }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return VecList.GetEnumerator();
    }

    public void AddRange(IEnumerable<T> collection)
    {
        VecList.AddRange(collection);
    }

    public void RemoveAt(int index)
    {
        VecList.RemoveAt(index);
    }

    public void Insert(int index, T item)
    {
        VecList.Insert(index, item);
    }

    public void Sort(Comparison<T> comparison)
    {
        VecList.Sort(comparison);
    }

    public void Sort(IComparer<T> comparer)
    {
        VecList.Sort(comparer);
    }

    public void Reverse()
    {
        VecList.Reverse();
    }

    public void Reverse(int index, int count)
    {
        VecList.Reverse(index, count);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        VecList.CopyTo(array, arrayIndex);
    }

    public T[] ToArray()
    {
        return VecList.ToArray();
    }

    public void TrimExcess()
    {
        VecList.TrimExcess();
    }

    public bool Contains(T item)
    {
        return VecList.Contains(item);
    }

    public int IndexOf(T item)
    {
        return VecList.IndexOf(item);
    }

    public int LastIndexOf(T item)
    {
        return VecList.LastIndexOf(item);
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        VecList.InsertRange(index, collection);
    }

    public List<T> GetRange(int index, int count)
    {
        return VecList.GetRange(index, count);
    }

    public bool Exists(Predicate<T> match)
    {
        return VecList.Exists(match);
    }

    public T Find(Predicate<T> match)
    {
        return VecList.Find(match);
    }

    public List<T> FindAll(Predicate<T> match)
    {
        return VecList.FindAll(match);
    }

    public int FindIndex(Predicate<T> match)
    {
        return VecList.FindIndex(match);
    }

    public int FindIndex(int startIndex, Predicate<T> match)
    {
        return VecList.FindIndex(startIndex, match);
    }

    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        return VecList.FindIndex(startIndex, count, match);
    }

    public T FindLast(Predicate<T> match)
    {
        return VecList.FindLast(match);
    }

    public int FindLastIndex(Predicate<T> match)
    {
        return VecList.FindLastIndex(match);
    }

    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
        return VecList.FindLastIndex(startIndex, match);
    }

    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        return VecList.FindLastIndex(startIndex, count, match);
    }

    public void ForEach(Action<T> action)
    {
        VecList.ForEach(action);
    }

    public int BinarySearch(T item)
    {
        return VecList.BinarySearch(item);
    }

    public int BinarySearch(T item, IComparer<T> comparer)
    {
        return VecList.BinarySearch(item, comparer);
    }

    public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
    {
        return VecList.BinarySearch(index, count, item, comparer);
    }

}
