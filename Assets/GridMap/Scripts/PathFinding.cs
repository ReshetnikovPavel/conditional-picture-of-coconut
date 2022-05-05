using System.Collections.Generic;
using System.Linq;
using Extensions;
using Unity.Mathematics;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;

namespace GridTools
{
    public class PathFinding
    {
        private const int MoveStraightCost = 10;
        private const int MoveDiagonalCost = 14;

        private struct PathNode
        {
            public int X;
            public int Y;

            public int Index;

            public int GCost;
            public int HCost;
            public int FCost;

            public bool IsWalkable;

            public int PreviousIndex;

            public void UpdateFCost()
                => FCost = GCost + HCost;
        }

        private static int GetIndex(int x, int y, int gridSize)
            => x + y * gridSize;

        private static int GetDistanceCost(int2 start, int2 end)
        {
            var xDistance = math.abs(start.x - end.x);
            var yDistance = math.abs(start.y - end.y);
            var remainingDistance = math.abs(xDistance - yDistance);
            return MoveDiagonalCost * math.min(xDistance, yDistance)
                   + MoveStraightCost * remainingDistance;
        }

        private static bool IsInsideGrid(int2 position, int2 gridSize)
            => position.x >= 0 && position.y >= 0 &&
               position.x < gridSize.x && position.y < gridSize.y;

        private int GetLowestCostFNodeIndex(SortedSet<int> openNodes,
            IReadOnlyList<PathNode> pathNodeArray)
        {
            var lowestCostNode = pathNodeArray[openNodes.Min];
            foreach (var testNode in openNodes
                         .Select(openNode => pathNodeArray[openNode])
                         .Where(testNode => testNode.FCost < lowestCostNode.FCost))
            {
                lowestCostNode = testNode;
            }

            return lowestCostNode.Index;
        }

        private static PathNode[] GetPathNodeArray(Grid grid, int2 gridSize, int2 endPosition)
        {
            var pathNodeArray = new PathNode[gridSize.x * gridSize.y];

            for (var x = 0; x < gridSize.x; x++)
            {
                for (var y = 0; y < gridSize.y; y++)
                {
                    var pathNode = new PathNode
                    {
                        X = x,
                        Y = y,
                        Index = GetIndex(x, y, gridSize.x),
                        GCost = int.MaxValue,
                        HCost = GetDistanceCost(new int2(x, y), endPosition),
                        FCost = int.MaxValue,
                        IsWalkable = grid.isWalkable(x, y),
                        PreviousIndex = -1
                    };
                    pathNodeArray[pathNode.Index] = pathNode;
                }
            }

            return pathNodeArray;
        }

        private static List<int2> GetNeighboringPositions()
        {
            var neighboringPosition = new List<int2>();
            for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
                if (dx != 0 || dy != 0)
                    neighboringPosition.Add(new int2(dx, dy));

            return neighboringPosition;
        }

        private static void OpenNode(PathNode toOpenNode, int2 neighborNode,
            int2 gridSize, ICollection<int> visited, PathNode[] pathNodeArray, ISet<int> openNodes, int maxDeep)
        {
            var nextToOpenPosition = new int2(toOpenNode.X + neighborNode.x, toOpenNode.Y + neighborNode.y);
            var toOpenPosition = new int2(toOpenNode.X, toOpenNode.Y);
            var nextToOpenIndex = GetIndex(nextToOpenPosition.x, nextToOpenPosition.y, gridSize.x);
            if (!IsInsideGrid(nextToOpenPosition, gridSize) || visited.Contains(nextToOpenIndex))
                return;

            var nextToOpenNode = pathNodeArray[nextToOpenIndex];
            if (!nextToOpenNode.IsWalkable)
                return;

            var tentativeGCost = toOpenNode.GCost + GetDistanceCost(toOpenPosition, nextToOpenPosition);
            if (tentativeGCost >= nextToOpenNode.GCost || tentativeGCost > maxDeep * MoveStraightCost)
                return;

            nextToOpenNode.PreviousIndex = toOpenNode.Index;
            nextToOpenNode.GCost = tentativeGCost;
            nextToOpenNode.UpdateFCost();
            pathNodeArray[nextToOpenIndex] = nextToOpenNode;

            openNodes.Add(nextToOpenNode.Index);
        }

        private static List<int2> GetFullPath(IReadOnlyList<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.PreviousIndex == -1)
                return null;
            var result = new List<int2> {new int2(endNode.X, endNode.Y)};
            var currentNode = endNode;
            while (currentNode.PreviousIndex != -1)
            {
                currentNode = pathNodeArray[currentNode.PreviousIndex];
                result.Add(new int2(currentNode.X, currentNode.Y));
            }

            return result;

        }

        public static List<int2> GetClearPath(List<int2> path)
        {
            var result = new List<int2> {path[0]};
            var lastDirection = path[1] - path[0];
            for (var i = 1; i < path.Count; i++)
            {
                if (lastDirection.IsEqual(path[i] - path[i - 1])) continue;
                lastDirection = path[i] - path[i - 1];
                result.Add(path[i-1]);
            }

            if (lastDirection.IsEqual(path[path.Count - 1] - path[path.Count - 2]))
                result.Add(path.Last());

            return result;
        }

        public List<int2> FindPathAStar(Grid grid, int2 startPosition, int2 endPosition, int maxDeep=15)
        {
            var gridSize = new int2(grid.Width, grid.Height);

            if (!IsInsideGrid(startPosition, gridSize) || !IsInsideGrid(endPosition, gridSize))
                return null;

            var pathNodeArray = GetPathNodeArray(grid, gridSize, endPosition);
            var neighboringPosition = GetNeighboringPositions();
            var endNodeIndex = GetIndex(endPosition.x, endPosition.y, gridSize.x);

            var startNode = pathNodeArray[GetIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.GCost = 0;
            startNode.UpdateFCost();
            pathNodeArray[startNode.Index] = startNode;

            var openNodes = new SortedSet<int>();
            var visited = new HashSet<int>();

            openNodes.Add(startNode.Index);
            while (openNodes.Count > 0)
            {
                var toOpenNode = pathNodeArray[GetLowestCostFNodeIndex(openNodes, pathNodeArray)];

                if (toOpenNode.Index == endNodeIndex)
                    break;

                openNodes.Remove(toOpenNode.Index);
                visited.Add(toOpenNode.Index);

                foreach (var neighborNode in neighboringPosition)
                    OpenNode(toOpenNode, neighborNode, gridSize, visited, pathNodeArray, openNodes, maxDeep);
            }

            var endNode = pathNodeArray[endNodeIndex];
            var resultPath = GetFullPath(pathNodeArray, endNode);
            resultPath?.Reverse();

            return resultPath;
        }
    }
}