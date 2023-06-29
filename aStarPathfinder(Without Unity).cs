using System;
using System.Collections.Generic;

namespace nimragAstar
{
    public class aStarPathfinder
    {
        int[,] tileArray;

        int tilesWidth;
        int tilesHeight;

        Dictionary<Vec2Int, dNode> OpenNodes;

        Vec2Int start;
        Vec2Int end;

        static bool UseDiagonals = true;

        static bool ShowPathfindingInfo = true;

        //can be used with or without an int array of the room's cells, however is much slower without as it has to calculate the array itself
        public List<Vec2Int> Pathfind(Vec2Int startPoint, Vec2Int endPoint, int[,] tileData)
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

            List<Vec2Int> path = TracePath(current);


            DateTime after = DateTime.Now; // getting a second datetime to compare

            TimeSpan duration = after.Subtract(before);
            if (ShowPathfindingInfo)
            {
                Console.WriteLine("Path Found in " + (float)duration.Ticks / (float)TimeSpan.TicksPerSecond + "s");
            }


            return path;
        }

        void Initialise(Vec2Int startPoint, Vec2Int endPoint, int[,] tileData)
        {
            tileArray = tileData.Clone() as int[,]; // must clone as is reference type

            tilesWidth = tileArray.GetLength(0); // storing the width and heigh so we dont overl
            tilesHeight = tileArray.GetLength(1);

            OpenNodes = new Dictionary<Vec2Int, dNode>(); //initialise the OpenNodes List

            start = startPoint;
            end = endPoint;
        }

        dNode CalcNextNode(dNode last) //given the last node, calculate the best node next to it
        {
            GetNodesAround(last);

            Vec2Int lowCostIndex = Vec2Int.zero;
            uint lowCost = 8008135; //high number so it will always get overwritten

            foreach (Vec2Int key in OpenNodes.Keys) //loops through the node dict until it finds the node with the lowest cost
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

            if (lowCostIndex == Vec2Int.zero) { return null; }

            return OpenNodes[lowCostIndex];
        }

        dNode[] GetNodesAround(dNode node)
        {
            List<dNode> newNodes = new List<dNode>();
            foreach (Vec2Int n in neighboursNormal)
            {
                Vec2Int pos = node.Pos + n;
                pos.x = Math.Clamp(pos.x, 0, tilesWidth);
                pos.y = Math.Clamp(pos.y, 0, tilesHeight);
                if (tileArray[pos.x, pos.y] != 0) { continue; }
                newNodes.Add(MakeNode(pos, node));
            }
            if (UseDiagonals)
            {
                foreach (Vec2Int n in neighboursDiagonal)
                {
                    Vec2Int pos = node.Pos + n;
                    if (!OpenNodes.ContainsKey(new Vec2Int(pos.x, node.Pos.y)) && !OpenNodes.ContainsKey(new Vec2Int(node.Pos.x, pos.y))) { continue; } //checking adjacent nodes to ensure it wont go through diagonal walls
                    pos.x = Math.Clamp(pos.x, 0, tilesWidth);
                    pos.y = Math.Clamp(pos.y, 0, tilesHeight);
                    if (tileArray[pos.x, pos.y] != 0) { continue; }
                    newNodes.Add(MakeNode(pos, node));
                }
            }


            return newNodes.ToArray();
        }

        dNode MakeNode(Vec2Int pos, dNode root) // create a new node and add it to the open node array
        {
            dNode d = new dNode(pos, start, end, root);
            OpenNodes.Add(pos, d);
            tileArray[pos.x, pos.y] = 0;
            return d;
        }

        List<Vec2Int> TracePath(dNode node) //get the final node and trace it back to the start
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

            List<Vec2Int> pathInts = new List<Vec2Int>();

            for (int i = 1; i < path.Count; i++)
            {
                pathInts.Add(path[i].Pos);
            }

            return pathInts;
        }

        private readonly Vec2Int[] neighboursNormal =
        {
            new Vec2Int(0,1),
            new Vec2Int(1,0),
            new Vec2Int(0,-1),
            new Vec2Int(-1,0)

        };

        private readonly Vec2Int[] neighboursDiagonal =
        {
            new Vec2Int(1,1),
            new Vec2Int(1,-1),
            new Vec2Int(-1,-1),
            new Vec2Int(-1,1)
        };

        public class dNode
        {
            public dNode Root { get; }  //the parent node that this node was created from
            public Vec2Int Pos { get; }  //position this node is at
            public uint dStart { get; } //distance from start
            public uint dEnd { get; }  //distance from end
            public uint Cost => dStart + dEnd;

            public dNode(Vec2Int pos, Vec2Int start, Vec2Int end, dNode root)
            {
                Pos = pos;
                dStart = (uint)Vec2Int.Distance(pos, start);
                dEnd = (uint)Vec2Int.Distance(pos, end);
                Root = root;
            }
        }
    }

    public struct Vec2Int : IEquatable<Vec2Int>
    {
        public int x;
        public int y;

        public Vec2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public bool Equals(Vec2Int other)
        {
            return Equals(other, this);
        }


        public static float Distance(Vec2Int a, Vec2Int b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            return (float)Math.Sqrt(num * num + num2 * num2);
        }

        public static readonly Vec2Int zero = new Vec2Int(0, 0);

        public static bool operator ==(Vec2Int c1, Vec2Int c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Vec2Int c1, Vec2Int c2)
        {
            return !c1.Equals(c2);
        }

        public static Vec2Int operator +(Vec2Int c1, Vec2Int c2)
        {
            return new Vec2Int(c1.x+c2.x, c1.y+c2.y);
        }

    }
}
