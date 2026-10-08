using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class FormalTopBar : MonoBehaviour
    {
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        [SerializeField] private Image _image;
        private int _period = -1;
        private void LateUpdate()
        {
            if (WorldSession.Current == null || _period == (int)WorldSession.Current.Time.Period) return;
            _period = (int)WorldSession.Current.Time.Period;
            _image.sprite = _spriteCatalog.Get(TimePeriod.IsDaylight((TimeOfDay)_period) ? "SHR-006-daylight" : "SHR-006-night"); _image.color = Color.white;
        }
    }
}
