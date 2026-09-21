using Everlight.Tales.Board;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 盘面页 UIForm（b42）：把 BoardGame 接入正式 UIForm 流程，
    /// 程序化装配盘面视图、六分区 HUD 与操作按钮（左旋／右旋／拍击）。
    /// 进入后加载教学样张第一段（旋转与碰撞）作为可玩盘面，
    /// 拍击结算后由 <see cref="BoardPage"/> 展示过轮结果。
    /// </summary>
    public sealed class BoardPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "BoardPage";

        private BoardPage m_Page;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            BuildTutorialBoard();
        }

        private void BuildTutorialBoard()
        {
            // 教学样张第一段：固定墙 + 撞锤 + 来撞件，2 拍 / 目标 15 分。
            TutorialStage stage = TutorialSample.RotateAndCollide();
            BoardGame game = new BoardGame(stage.Board, stage.Settle, stage.Session, stage.Level);

            m_Page = gameObject.AddComponent<BoardPage>();
            m_Page.Bind(game);
        }
    }
}
