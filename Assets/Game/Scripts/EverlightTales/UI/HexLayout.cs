using Everlight.Tales.Board;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 蜂窝格轴向坐标 → 屏幕像素坐标（点顶六边形，P1-014）。
    /// 只做布局换算，供 <see cref="HexBoardView"/> 摆放 37 格与棋子；引擎相关，落 UI 层。
    /// </summary>
    public static class HexLayout
    {
        /// <summary>轴向 (q,r) → 屏幕坐标（点顶六边形，中心为原点）。</summary>
        public static Vector2 AxialToPixel(HexCoord coord, float size)
        {
            float x = size * (Mathf.Sqrt(3f) * coord.Q + Mathf.Sqrt(3f) / 2f * coord.R);
            float y = size * 1.5f * coord.R;
            return new Vector2(x, y);
        }

        /// <summary>相邻两格中心的间距（点顶六边形的水平间距）。</summary>
        public static float HorizontalSpacing(float size)
        {
            return Mathf.Sqrt(3f) * size;
        }

        /// <summary>
        /// 盘面视觉旋转角：把当前重力方向（盘内坐标）转到屏幕正下方所需的 z 旋转角。
        /// 旋转后重力始终指向屏幕下方，盘面（含格与实体）随之一同旋转，直观呈现「六相定势」旋转。
        /// </summary>
        public static float RotationAngleForGravity(HexDirection gravity)
        {
            Vector2 dir = AxialToPixel(HexDirections.Offset(gravity), 1f);
            return Vector2.SignedAngle(dir, Vector2.down);
        }
    }
}
