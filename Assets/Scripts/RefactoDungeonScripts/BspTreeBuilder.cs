using VTools.RandomService;
using UnityEngine;

namespace Components.ProceduralGeneration.BSP
{
    public class BspTreeBuilder
    {
        private readonly BspDungeonParameters _p;
        private readonly RandomService _rnd;

        public BspTreeBuilder(BspDungeonParameters parameters, RandomService rnd)
        {
            _p = parameters;
            _rnd = rnd;
        }

        public BSPNode BuildRoot(int gridWidth, int gridHeight)
        {
            int w = Mathf.Clamp(_p.dungeonWidth, 1, gridWidth);
            int h = Mathf.Clamp(_p.dungeonHeight, 1, gridHeight);
            return new BSPNode(new RectInt(0, 0, w, h));
        }

        public void SplitRecursive(BSPNode node, int iteration)
        {
            if (iteration >= _p.maxIterations) return;

            var b = node.bounds;
            bool canH = b.height >= _p.minRoomSize * 2;
            bool canV = b.width >= _p.minRoomSize * 2;
            if (!canH && !canV) return;

            bool splitH = (canH && canV) ? _rnd.Chance(0.5f) : canH;

            if (splitH)
            {
                int splitY = _rnd.Range(b.yMin + _p.minRoomSize, b.yMax - _p.minRoomSize);
                node.left = new BSPNode(new RectInt(b.xMin, b.yMin, b.width, splitY - b.yMin));
                node.right = new BSPNode(new RectInt(b.xMin, splitY, b.width, b.yMax - splitY));
            }
            else
            {
                int splitX = _rnd.Range(b.xMin + _p.minRoomSize, b.xMax - _p.minRoomSize);
                node.left = new BSPNode(new RectInt(b.xMin, b.yMin, splitX - b.xMin, b.height));
                node.right = new BSPNode(new RectInt(splitX, b.yMin, b.xMax - splitX, b.height));
            }

            SplitRecursive(node.left, iteration + 1);
            SplitRecursive(node.right, iteration + 1);
        }
    }
}