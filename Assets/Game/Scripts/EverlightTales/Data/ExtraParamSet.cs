using System;
using System.Collections.Generic;
using System.Globalization;

namespace Everlight.Tales.Data
{
    /// <summary>逐项参数的取值类型（配置表 ExtraData 列）。</summary>
    public enum ExtraParamKind : byte
    {
        /// <summary>整数。</summary>
        Int = 0,

        /// <summary>布尔，接受 1/0 与 true/false。</summary>
        Bool = 1,

        /// <summary>文本。</summary>
        Text = 2,
    }

    /// <summary>
    /// 逐项参数集合：解析配置表 <c>ExtraData</c> 列的 <c>k=v;k=v</c> 文本。
    /// 语法错误（缺等号、空键值、重复键）在解析时抛出；键是否被该条目接受、值是否能按声明类型解析，
    /// 由 <see cref="Validate"/> 在装载配置时校验，避免拼写错误静默失效。
    /// </summary>
    public sealed class ExtraParamSet
    {
        /// <summary>空集合：没有逐项参数的条目使用。</summary>
        public static readonly ExtraParamSet Empty = new ExtraParamSet(
            new Dictionary<string, string>(0, StringComparer.Ordinal),
            Array.Empty<string>());

        private readonly Dictionary<string, string> _values;
        private readonly string[] _keys;

        private ExtraParamSet(Dictionary<string, string> values, string[] keys)
        {
            _values = values;
            _keys = keys;
        }

        /// <summary>已声明的键，按配置文本中的出现顺序。</summary>
        public IReadOnlyList<string> Keys => _keys;

        /// <summary>已声明的键数量。</summary>
        public int Count => _keys.Length;

        /// <summary>解析 <c>k=v;k=v</c> 文本；空文本或仅分号返回 <see cref="Empty"/>。</summary>
        public static ExtraParamSet Parse(string text, string owner)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Empty;
            }

            string[] segments = text.Split(';');
            var values = new Dictionary<string, string>(segments.Length, StringComparer.Ordinal);
            var keys = new List<string>(segments.Length);
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i].Trim();
                if (segment.Length == 0)
                {
                    continue;
                }

                int separator = segment.IndexOf('=');
                if (separator <= 0 || separator == segment.Length - 1)
                {
                    throw new ArgumentException(
                        string.Format("{0} 的逐项参数片段 '{1}' 不是合法的 k=v。", owner, segment), nameof(text));
                }

                string key = segment.Substring(0, separator).Trim();
                string value = segment.Substring(separator + 1).Trim();
                if (key.Length == 0 || value.Length == 0)
                {
                    throw new ArgumentException(
                        string.Format("{0} 的逐项参数片段 '{1}' 缺少键或值。", owner, segment), nameof(text));
                }

                if (!values.TryAdd(key, value))
                {
                    throw new ArgumentException(
                        string.Format("{0} 的逐项参数键 '{1}' 重复。", owner, key), nameof(text));
                }

                keys.Add(key);
            }

            return keys.Count == 0 ? Empty : new ExtraParamSet(values, keys.ToArray());
        }

        /// <summary>是否声明了该键。</summary>
        public bool Has(string key)
        {
            return _values.ContainsKey(key);
        }

        /// <summary>读取整数；未声明返回 <paramref name="fallback"/>，声明但无法解析则抛出。</summary>
        public int GetInt(string key, int fallback)
        {
            if (!_values.TryGetValue(key, out string value))
            {
                return fallback;
            }

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                throw new ArgumentException(string.Format("逐项参数 '{0}' 的值 '{1}' 不是整数。", key, value));
            }

            return parsed;
        }

        /// <summary>读取布尔；未声明返回 <paramref name="fallback"/>，声明但无法解析则抛出。</summary>
        public bool GetBool(string key, bool fallback)
        {
            if (!_values.TryGetValue(key, out string value))
            {
                return fallback;
            }

            if (value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (value == "0" || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            throw new ArgumentException(string.Format("逐项参数 '{0}' 的值 '{1}' 不是布尔。", key, value));
        }

        /// <summary>读取文本；未声明返回 <paramref name="fallback"/>。</summary>
        public string GetText(string key, string fallback)
        {
            return _values.TryGetValue(key, out string value) ? value : fallback;
        }

        /// <summary>
        /// 按条目声明的键与类型校验本集合：未声明的键、以及无法按声明类型解析的值都会抛出。
        /// </summary>
        public void Validate(string owner, IReadOnlyDictionary<string, ExtraParamKind> schema)
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                string key = _keys[i];
                if (schema == null || !schema.TryGetValue(key, out ExtraParamKind kind))
                {
                    throw new ArgumentException(string.Format("{0} 不支持逐项参数键 '{1}'。", owner, key));
                }

                try
                {
                    switch (kind)
                    {
                        case ExtraParamKind.Int:
                            GetInt(key, 0);
                            break;
                        case ExtraParamKind.Bool:
                            GetBool(key, false);
                            break;
                    }
                }
                catch (ArgumentException exception)
                { throw new ArgumentException(owner + "：" + exception.Message, exception); }
            }
        }
    }
}
