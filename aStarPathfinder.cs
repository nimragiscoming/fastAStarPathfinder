using System;
using System.Collections.Generic;
using UnityEngine;

namespace nimragAstar
{
    public class aStarPathfinder
    {
        int[,] tileArray;

        int tilesWidth;
        int tilesHeight;

        Dictionary<Vector2Int, dNode> OpenNodes;

        Vector2Int start;
        Vector2Int end;

        static bool UseDiagonals = true;

        static bool ShowPathfindingInfo = true;

        //can be used with or without an int array of the room's cells, however is much slower without as it has to calculate the array itself
        public List<Vector2Int> Pathfind(Vector2Int startPoint, Vector2Int endPoint, int[,] tileData)
        {
            DateTime before = DateTime.Now; // logging how long it takes to run

            Initialise(startPoint, endPoint, tileData); // assign the data

            dNode startNode = new dNode(start, start, end, null); // turn the start coord into a dNode

            dNode current = startNode;
            while (!current.Pos.Equals(end)) // loops until it reaches the end node
            {
                OpenNodes.Remove(current.Pos);

                current = CalcNextNode(current);
                if (current == null)
                {
                    break;
                }
            }

            List<Vector2Int> path = TracePath(current);


            DateTime after = DateTime.Now; // getting a second datetime to compare

            TimeSpan duration = after.Subtract(before);
            if (ShowPathfindingInfo)
            {
                Debug.Log("Path Found in " + (float)duration.Ticks / (float)TimeSpan.TicksPerSecond + "s");
            }


            return path;
        }

        void Initialise(Vector2Int startPoint, Vector2Int endPoint, int[,] tileData)
        {
            tileArray = tileData.Clone() as int[,]; // must clone as is reference type

            tilesWidth = tileArray.GetLength(0); // storing the width and heigh so we dont overl
            tilesHeight = tileArray.GetLength(1);

            OpenNodes = new Dictionary<Vector2Int, dNode>(); //initialise the OpenNodes List

            start = startPoint;
            end = endPoint;
        }

        dNode CalcNextNode(dNode last) //given the last node, calculate the best node next to it
        {
            GetNodesAround(last);

            Vector2Int lowCostIndex = Vector2Int.zero;
            uint lowCost = 8008135; //high number so it will always get overwritten

            foreach (Vector2Int key in OpenNodes.Keys) //loops through the node dict until it finds the node with the lowest cost
            {
                if (OpenNodes[key].Cost == lowCost)
                {
                    if (OpenNodes[key].dEnd < OpenNodes[lowCostIndex].dEnd) //prioritise nodes closer to the end if node cost is equivalent
                    {
                        lowCost = OpenNodes[key].Cost;
                        lowCostIndex = key;
                        continue;
                    }
                }
                if (OpenNodes[key].Cost < lowCost)
                {
                    lowCost = OpenNodes[key].Cost;
                    lowCostIndex = key;
                }
            }

            if (lowCostIndex == Vector2Int.zero) { return null; }

            return OpenNodes[lowCostIndex];
        }

        dNode[] GetNodesAround(dNode node)
        {
            List<dNode> newNodes = new List<dNode>();
            foreach (Vector2Int n in neighboursNormal)
            {
                Vector2Int pos = node.Pos + n;
                pos.x = Mathf.Clamp(pos.x, 0, tilesWidth);
                pos.y = Mathf.Clamp(pos.y, 0, tilesHeight);
                if (tileArray[pos.x, pos.y] != 0) { continue; }
                newNodes.Add(MakeNode(pos, node));
            }
            if (UseDiagonals)
            {
                foreach (Vector2Int n in neighboursDiagonal)
                {
                    Vector2Int pos = node.Pos + n;
                    if (!OpenNodes.ContainsKey(new Vector2Int(pos.x, node.Pos.y)) && !OpenNodes.ContainsKey(new Vector2Int(node.Pos.x, pos.y))) { continue; } //checking adjacent nodes to ensure it wont go through diagonal walls
                    pos.x = Mathf.Clamp(pos.x, 0, tilesWidth);
                    pos.y = Mathf.Clamp(pos.y, 0, tilesHeight);
                    if (tileArray[pos.x, pos.y] != 0) { continue; }
                    newNodes.Add(MakeNode(pos, node));
                }
            }


            return newNodes.ToArray();
        }

        dNode MakeNode(Vector2Int pos, dNode root) // create a new node and add it to the open node array
        {
            dNode d = new dNode(pos, start, end, root);
            OpenNodes.Add(pos, d);
            tileArray[pos.x, pos.y] = 0;
            return d;
        }

        List<Vector2Int> TracePath(dNode node) //get the final node and trace it back to the start
        {
            List<dNode> path = new List<dNode>
            {
                node
            };

            dNode last = node;

            while (last != null)
            {
                if (last.Root == null) { break; }
                path.Add(last.Root);
                last = last.Root;
            }

            path.Reverse();  //the original array starts at the end node and traces back to the start, so we must reverse it

            List<Vector2Int> pathInts = new List<Vector2Int>();

            for (int i = 1; i < path.Count; i++)
            {
                pathInts.Add(path[i].Pos);
            }

            return pathInts;
        }

        private readonly Vector2Int[] neighboursNormal =
        {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(0,-1),
            new Vector2Int(-1,0)

        };

        private readonly Vector2Int[] neighboursDiagonal =
        {
            new Vector2Int(1,1),
            new Vector2Int(1,-1),
            new Vector2Int(-1,-1),
            new Vector2Int(-1,1)
        };

        public class dNode
        {
            public dNode Root { get; }  //the parent node that this node was created from
            public Vector2Int Pos { get; }  //position this node is at
            public uint dStart { get; } //distance from start
            public uint dEnd { get; }  //distance from end
            public uint Cost => dStart + dEnd;

            public dNode(Vector2Int pos, Vector2Int start, Vector2Int end, dNode root)
            {
                Pos = pos;
                dStart = (uint)Vector2Int.Distance(pos, start);
                dEnd = (uint)Vector2Int.Distance(pos, end);
                Root = root;
            }
        }
    }
}
