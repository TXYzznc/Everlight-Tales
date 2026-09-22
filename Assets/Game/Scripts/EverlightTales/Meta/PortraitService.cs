using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 立绘状态服务（P5-008）：人物登场/事件对话/情感动作的状态切换。
    /// 纯逻辑（Meta 仅引用 Data）：只维护「当前人物 + 场景 + 情感」状态，
    /// 由 UI 层在状态变化时挂接真实立绘素材播放。
    /// </summary>
    public sealed class PortraitService
    {
        public string CurrentCharacter { get; private set; }

        public PortraitScene Scene { get; private set; }

        public PortraitEmotion Emotion { get; private set; }

        public bool IsVisible { get; private set; }

        /// <summary>展示某人物立绘（登场/对话）并设置情感动作；未知人物返回 false。</summary>
        public bool Show(string characterId, PortraitScene scene, PortraitEmotion emotion = PortraitEmotion.Neutral)
        {
            if (PortraitCatalog.Get(characterId) == null)
            {
                return false;
            }

            CurrentCharacter = characterId;
            Scene = scene;
            Emotion = emotion;
            IsVisible = true;
            return true;
        }

        /// <summary>切换情感动作（须已展示某人物）。</summary>
        public bool SetEmotion(PortraitEmotion emotion)
        {
            if (!IsVisible)
            {
                return false;
            }

            Emotion = emotion;
            return true;
        }

        /// <summary>收起立绘。</summary>
        public void Hide()
        {
            IsVisible = false;
            CurrentCharacter = null;
        }

        public bool IsShowing(string characterId)
        {
            return IsVisible && CurrentCharacter == characterId;
        }
    }
}
