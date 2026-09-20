using System;
using System.IO;
using UnityEngine;

namespace Everlight.Tales.Meta
{
    /// <summary>存档读取结果。</summary>
    public enum SaveReadStatus
    {
        /// <summary>读取成功。</summary>
        Success,
        /// <summary>存档从未存在过。</summary>
        NotFound,
        /// <summary>存档文件损坏（无法解析或结构非法）。</summary>
        Corrupt,
        /// <summary>存档版本号高于当前支持范围，被拒绝。</summary>
        VersionUnsupported,
    }

    /// <summary>三段信封：版本号 + 写入时间 + 载荷。</summary>
    public sealed class SaveEnvelope
    {
        public int version;
        public string savedAtUtc;
        public object payload;
    }

    /// <summary>
    /// 存档服务骨架：世界存档与尝试存档两个独立文件，各自带版本号。
    /// 序列化复用框架 NewtonsoftJsonHelper，落盘沿用 persistentDataPath 约定；读写时机由调用方驱动，不做隐式写盘。
    /// </summary>
    public static class SaveService
    {
        public const int WorldVersion = 1;
        public const int AttemptVersion = 1;

        private const string DirectoryName = "Save";

        public static string SaveDirectory => Path.Combine(Application.persistentDataPath, DirectoryName);
        public static string WorldPath => Path.Combine(SaveDirectory, "world.json");
        public static string AttemptPath => Path.Combine(SaveDirectory, "attempt.json");

        public static void WriteWorld<T>(T payload) => Write(WorldPath, WorldVersion, payload);
        public static SaveReadStatus ReadWorld<T>(out T payload) => Read(WorldPath, WorldVersion, out payload);

        public static void WriteAttempt<T>(T payload) => Write(AttemptPath, AttemptVersion, payload);
        public static SaveReadStatus ReadAttempt<T>(out T payload) => Read(AttemptPath, AttemptVersion, out payload);

        public static void DeleteWorld() => DeleteFile(WorldPath);
        public static void DeleteAttempt() => DeleteFile(AttemptPath);

        private static void Write<T>(string path, int version, T payload)
        {
            Directory.CreateDirectory(SaveDirectory);
            SaveEnvelope envelope = new SaveEnvelope
            {
                version = version,
                savedAtUtc = DateTime.UtcNow.ToString("O"),
                payload = payload,
            };
            string json = JsonHelper.ToJson(envelope);
            File.WriteAllText(path, json);
        }

        private static SaveReadStatus Read<T>(string path, int supportedVersion, out T payload)
        {
            payload = default;
            if (!File.Exists(path))
            {
                return SaveReadStatus.NotFound;
            }

            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch
            {
                return SaveReadStatus.Corrupt;
            }

            SaveEnvelope envelope;
            try
            {
                envelope = JsonHelper.ToObject<SaveEnvelope>(json);
            }
            catch
            {
                return SaveReadStatus.Corrupt;
            }

            if (envelope == null)
            {
                return SaveReadStatus.Corrupt;
            }

            // 版本高于支持范围：拒绝并报可辨识错误，不按当前格式静默解析。
            if (envelope.version > supportedVersion)
            {
                return SaveReadStatus.VersionUnsupported;
            }

            if (envelope.payload == null)
            {
                payload = default;
                return SaveReadStatus.Success;
            }

            try
            {
                payload = JsonHelper.ToObject<T>(JsonHelper.ToJson(envelope.payload));
            }
            catch
            {
                return SaveReadStatus.Corrupt;
            }

            return SaveReadStatus.Success;
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static NewtonsoftJsonHelper JsonHelper => new NewtonsoftJsonHelper();
    }
}
