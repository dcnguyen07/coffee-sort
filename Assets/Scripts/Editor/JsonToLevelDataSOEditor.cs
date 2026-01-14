using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

public class JsonToLevelDataSOEditor : EditorWindow
{
    private TextAsset jsonFile;

    [MenuItem("Tools/Json To LevelDataSO")]
    public static void ShowWindow()
    {
        GetWindow(typeof(JsonToLevelDataSOEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert JSON to LevelDataSO", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);

        if (GUILayout.Button("Convert to LevelDataSO") && jsonFile != null)
        {
            ConvertJsonToSO(jsonFile);
        }
    }

    private void ConvertJsonToSO(TextAsset jsonFile)
    {
        LevelDataJson levelDataJson = JsonConvert.DeserializeObject<LevelDataJson>(jsonFile.text);
        LevelDataSO levelDataSO = ScriptableObject.CreateInstance<LevelDataSO>();

        levelDataSO.level = levelDataJson.Level;
        levelDataSO.maxRow = levelDataJson.MaxRow;
        levelDataSO.maxCol = levelDataJson.MaxCol;
        levelDataSO.cups = new List<int>(levelDataJson.Cups);

        levelDataSO.layers = new List<LayerGroup>();

        foreach (var layerPair in levelDataJson.Layers)
        {
            LayerGroup group = new LayerGroup
            {
                trays = new List<LayerData>()
            };

            foreach (var tray in layerPair.Value)
            {
                LayerData trayData = new LayerData
                {
                    id = tray.ID,
                    resID = tray.ResID,
                    layer = tray.Layer,
                    posX = tray.PosX,
                    posY = tray.PosY,
                    angle = tray.Angle,
                    isQuestion = tray.IsQuestion,
                    cupIds = new List<int>(tray.CupIds),
                    parentIds = tray.ParentIds != null ? new List<int>(tray.ParentIds) : null
                };

                group.trays.Add(trayData);
            }

            levelDataSO.layers.Add(group);

            // Chuyển đổi danh sách SpecialElementList

            if (levelDataJson.SpecialElementList != null) // ✅ Kiểm tra nếu có dữ liệu phần tử đặc biệt
            {
                levelDataSO.SpecialElementList.Clear();
                foreach (var special in levelDataJson.SpecialElementList)
                {
                    SpecialElement element = new SpecialElement
                    {
                        ID = special.ID,
                        ResId = special.ResId,
                        LinkPlateId = special.LinkPlateId,
                        LinkPlateIdList = new List<int>(special.LinkPlateIdList),
                        PosX = special.PosX,
                        PosY = special.PosY
                    };

                    levelDataSO.SpecialElementList.Add(element);
                }
            }      
        }

        string path = EditorUtility.SaveFilePanelInProject("Save LevelDataSO", $"LevelData_{levelDataJson.Level}.asset", "asset", "Save LevelDataSO asset");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(levelDataSO, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = levelDataSO;
        }
    }
}

[System.Serializable]
public class LevelDataJson
{
    public int Level;
    public int MaxRow;
    public int MaxCol;
    public Dictionary<string, List<LayerDataJson>> Layers;
    public List<SpecialElement> SpecialElementList;
    public List<int> Cups;
}

[System.Serializable]
public class LayerDataJson
{
    public int ID;
    public int ResID;
    public int Layer;
    public int PosX;
    public int PosY;
    public int Angle;
    public int IsQuestion;
    public List<int> CupIds;
    public List<int> ParentIds;
}