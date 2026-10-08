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
        [SerializeField] public TextMeshProUGUI DayPeriodLabel;
        [SerializeField] public TextMeshProUGUI RemainingLabel;
        [SerializeField] public Image[] PeriodCells;
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;

        public static Color DayActive = new Color(1f, 0.88f, 0.4f);
        public static Color DayIdle = new Color(0.72f, 0.70f, 0.58f);
        public static Color NightActive = new Color(0.42f, 0.44f, 0.72f);
        public static Color NightIdle = new Color(0.20f, 0.21f, 0.30f);

        public void BindStaticLayout()
        {
            if (DayPeriodLabel == null) DayPeriodLabel = FindText("Txt_DayPeriod");
            if (RemainingLabel == null) RemainingLabel = FindText("Txt_Remaining");
            if (PeriodCells == null || PeriodCells.Length != 4) PeriodCells = new Image[4];
            for (int i = 0; i < PeriodCells.Length; i++)
            {
                Transform cell = transform.Find("Cell_" + i);
                if (PeriodCells[i] == null) PeriodCells[i] = cell != null ? cell.GetComponent<Image>() : null;
            }
        }

        public void Build()
        {
            if (DayPeriodLabel == null || RemainingLabel == null || PeriodCells == null || PeriodCells.Length != 4)
                Debug.LogError("TimePeriodBar 静态布局不完整，拒绝运行时创建 UI。", this);
        }

        private TextMeshProUGUI FindText(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<TextMeshProUGUI>() : null;
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
                PeriodCells[i].sprite = _spriteCatalog != null ? _spriteCatalog.Get("SHR-036-" + (day ? "day-" : "night-") + (active ? "active" : "idle")) : null;
                PeriodCells[i].color = Color.white;
            }
        }

    }
}
