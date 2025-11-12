using VTools.RandomService;
using UnityEngine;

namespace Components.ProceduralGeneration.BSP
{
    public class BspTreeBuilder
    {
        private readonly int _dungeonWidth;
        private readonly int _dungeonHeight;
        private readonly int _minRoomSize;
        private readonly int _maxIterations;
        private readonly RandomService _rnd;

        public BspTreeBuilder(int dungeonWidth, int dungeonHeight, int minRoomSize, int maxIterations, RandomService rnd)
        {
            _dungeonWidth = dungeonWidth;
            _dungeonHeight = dungeonHeight;
            _minRoomSize = minRoomSize;
            _maxIterations = maxIterations;
            _rnd = rnd;
        }

        public BSPNode BuildRoot(int gridWidth, int gridHeight)
        {
            int w = Mathf.Clamp(_dungeonWidth, 1, gridWidth);
            int h = Mathf.Clamp(_dungeonHeight, 1, gridHeight);
            return new BSPNode(new RectInt(0, 0, w, h));
        }

        public void SplitRecursive(BSPNode node, int iteration)
        {
            if (iteration >= _maxIterations) return;

            var b = node.bounds;
            bool canH = b.height >= _minRoomSize * 2;
            bool canV = b.width >= _minRoomSize * 2;
            if (!canH && !canV) return;

            bool splitH = (canH && canV) ? _rnd.Chance(0.5f) : canH;

            if (splitH)
            {
                int splitY = _rnd.Range(b.yMin + _minRoomSize, b.yMax - _minRoomSize);
                node.left = new BSPNode(new RectInt(b.xMin, b.yMin, b.width, splitY - b.yMin));
                node.right = new BSPNode(new RectInt(b.xMin, splitY, b.width, b.yMax - splitY));
            }
            else
            {
                int splitX = _rnd.Range(b.xMin + _minRoomSize, b.xMax - _minRoomSize);
                node.left = new BSPNode(new RectInt(b.xMin, b.yMin, splitX - b.xMin, b.height));
                node.right = new BSPNode(new RectInt(splitX, b.yMin, b.xMax - splitX, b.height));
            }

            SplitRecursive(node.left, iteration + 1);
            SplitRecursive(node.right, iteration + 1);
        }
    }
}