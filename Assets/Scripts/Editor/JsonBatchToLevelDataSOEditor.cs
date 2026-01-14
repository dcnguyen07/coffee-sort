using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class JsonBatchToLevelDataSOEditor : Editor
{
    [MenuItem("Assets/Create/Export To Level Files", false, 100)]
    private static void ConvertSelectedJsonToLevelDataSO()
    {
        string folderPath = "Assets/GameLevels/";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        foreach (Object obj in Selection.objects)
        {
            string jsonPath = AssetDatabase.GetAssetPath(obj);

            if (!jsonPath.EndsWith(".json"))
            {
                Debug.LogWarning($"❌ Skipped {jsonPath}: Not a JSON file.");
                continue;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            try
            {
                JObject jsonObject = JObject.Parse(jsonContent);
                LevelDataSO levelDataSO = ScriptableObject.CreateInstance<LevelDataSO>();

                // ✅ Gán dữ liệu cơ bản
                levelDataSO.level = jsonObject["Level"]?.Value<int>() ?? 0;
                levelDataSO.maxRow = jsonObject["MaxRow"]?.Value<int>() ?? 0;
                levelDataSO.maxCol = jsonObject["MaxCol"]?.Value<int>() ?? 0;

                // ✅ Xử lý Layers
                levelDataSO.layers = new List<LayerGroup>();

                if (jsonObject.ContainsKey("Layers") && jsonObject["Layers"] is JObject layersObj)
                {
                    foreach (var layerPair in layersObj)
                    {
                        LayerGroup group = new LayerGroup
                        {
                            trays = new List<LayerData>()
                        };

                        if (layerPair.Value is JArray traysArray)
                        {
                            foreach (var tray in traysArray)
                            {
                                LayerData trayData = new LayerData
                                {
                                    id = tray["ID"]?.Value<int>() ?? 0,
                                    resID = tray["ResID"]?.Value<int>() ?? 0,
                                    layer = tray["Layer"]?.Value<int>() ?? 0,
                                    posX = tray["PosX"]?.Value<int>() ?? 0,
                                    posY = tray["PosY"]?.Value<int>() ?? 0,
                                    angle = tray["Angle"]?.Value<int>() ?? 0,
                                    isQuestion = tray["IsQuestion"]?.Value<int>() ?? 0,
                                    cupIds = tray["CupIds"]?.ToObject<List<int>>() ?? new List<int>(),
                                    parentIds = tray["ParentIds"]?.ToObject<List<int>>()
                                };

                                group.trays.Add(trayData);
                            }
                        }

                        levelDataSO.layers.Add(group);
                    }
                }

                // ✅ Xử lý SpecialElementList
                if (jsonObject.ContainsKey("SpecialElementList") && jsonObject["SpecialElementList"] is JArray specialElementsArray)
                {
                    levelDataSO.SpecialElementList = new List<SpecialElement>();

                    foreach (var special in specialElementsArray)
                    {
                        SpecialElement element = new SpecialElement
                        {
                            ID = special["ID"]?.Value<int>() ?? 0,
                            ResId = special["ResId"]?.Value<int>() ?? 0,
                            LinkPlateId = special["LinkPlateId"]?.Value<int>() ?? 0,
                            LinkPlateIdList = special["LinkPlateIdList"]?.ToObject<List<int>>() ?? new List<int>(),
                            PosX = special["PosX"]?.Value<int>() ?? 0,
                            PosY = special["PosY"]?.Value<int>() ?? 0
                        };

                        levelDataSO.SpecialElementList.Add(element);
                    }
                }

                // ✅ Xử lý Cups
                if (jsonObject.ContainsKey("Cups") && jsonObject["Cups"] is JArray cupsArray)
                {
                    levelDataSO.cups = cupsArray.ToObject<List<int>>() ?? new List<int>();
                }
                else
                {
                    levelDataSO.cups = new List<int>();
                }

                // ✅ Lưu Asset vào thư mục GameLevels
                string assetPath = $"{folderPath}{Path.GetFileNameWithoutExtension(jsonPath)}.asset";
                AssetDatabase.CreateAsset(levelDataSO, assetPath);
                AssetDatabase.SaveAssets();

                Debug.Log($"✅ Successfully exported: {assetPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to convert {jsonPath}: {e.Message}");
            }
        }

        AssetDatabase.Refresh();
    }
}