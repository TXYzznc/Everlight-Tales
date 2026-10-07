#if UNITY_EDITOR
using System.IO;
using System;
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>仅补齐缺失的规范占位资源；已有图片和 meta 原样保留。</summary>
    public static class InvestigationHotspotSpriteAssets
    {
        public const string Idle = "Assets/Game/Sprites/UI/控件/SCR-16-04-idle热点圆环.png";
        public const string Pulse = "Assets/Game/Sprites/UI/控件/SCR-16-04-pulse热点圆环.png";
        public const string Confirmed = "Assets/Game/Sprites/UI/控件/SCR-16-05-confirmed热点圆环.png";
        public const string Pressed = "Assets/Game/Sprites/UI/控件/SCR-16-06-pressed热点反馈.png";

        public static void EnsureMissing()
        {
            CreateMissing(Idle, new Color(0.66f, 0.42f, 0.78f, 0.35f), 108, false);
            CreateMissing(Pulse, new Color(0.66f, 0.42f, 0.78f, 0.42f), 119, false);
            CreateMissing(Confirmed, new Color(0.88f, 0.66f, 0.35f, 0.55f), 108, false);
            CreateMissing(Pressed, new Color(0.66f, 0.42f, 0.78f, 0.55f), 108, true);
        }

        private static void CreateMissing(string path, Color tint, float radius, bool scan)
        {
            if (File.Exists(path)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color[256 * 256];
                for (int y = 0; y < 256; y++)
                for (int x = 0; x < 256; x++)
                {
                    float distance = new Vector2(x - 127.5f, y - 127.5f).magnitude;
                    float edge = Mathf.Abs(distance - radius);
                    float coverage = Mathf.Clamp01(4.5f - edge);
                    if (scan) coverage = Mathf.Max(coverage, Mathf.Clamp01(2.5f - Mathf.Abs(distance - 82)));
                    Color pixel = tint;
                    pixel.a *= coverage;
                    pixels[y * 256 + x] = pixel;
                }
                texture.SetPixels(pixels); texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.spriteBorder = Vector4.zero;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 256;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        public static void Validate()
        {
            foreach (string path in new[] { Idle, Pulse, Confirmed, Pressed })
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (sprite == null || sprite.texture.width != 256 || sprite.texture.height != 256 ||
                    importer == null || importer.textureType != TextureImporterType.Sprite ||
                    importer.spriteImportMode != SpriteImportMode.Single || !importer.alphaIsTransparency ||
                    importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp)
                    throw new InvalidOperationException("调查图片导入不符合规范：" + path);
            }
        }
    }
}
#endif
