#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Everlight.Tales.UI.Editor
{
    public static class MapPanelContractMigration
    {
        private const string PrefabPath="Assets/Game/Prefabs/UI/MapPage.prefab";
        [MenuItem("Game Framework/EverlightTales/UI/绑定 MapPanel 序列化引用")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root=PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                MapPanel panel=root.GetComponentInChildren<MapPanel>(true); if(panel==null)throw new System.InvalidOperationException("MapPage.prefab 中缺少 MapPanel。");
                Transform layout=panel.transform.name=="Panel_Map"?panel.transform:panel.transform.Find("Panel_Map"); Transform time=Find(layout,"Panel_TimeBar"); Transform map=Find(layout,"Panel_MapView"); Transform place=Find(layout,"Panel_Place"); Transform viewport=Find(place,"List_Event"); Transform content=Find(viewport,"EventContent")??viewport; Transform template=Find(content,"EventCardTemplate");
                SerializedObject so=new SerializedObject(panel); Set(so,"m_LayoutRoot",layout); Set(so,"m_TimeRoot",time); Set(so,"m_MapRoot",map); Set(so,"m_TimeBar",time==null?null:time.GetComponent<TimePeriodBar>()); Set(so,"m_MapView",map==null?null:map.GetComponent<CityMapView>()); Set(so,"m_TrackLabel",Text(layout,"Txt_Track")); Set(so,"m_PlaceLabel",Text(place,"Txt_PlaceLabel")); Set(so,"m_PlaceDesc",Text(place,"Txt_PlaceDesc")); Set(so,"m_EventListRoot",content); Set(so,"m_WaitButton",Button(place,"Btn_Wait")); Set(so,"_eventItemTemplate",AssetDatabase.LoadAssetAtPath<GameObject>(ListRowPrefabMigration.RowPath)); so.ApplyModifiedPropertiesWithoutUndo();
                TimePeriodBar bar=time==null?null:time.GetComponent<TimePeriodBar>(); if(bar!=null){SerializedObject bs=new SerializedObject(bar); Set(bs,"DayPeriodLabel",Text(time,"Txt_DayPeriod")); Set(bs,"RemainingLabel",Text(time,"Txt_Remaining")); SerializedProperty cells=bs.FindProperty("PeriodCells"); cells.arraySize=4; for(int i=0;i<4;i++) cells.GetArrayElementAtIndex(i).objectReferenceValue=Component<Image>(time,"Cell_"+i); bs.ApplyModifiedPropertiesWithoutUndo();}
                CityMapView city=map==null?null:map.GetComponent<CityMapView>(); if(city!=null){SerializedObject cs=new SerializedObject(city); Set(cs,"_nodeRoot",Find(map,"MapContent")); cs.ApplyModifiedPropertiesWithoutUndo();}
                PrefabUtility.SaveAsPrefabAsset(root,PrefabPath); Debug.Log("[MapPanelContractMigration] MapPanel 序列化引用绑定完成。");
            }
            finally{PrefabUtility.UnloadPrefabContents(root);} AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        private static Transform Find(Transform r,string p)=>r==null?null:r.Find(p);
        private static TMP_Text Text(Transform r,string p){Transform t=Find(r,p);return t==null?null:t.GetComponent<TMP_Text>();}
        private static Button Button(Transform r,string p){Transform t=Find(r,p);return t==null?null:t.GetComponent<Button>();}
        private static T Component<T>(Transform r,string p) where T:Component {Transform t=Find(r,p);return t==null?null:t.GetComponent<T>();}
        private static void Set(SerializedObject so,string n,Object v){SerializedProperty p=so.FindProperty(n);if(p==null)throw new System.InvalidOperationException("MapPanel 缺少序列化字段："+n);p.objectReferenceValue=v;}
    }
}
#endif
