using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(RectTransform))]
public class ExpandAbleGrid : MonoBehaviour
{
    public GameObject TargetPerfab;
    public List<GameObject> TargetPerfabs;
    public AnchorType anchorType = AnchorType.Center;
    public enum AnchorType
    {
        TopLeft,
        TopMiddle,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomMiddle,
        BottomRight,
    }

    public int columns = 5;
    public int rows = 5;
    public float DefaultCellWidth = 100f;
    public float DefaultCellHeight = 100f;
    public bool UseTargetSize = false;
    public float TargetCellWSpace = 100f;
    public float TargetCellHSpace = 100f;
    public float CellScale = 1f;

    public Vector2 CellOffset = Vector2.zero;

    public int CellsCount = -1;

    public int ActCellsCount
    {
        get
        {
            if (CellsCount == -1)
            {
                return rows * columns;
            }
            else
            {
                return CellsCount;
            }
        }
    }

    public RectTransform TargetScrollRectRectTransform;

    public GameObject VerticalScrollbar;
    public float HeightToHideVerticalScrollbar = -1f;
    public GameObject HorizontalScrollbar;
    public float WidthToHideHorizontalScrollbar = -1f;

    public List<float> cellWidths = new List<float>();
    public List<float> cellHeights = new List<float>();

    public List<SerializableList<int>> cellData = new();
    public List<GameObject> cellObjects = new List<GameObject>();

    public void ClearCells()
    {
        foreach (var cell in cellObjects)
        {
            if(Application.isPlaying)
                Destroy(cell);
            else
                DestroyImmediate(cell);
        }
        cellObjects.Clear();
    }

    [ContextMenu("Respawn Cells At All")]
    public void RespawnCellsAtAll()
    {
        ClearCells();
        cellWidths = new List<float>();
        cellHeights = new List<float>();

        for (int i = 0; i < columns; i++)
        {
            cellWidths.Add(DefaultCellWidth);
        }
        for (int i = 0; i < rows; i++)
        {
            cellHeights.Add(DefaultCellHeight);
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 size = rectTransform.sizeDelta;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cellObject = (GameObject)PrefabUtility.InstantiatePrefab(TargetPerfabs[cellData[row][col]], transform);
                RectTransform cellRectTransform = cellObject.GetComponent<RectTransform>();
                // cellRectTransform.sizeDelta = new Vector2(cellWidths[col], cellHeights[row]);
                Vector2 position = CalculateCellPosition(row, col, size);
                cellRectTransform.anchoredPosition = position;
                cellObjects.Add(cellObject);
            }
        }
    }

    public void AdjustCellSize(int row, int col, float width, float height)
    {
        if (row < 0 || row >= rows || col < 0 || col >= columns)
        {
            Debug.LogError("Row or column index out of bounds.");
            return;
        }

        // Adjust the cell size
        cellWidths[col] = width;
        cellHeights[row] = height;

        // Update the size of the existing cell object
        UpdateCell();
    }

    public void AdjustRowSize(int row, float height=default)
    {
        if (row < 0 || row >= rows)
        {
            Debug.LogError("Row index out of bounds.");
            return;
        }

        if (height == default) height = DefaultCellHeight;
        cellHeights[row] = height;

        UpdateCell();
    }

    public void AdjustColumnSize(int col, float width = default)
    {
        if (col < 0 || col >= columns)
        {
            Debug.LogError("Column index out of bounds.");
            return;
        }

        if (width == default) width = DefaultCellWidth;
        cellWidths[col] = width;

        UpdateCell();
    }

    [ContextMenu("Reset Cell Size")]
    public void ResetCellSize()
    {
        cellWidths.Clear();
        cellHeights.Clear();

        for (int i = 0; i < columns; i++)
        {
            cellWidths.Add(DefaultCellWidth);
        }
        for (int i = 0; i < rows; i++)
        {
            cellHeights.Add(DefaultCellHeight);
        }

        UpdateCell();
    }

    public Vector2 CalculateCellPosition(int row, int col, Vector2 gridSize)
    {
        // 计算累积位置
        float accumulatedX = 0f;
        for (int i = 0; i < col; i++)
        {
            accumulatedX += cellWidths[i];
        }
        
        float accumulatedY = 0f;
        for (int i = 0; i < row; i++)
        {
            accumulatedY += cellHeights[i];
        }

        float totalWidth = CalculateTotalWidth();
        float totalHeight = CalculateTotalHeight();
        
        float x = 0f;
        float y = 0f;

        // Calculate the position based on the anchor type
        switch (anchorType)
        {
            case AnchorType.TopLeft:
                x = accumulatedX + cellWidths[col] * 0.5f;
                y = -(accumulatedY + cellHeights[row] * 0.5f);
                break;
            case AnchorType.TopMiddle:
                x = accumulatedX - totalWidth * 0.5f + cellWidths[col] * 0.5f;
                y = -(accumulatedY + cellHeights[row] * 0.5f);
                break;
            case AnchorType.TopRight:
                x = accumulatedX - totalWidth + cellWidths[col] * 0.5f;
                y = -(accumulatedY + cellHeights[row] * 0.5f);
                break;
            case AnchorType.MiddleLeft:
                x = accumulatedX + cellWidths[col] * 0.5f;
                y = totalHeight * 0.5f - accumulatedY - cellHeights[row] * 0.5f;
                break;
            case AnchorType.Center:
                x = accumulatedX - totalWidth * 0.5f + cellWidths[col] * 0.5f;
                y = totalHeight * 0.5f - accumulatedY - cellHeights[row] * 0.5f;
                break;
            case AnchorType.MiddleRight:
                x = accumulatedX - totalWidth + cellWidths[col] * 0.5f;
                y = totalHeight * 0.5f - accumulatedY - cellHeights[row] * 0.5f;
                break;
            case AnchorType.BottomLeft:
                x = accumulatedX + cellWidths[col] * 0.5f;
                y = totalHeight - accumulatedY - cellHeights[row] * 0.5f;
                break;
            case AnchorType.BottomMiddle:
                x = accumulatedX - totalWidth * 0.5f + cellWidths[col] * 0.5f;
                y = totalHeight - accumulatedY - cellHeights[row] * 0.5f;
                break;
            case AnchorType.BottomRight:
                x = accumulatedX - totalWidth + cellWidths[col] * 0.5f;
                y = totalHeight - accumulatedY - cellHeights[row] * 0.5f;
                break;
        }

        return new Vector2(x, y) + CellOffset;
    }

    public float CalculateTotalWidth()
    {
        float totalWidth = 0f;
        for (int i = 0; i < columns; i++)
        {
            totalWidth += cellWidths[i];
        }
        return totalWidth;
    }

    public float CalculateTotalHeight()
    {
        float totalHeight = 0f;
        for (int i = 0; i < rows; i++)
        {
            totalHeight += cellHeights[i];
        }
        return totalHeight;
    }

    public void UpdateDataInfo(){
        if (cellWidths.Count != columns || cellHeights.Count != rows)
        {
            while (cellWidths.Count < columns) cellWidths.Add(DefaultCellWidth);
            while (cellHeights.Count < rows) cellHeights.Add(DefaultCellHeight);
            while (cellWidths.Count > columns) cellWidths.RemoveAt(cellWidths.Count - 1);
            while (cellHeights.Count > rows) cellHeights.RemoveAt(cellHeights.Count - 1);
        }

        for (int i = 0; i < cellObjects.Count; i++)
        {
            if (cellObjects[i] == null)
            {
                cellObjects.RemoveAt(i);
                i--;
            }
        }

        if(CellsCount == -1){
            while (cellData.Count < rows) cellData.Add(new SerializableList<int>());
            while (cellData.Count > rows) cellData.RemoveAt(cellData.Count - 1);
            for (int i = 0; i < cellData.Count; i++)
            {
                while (cellData[i].Count < columns) cellData[i].Add(0);
                while (cellData[i].Count > columns) cellData[i].RemoveAt(cellData[i].Count - 1);
            }
        }

        if(UseTargetSize){
            for (int i = 0; i < cellWidths.Count; i++)
            {
                int num = ActCellsCount / columns + (ActCellsCount % columns > i ? 1 : 0);
                float maxWidth = 0f;
                for (int j = 0; j < num; j++)
                {
                    if (ActCellsCount > i + j * columns)
                    {
                        RectTransform cellRectTransform = TargetPerfabs[cellData[j][i]].GetComponent<RectTransform>();
                        if (cellRectTransform != null)
                        {
                            float width = cellRectTransform.sizeDelta.x * CellScale;
                            // Debug.Log($"Cell {i + j * columns} width: {width}");
                            if (width > maxWidth)
                            {
                                maxWidth = width;
                            }
                        }
                    }
                }
                cellWidths[i] = maxWidth + TargetCellWSpace;
            }
            for (int i = 0; i < cellHeights.Count; i++)
            {
                if (columns == 0) continue;
                int num = (i<ActCellsCount / columns) ? columns : ActCellsCount % columns;
                float maxHeight = 0f;
                for (int j = 0; j < num; j++)
                {
                    if (ActCellsCount > i + j * columns)
                    {
                        RectTransform cellRectTransform = TargetPerfabs[cellData[i][j]].GetComponent<RectTransform>();
                        if (cellRectTransform != null)
                        {
                            float height = cellRectTransform.sizeDelta.y * CellScale;
                            if (height > maxHeight)
                            {
                                maxHeight = height;
                            }
                        }
                    }
                }
                cellHeights[i] = maxHeight + TargetCellHSpace;
            }
        }
    }

    [ContextMenu("Update Cell")]
    public void UpdateCell()
    {

        UpdateDataInfo();

        if (CellsCount == -1){
            while(cellObjects.Count != rows * columns)
            {
                if (cellObjects.Count < rows * columns)
                {
                    int row = cellObjects.Count / columns;
                    int col = cellObjects.Count % columns;
                    GameObject cellObject = (GameObject)PrefabUtility.InstantiatePrefab(TargetPerfabs[cellData[row][col]], transform);
                    RectTransform cellRectTransform = cellObject.GetComponent<RectTransform>();
                    // cellRectTransform.sizeDelta = new Vector2(DefaultCellWidth, DefaultCellHeight);
                    Vector2 position = CalculateCellPosition(row, col, GetComponent<RectTransform>().sizeDelta);
                    cellRectTransform.anchoredPosition = position;
                    cellObjects.Add(cellObject);
                }
                else if (cellObjects.Count > rows * columns)
                {
                    GameObject cellToRemove = cellObjects[cellObjects.Count - 1];
                    if(Application.isPlaying)
                        Destroy(cellToRemove);
                    else
                        DestroyImmediate(cellToRemove);
                    cellObjects.RemoveAt(cellObjects.Count - 1);
                }
            }
            for (int i = 0; i < cellObjects.Count; i++)
            {
                RectTransform cellRectTransform = cellObjects[i].GetComponent<RectTransform>();
                int row = i / columns;
                int col = i % columns;
                // cellRectTransform.sizeDelta = new Vector2(cellWidths[col], cellHeights[row]);
                cellRectTransform.localScale = new Vector3(CellScale, CellScale, 1f);
                Vector2 position = CalculateCellPosition(row, col, GetComponent<RectTransform>().sizeDelta);
                cellRectTransform.anchoredPosition = position;
            }
            if (TargetScrollRectRectTransform != null)
            {
                TargetScrollRectRectTransform.sizeDelta = new Vector2(CalculateTotalWidth(), CalculateTotalHeight());
                if (VerticalScrollbar != null && HeightToHideVerticalScrollbar > 0) VerticalScrollbar.SetActive(TargetScrollRectRectTransform.sizeDelta.y > HeightToHideVerticalScrollbar);
                if (HorizontalScrollbar != null && WidthToHideHorizontalScrollbar > 0) HorizontalScrollbar.SetActive(TargetScrollRectRectTransform.sizeDelta.x > WidthToHideHorizontalScrollbar);

            }
        }else{
            while (cellObjects.Count != CellsCount)
            {
                if (cellObjects.Count < CellsCount)
                {
                    int row = cellObjects.Count / columns;
                    int col = cellObjects.Count % columns;
                    GameObject cellObject = (GameObject)PrefabUtility.InstantiatePrefab(TargetPerfabs[cellData[row][col]], transform);
                    RectTransform cellRectTransform = cellObject.GetComponent<RectTransform>();
                    // cellRectTransform.sizeDelta = new Vector2(DefaultCellWidth, DefaultCellHeight);
                    Vector2 position = CalculateCellPosition(cellObjects.Count / columns, cellObjects.Count % columns, GetComponent<RectTransform>().sizeDelta);
                    cellRectTransform.anchoredPosition = position;
                    cellObjects.Add(cellObject);
                }
                else if (cellObjects.Count > CellsCount)
                {
                    GameObject cellToRemove = cellObjects[cellObjects.Count - 1];
                    if(Application.isPlaying)
                        Destroy(cellToRemove);
                    else
                        DestroyImmediate(cellToRemove);
                    cellObjects.RemoveAt(cellObjects.Count - 1);
                }
            }
            for (int i = 0; i < cellObjects.Count; i++)
            {
                RectTransform cellRectTransform = cellObjects[i].GetComponent<RectTransform>();
                int row = i / columns;
                int col = i % columns;
                // cellRectTransform.sizeDelta = new Vector2(cellWidths[col], cellHeights[row]);
                cellRectTransform.localScale = new Vector3(CellScale, CellScale, 1f);
                Vector2 position = CalculateCellPosition(row, col, GetComponent<RectTransform>().sizeDelta);
                cellRectTransform.anchoredPosition = position;
            }
            if (TargetScrollRectRectTransform != null)
            {
                TargetScrollRectRectTransform.sizeDelta = new Vector2(CalculateTotalWidth(), CalculateTotalHeight());
                if (VerticalScrollbar != null && HeightToHideVerticalScrollbar > 0) VerticalScrollbar.SetActive(TargetScrollRectRectTransform.sizeDelta.y > HeightToHideVerticalScrollbar);
                if (HorizontalScrollbar != null && WidthToHideHorizontalScrollbar > 0) HorizontalScrollbar.SetActive(TargetScrollRectRectTransform.sizeDelta.x > WidthToHideHorizontalScrollbar);

            }
        }
    }

    public Vector2Int GetSelfRowCol(GameObject target)
    {
        if (cellObjects.Contains(target))
        {
            int index = cellObjects.IndexOf(target);
            int row = index / columns;
            int col = index % columns;
            return new Vector2Int(row, col);
        }
        else
        {
            // Debug.LogWarning("Target GameObject is not in the cellObjects list.");
            return Vector2Int.one * -1;
        }
    }

}
