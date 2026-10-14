using System;
using System.IO;
using UnityEditor;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>只生成本批改变结构的固定元素表，避免全量生成改动暂缓的其它表。</summary>
    public static class FixedElementTableGenerator
    {
        [MenuItem("Game Framework/EverlightTales/Board/只生成固定元素表")]
        public static void Generate()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("停止Play Mode后生成配置。");
            // Editor工具程序集不作为UI运行程序集依赖；调用已存在的公开生成器入口。
            Type excelGenerator = FindType("UGF.EditorTools.GameDataGenerator");
            Type codeGenerator = FindType("GameFramework.Editor.DataTableTools.DataTableGenerator");
            string root = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            string source = Path.Combine(root, "GameData/DataTables/Board/LevelFixedElementTable.xlsx");
            string output = Path.Combine(root, "Assets/Game/DataTable/Board/LevelFixedElementTable.txt");
            if (!File.Exists(source) || !(bool)excelGenerator.GetMethod("Excel2TxtFile").Invoke(null, new object[] { source, output, true }))
                throw new IOException("固定元素表导出失败。");
            object processor = codeGenerator.GetMethod("CreateDataTableProcessor").Invoke(null, new object[] { output });
            if (!(bool)codeGenerator.GetMethod("CheckRawData").Invoke(null, new[] { processor, output }))
                throw new InvalidOperationException("固定元素表结构无效。");
            if (AppConfigs.GetInstanceEditor().LoadFromBytes)
                codeGenerator.GetMethod("GenerateDataFile").Invoke(null, new[] { processor, output });
            codeGenerator.GetMethod("GenerateCodeFile").Invoke(null, new[] { processor, output });
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[BoardChallenge][Generate] Board/LevelFixedElementTable only");
        }

        private static Type FindType(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name, false);
                if (type != null) return type;
            }
            throw new InvalidOperationException("项目生成器类型未加载：" + name);
        }
    }
}
