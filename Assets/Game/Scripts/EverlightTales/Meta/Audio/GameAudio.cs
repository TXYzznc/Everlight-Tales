namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 音频统一入口（P0-008）。三分组：Music（背景音乐）/ Sound（音效）/ UISound（界面音）。
    ///
    /// 通路（4.2）：本门面经框架 GF.Sound 播放到指定分组；分组在 PreloadProcedure 依据
    /// SoundGroupTable 注册（本批次已增补 UISound 行）。
    ///
    /// 音量/静音（4.3）：直接读写 GF.Sound 对应分组的 Volume/Mute，只影响目标分组、不影响其它分组。
    ///
    /// 持久化边界（4.5）：本批次仅内存态——Set* 不写入 SettingComponent，
    /// 跨启动保留留给 b03 存档服务批次（design D6）。
    ///
    /// 占位音（4.4）：Play* 传入的 assetName 经框架解析为 Assets/Game/Audios/&lt;assetName&gt; 路径；
    /// 素材缺失时框架记录警告并安全忽略（返回 0），Play Mode 走查用占位音即可，不依赖正式音频素材。
    /// </summary>
    public static class GameAudio
    {
        public static int PlayMusic(string assetName) => Play(Const.SoundGroup.Music, assetName);
        public static int PlaySound(string assetName) => Play(Const.SoundGroup.Sound, assetName);
        public static int PlayUISound(string assetName) => Play(Const.SoundGroup.UISound, assetName);

        public static void SetMusicVolume(float volume) => SetVolume(Const.SoundGroup.Music, volume);
        public static void SetSoundVolume(float volume) => SetVolume(Const.SoundGroup.Sound, volume);
        public static void SetUISoundVolume(float volume) => SetVolume(Const.SoundGroup.UISound, volume);

        public static float GetMusicVolume() => GetVolume(Const.SoundGroup.Music);
        public static float GetSoundVolume() => GetVolume(Const.SoundGroup.Sound);
        public static float GetUISoundVolume() => GetVolume(Const.SoundGroup.UISound);

        public static void SetMusicMuted(bool muted) => SetMuted(Const.SoundGroup.Music, muted);
        public static void SetSoundMuted(bool muted) => SetMuted(Const.SoundGroup.Sound, muted);
        public static void SetUISoundMuted(bool muted) => SetMuted(Const.SoundGroup.UISound, muted);

        public static bool IsMusicMuted() => IsMuted(Const.SoundGroup.Music);
        public static bool IsSoundMuted() => IsMuted(Const.SoundGroup.Sound);
        public static bool IsUISoundMuted() => IsMuted(Const.SoundGroup.UISound);

        private static int Play(Const.SoundGroup group, string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
            {
                return 0;
            }
            return GF.Sound.PlayEffect(assetName, group.ToString());
        }

        private static void SetVolume(Const.SoundGroup group, float volume)
        {
            var soundGroup = GF.Sound.GetSoundGroup(group.ToString());
            if (soundGroup != null)
            {
                soundGroup.Volume = volume;
            }
        }

        private static float GetVolume(Const.SoundGroup group)
        {
            var soundGroup = GF.Sound.GetSoundGroup(group.ToString());
            return soundGroup != null ? soundGroup.Volume : 0f;
        }

        private static void SetMuted(Const.SoundGroup group, bool muted)
        {
            var soundGroup = GF.Sound.GetSoundGroup(group.ToString());
            if (soundGroup != null)
            {
                soundGroup.Mute = muted;
            }
        }

        private static bool IsMuted(Const.SoundGroup group)
        {
            var soundGroup = GF.Sound.GetSoundGroup(group.ToString());
            return soundGroup != null && soundGroup.Mute;
        }
    }
}
