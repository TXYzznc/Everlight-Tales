using System.Collections.Generic;
using Everlight.Tales.Board;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 盘面渲染视图（P1-014／P1-015）。程序化摆放 37 格边界与在盘实体的表现块，
    /// 布局由 <see cref="HexLayout"/> 换算，配色由 <see cref="EntityVisuals"/> 决定。
    /// 视图只读模型：外部调 <see cref="Refresh"/> 同步；本类不做结算。
    /// </summary>
    public sealed class HexBoardView : MonoBehaviour
    {
        [SerializeField] private float m_CellSize = 40f;

        private readonly List<GameObject> _cells = new List<GameObject>();
        private readonly Dictionary<int, GameObject> _entityTiles = new Dictionary<int, GameObject>();

        /// <summary>当前已渲染的格子数。</summary>
        public int CellCount => _cells.Count;

        /// <summary>当前已渲染的实体数。</summary>
        public int EntityCount => _entityTiles.Count;

        public void Refresh(BoardState board)
        {
            if (board == null)
            {
                return;
            }

            ClearChildren();
            _cells.Clear();
            _entityTiles.Clear();

            foreach (HexCoord cell in HexGrid.Enumerate(board.SideLength))
            {
                _cells.Add(CreateTile(
                    "cell_" + cell.Q + "_" + cell.R,
                    HexLayout.AxialToPixel(cell, m_CellSize),
                    new Color(0.10f, 0.11f, 0.13f, 1f),
                    0.86f));
            }

            foreach (BoardEntity entity in board.Entities)
            {
                GameObject tile = CreateTile(
                    "entity_" + entity.Id,
                    HexLayout.AxialToPixel(entity.Coord, m_CellSize),
                    EntityVisuals.GetColor(entity),
                    0.60f);
                _entityTiles.Add(entity.Id, tile);
            }
        }

        public bool TryGetEntityTile(int id, out GameObject tile)
        {
            return _entityTiles.TryGetValue(id, out tile);
        }

        private GameObject CreateTile(string name, Vector2 position, Color color, float fillRatio)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            float size = m_CellSize * fillRatio;
            rt.sizeDelta = new Vector2(size, size);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
