using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    public static class FormalResourceValidation
    {
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/验证资产")]
        public static void ValidateAssets()
        {
            var errors = new List<string>(); int prefabs = 0, sprites = 0;
            var catalog = AssetDatabase.LoadAssetAtPath<UIFormalSpriteCatalog>(FormalResourceMigration.CatalogPath);
            Require(catalog != null,"资源目录存在",errors);
            var so = new SerializedObject(catalog); var entries = so.FindProperty("_entries");
            var keys = new HashSet<string>();
            for(int i=0;i<entries.arraySize;i++)
            {
                var e=entries.GetArrayElementAtIndex(i); string key=e.FindPropertyRelative("Key").stringValue;
                Require(keys.Add(key),"重复键："+key,errors); Require(e.FindPropertyRelative("Sprite").objectReferenceValue != null,"缺图："+key,errors); sprites++;
            }
            foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Game/Prefabs/UI"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid); GameObject root=AssetDatabase.LoadAssetAtPath<GameObject>(path); prefabs++;
                foreach(var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    Require(component != null,"丢失脚本："+path,errors); if(component==null)continue;
                    var obj=new SerializedObject(component); var prop=obj.GetIterator();
                    while(prop.Next(true))
                        if(prop.propertyType==SerializedPropertyType.ObjectReference && prop.objectReferenceValue==null && prop.objectReferenceInstanceIDValue!=0)
                            errors.Add("丢失引用："+path+"/"+component.name+"."+prop.propertyPath);
                }
                foreach(string dependency in AssetDatabase.GetDependencies(path,true)) Require(!dependency.StartsWith("Assets/Test/",StringComparison.Ordinal),"测试资源依赖："+path+" -> "+dependency,errors);
                foreach(Image image in root.GetComponentsInChildren<Image>(true))
                {
                    if(image.sprite != null && image.sprite.name.StartsWith("ICO-")) Require(image.type == Image.Type.Simple && image.preserveAspect,"图标比例："+path+"/"+image.name,errors);
                    if(image.name == "Dimed" || image.name == "Panel_Dim" || image.name == "Panel_Pause" || image.name.Contains("Backdrop")) Require(image.sprite == null,"遮罩不能套用按钮贴图："+path+"/"+image.name,errors);
                }
                foreach(var row in root.GetComponentsInChildren<ListRowItem>(true))
                {
                    Require(row.transform.Find("StatusGroup/Img_Status") != null,"状态节点："+path,errors);
                    Require(row.transform.Find("IconRoot").GetComponent<LayoutElement>().preferredWidth==48,"主图标48："+path,errors);
                }
                foreach(Button button in root.GetComponentsInChildren<Button>(true))
                {
                    if(!button.name.StartsWith("Tab_") && !button.name.StartsWith("Btn_Sub_") && !button.name.StartsWith("Btn_Zone_"))continue;
                    string expected=path.Contains("ArchivePage")?"SHR-028":path.Contains("HomePage")||path.Contains("MainPageShell")?"SHR-027":"SHR-026";
                    Require(button.image!=null && button.image.sprite!=null && button.image.sprite.name.StartsWith(expected),"页签族："+path+"/"+button.name,errors);
                }
            }
            foreach(string name in new[]{"ICO-063","ICO-064","ICO-057-generic","SHR-041-info","SHR-041-success","SHR-041-warning","SHR-041-danger"})Require(catalog.Get(name)!=null,"必需资源："+name,errors);
            Require(catalog.Get("anomaly:ReservedA003")==null && catalog.Get("anomaly:ReservedA007")==null,"保留异常未绑定",errors);
            Directory.CreateDirectory("Library/FormalResourceValidation");
            string report="prefabs="+prefabs+"; semanticEntries="+sprites+"; errors="+errors.Count+"\n"+string.Join("\n",errors);
            File.WriteAllText("Library/FormalResourceValidation/assets.txt",report);
            if(errors.Count>0)throw new InvalidOperationException(report);
            Debug.Log("[FormalResources][Assets] PASS "+report);
        }
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/准备运行验收")]
        public static void Prepare()
        {
            UIValidationHarness.ConfigureSmokeTest(1080,1920,"Library/FormalResourceValidation/1080x1920");
        }
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/验收720x1280")]
        public static void Size720() => Capture(720,1280);
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/验收1170x2532")]
        public static void Size1170() => Capture(1170,2532);
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/验收1440x2560")]
        public static void Size1440() => Capture(1440,2560);
        private static void Capture(int width,int height)
        {
            UIValidationHarness.ConfigureCapture(width,height,"Library/FormalResourceValidation/"+width+"x"+height,true);
            Screen.SetResolution(width,height,false); UIValidationHarness.StartAllPagesSmokeTest();
        }
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/验证特殊条目")]
        public static void Special() => SpecialItemRuntimeValidation.Run();
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/验证导航视觉状态")]
        public static void Visual() => FormalVisualRuntimeValidation.Run();
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/验证运行组件")]
        public static void Runtime()
        {
            ListRowRuntimeValidation.Run(AssetDatabase.LoadAssetAtPath<GameObject>(ListRowPrefabMigration.RowPath));
            FormalResourceRuntimeValidation.Run(AssetDatabase.LoadAssetAtPath<UIFormalSpriteCatalog>(FormalResourceMigration.CatalogPath));
        }
        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/删除零引用淘汰资源")]
        public static void Retire()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("停止 Play Mode 后删除淘汰资源。");
            string[] targets=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Game/Sprites/UI"}).Select(AssetDatabase.GUIDToAssetPath)
                .Where(p=>Path.GetFileName(p).StartsWith("SHR-027-badge")||Path.GetFileName(p).StartsWith("SHR-047")).ToArray();
            var dependencies=new List<string>();
            foreach(string path in AssetDatabase.GetAllAssetPaths())
            {
                if(!path.StartsWith("Assets/",StringComparison.Ordinal)||targets.Contains(path)||Directory.Exists(path))continue;
                string ext=Path.GetExtension(path);
                if(ext==".png"||ext==".cs"||ext==".dll"||ext==".meta"||ext==".ttf")continue;
                foreach(string target in targets)if(AssetDatabase.GetDependencies(path,true).Contains(target))dependencies.Add(path+" -> "+target);
            }
            if(dependencies.Count>0)throw new InvalidOperationException("仍有引用，拒绝删除：\n"+string.Join("\n",dependencies));
            foreach(string target in targets)if(!AssetDatabase.DeleteAsset(target))throw new InvalidOperationException("无法删除："+target);
            AssetDatabase.SaveAssets(); Debug.Log("[FormalResources][Retire] PASS zero dependencies; deleted="+targets.Length);
        }
        private static void Require(bool value,string message,List<string> errors){if(!value)errors.Add(message);}
    }
}
