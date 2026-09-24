using Everlight.Tales.Board;
using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 时段条（P3-006，D-069）：顶部常驻，四时段横向排列、当前高亮、白昼/夜晚底色区分。
    /// 左侧「第N天 · 时段名」、右侧「剩N/4」。数据源为 Board 层 TimeState（运行时时间）。
    /// </summary>
    public sealed class TimePeriodBar : MonoBehaviour
    {
        public TextMeshProUGUI DayPeriodLabel;
        public TextMeshProUGUI RemainingLabel;
        public Image[] PeriodCells;

        public static Color DayActive = new Color(1f, 0.88f, 0.4f);
        public static Color DayIdle = new Color(0.72f, 0.70f, 0.58f);
        public static Color NightActive = new Color(0.42f, 0.44f, 0.72f);
        public static Color NightIdle = new Color(0.20f, 0.21f, 0.30f);

        public void Build()
        {
            DayPeriodLabel = MakeText("day-period");
            RemainingLabel = MakeText("remaining");
            PeriodCells = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("period-" + i, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                PeriodCells[i] = go.GetComponent<Image>();
            }
        }

        public void Refresh(TimeState time)
        {
            if (DayPeriodLabel != null)
            {
                DayPeriodLabel.text = "第" + time.Day + "天 · " + TimePeriod.DisplayName(time.Period);
            }

            if (RemainingLabel != null)
            {
                RemainingLabel.text = "剩" + time.RemainingCells + "/" + TimeState.CellsPerPeriod;
            }

            for (int i = 0; i < 4; i++)
            {
                if (PeriodCells[i] == null)
                {
                    continue;
                }

                bool active = i == (int)time.Period;
                bool day = TimePeriod.IsDaylight((TimeOfDay)i);
                PeriodCells[i].color = active ? (day ? DayActive : NightActive) : (day ? DayIdle : NightIdle);
            }
        }

        private TextMeshProUGUI MakeText(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            return go.GetComponent<TextMeshProUGUI>();
        }
    }
}
