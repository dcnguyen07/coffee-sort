using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelDataSO : ScriptableObject
{
    public int level;
    public int maxRow;
    public int maxCol;
    public List<LayerGroup> layers;
    public List<SpecialElement> SpecialElementList = new List<SpecialElement>(); // Danh sách phần tử đặc biệt
    public List<int> cups;
}

[System.Serializable]
public class LayerGroup
{
    public List<LayerData> trays;
}

[System.Serializable]
public class LayerData
{
    public int id;
    public int resID;
    public int layer;
    public int posX;
    public int posY;
    public int angle;
    public int isQuestion;
    public List<int> cupIds;
    public List<int> parentIds;
}
[Serializable]
public class SpecialElement
{
    public int ID; 
    public int ResId;
    public int LinkPlateId; 
    public List<int> LinkPlateIdList = new List<int>();
    public int PosX;
    public int PosY;
}