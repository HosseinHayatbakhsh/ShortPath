using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShortestPath
{
    public enum VertexType
    {
        Player,
        Space,
        Wall,
        Flag
    }

    public enum VertexStatus
    {
        None = 0,
        Temporary = 1,
        Permanent = 2
    }

    public class Vertex
    {
        public Vertex Predecessor { get; set; }
        public int PathLength { get; set; }
        public VertexType Type { get; set; }
        public VertexStatus Status { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        public Vertex(char type, int rowIndex, int columnIndex)
        {
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.Predecessor = null;
            this.PathLength = ShortestPathService.Infinity;
            this.Status = VertexStatus.Temporary;
            switch (type)
            {
                case 'P':
                    this.Type = VertexType.Player;
                    this.PathLength = 0;
                    this.Status = VertexStatus.Permanent;
                    break;

                case '.':
                    this.Type = VertexType.Space;
                    break;

                case 'W':
                    this.Type = VertexType.Wall;
                    this.Status = VertexStatus.None;
                    break;

                case 'F':
                    this.Type = VertexType.Flag;
                    break;

                default:
                    throw new Exception("Invalid vertex type");
            }
        }
    }

    public class ShortestPathService
    {
        public static int Infinity = int.MaxValue;
        public const int MaxMapSize = 100;

        public string ShortestPath(string mapData)
        {
            ValidateMapData(mapData);
            Vertex[,] vertexData = GetVertexData(mapData);
            Vertex player = FindPlayer(vertexData);
            ComputePathLength(player, vertexData);

            Vertex flag = FindShortesPathFlag(vertexData);
            return TraversePath(flag);
        }

        private string TraversePath(Vertex vertex)
        {
            if (vertex.PathLength == Infinity)
            {
                throw new Exception("The vertex is not reachable");
            }

            Vertex current = vertex;
            StringBuilder path = new StringBuilder();

            while (current.Type != VertexType.Player)
            {
                Vertex predecessor = current.Predecessor;
                if (predecessor == null)
                {
                    throw new Exception("Predecessor cannot be null");
                }

                if (predecessor.ColumnIndex != current.ColumnIndex)
                {
                    if (predecessor.ColumnIndex == current.ColumnIndex - 1)
                    {
                        path.Insert(0, "R");
                    }
                    else
                    {
                        path.Insert(0, "L");
                    }
                }
                else if (predecessor.RowIndex != current.RowIndex)
                {
                    if (predecessor.RowIndex == current.RowIndex - 1)
                    {
                        path.Insert(0, "D");
                    }
                    else
                    {
                        path.Insert(0, "U");
                    }
                }
                current = current.Predecessor;
            }
            return path.ToString();
        }

        private Vertex FindShortesPathFlag(Vertex[,] vertexData)
        {
            IEnumerable<Vertex> flagVertices = (from Vertex vertex in vertexData
                                                where vertex.Type == VertexType.Flag
                                                select vertex);
            if (flagVertices.Count() == 0)
            {
                throw new Exception("No flag was not found");
            }
            int minPathLength = flagVertices.Min((vertex) => vertex.PathLength);
            return flagVertices.First((vertex) => vertex.PathLength == minPathLength);
        }

        private Vertex FindPlayer(Vertex[,] vertexData)
        {
            Vertex player = (from Vertex vertex in vertexData
                             where vertex.Type == VertexType.Player
                             select vertex).FirstOrDefault();
            if (player == null)
            {
                throw new Exception("Player was not found");
            }
            return player;
        }

        private void ComputePathLength(Vertex player, Vertex[,] vertexData)
        {
            Vertex currentVertex = player;
            while (true)
            {
                ExamineAdjacentVetices(currentVertex, vertexData);

                int count = (from Vertex vertex in vertexData
                             where vertex.Status == VertexStatus.Temporary && vertex.PathLength != Infinity
                             select vertex).Count();
                if (count == 0)
                {
                    break;
                }
                currentVertex = FindMinimumPathTempVertex(currentVertex, vertexData);
            }
        }

        private void ExamineAdjacentVetices(Vertex currentVertex, Vertex[,] vertexData)
        {
            int rowLength = vertexData.GetLength(0);
            int columnLength = vertexData.GetLength(1);
            List<Vertex> adjacentVertices = new List<Vertex>();

            // Left adjacent vertex
            if (currentVertex.ColumnIndex > 0)
            {
                Vertex adjacentVertex = vertexData[currentVertex.RowIndex, currentVertex.ColumnIndex - 1];
                if (adjacentVertex.Status == VertexStatus.Temporary)
                {
                    adjacentVertices.Add(adjacentVertex);
                }
            }

            // Right adjacent vertex
            if (columnLength > (currentVertex.ColumnIndex + 1))
            {
                Vertex adjacentVertex = vertexData[currentVertex.RowIndex, currentVertex.ColumnIndex + 1];
                if (adjacentVertex.Status == VertexStatus.Temporary)
                {
                    adjacentVertices.Add(adjacentVertex);
                }
            }

            // Up adjacent vertex
            if (currentVertex.RowIndex > 0)
            {
                Vertex adjacentVertex = vertexData[currentVertex.RowIndex - 1, currentVertex.ColumnIndex];
                if (adjacentVertex.Status == VertexStatus.Temporary)
                {
                    adjacentVertices.Add(adjacentVertex);
                }
            }

            // Down adjacent vertex
            if (rowLength > (currentVertex.RowIndex + 1))
            {
                Vertex adjacentVertex = vertexData[currentVertex.RowIndex + 1, currentVertex.ColumnIndex];
                if (adjacentVertex.Status == VertexStatus.Temporary)
                {
                    adjacentVertices.Add(adjacentVertex);
                }
            }

            foreach (Vertex adjacentVertex in adjacentVertices)
            {
                if (currentVertex.PathLength + 1 < adjacentVertex.PathLength)
                {
                    adjacentVertex.PathLength = currentVertex.PathLength + 1;
                    adjacentVertex.Predecessor = currentVertex;
                }
            }
        }

        private Vertex FindMinimumPathTempVertex(Vertex currentVertex, Vertex[,] vertexData)
        {
            int minPathLength = Infinity;

            for (int rowIndex = 0; rowIndex < vertexData.GetLength(0); rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < vertexData.GetLength(1); columnIndex++)
                {
                    Vertex vertex = vertexData[rowIndex, columnIndex];
                    if (vertex.Status == VertexStatus.Temporary && vertex.PathLength < minPathLength)
                    {
                        minPathLength = vertex.PathLength;
                        currentVertex = vertex;
                    }
                }
            }
            currentVertex.Status = VertexStatus.Permanent;
            return currentVertex;
        }

        private Vertex[,] GetVertexData(string mapData)
        {
            string[] mapDataRows = mapData.Split(';');
            Vertex[,] vertexData = new Vertex[mapDataRows.Length, mapDataRows[0].Length];

            for (int rowIndex = 0; rowIndex < mapDataRows.Length; rowIndex++)
            {
                string mapDataRow = mapDataRows[rowIndex];
                for (int columnIndex = 0; columnIndex < mapDataRow.Length; columnIndex++)
                {
                    vertexData[rowIndex, columnIndex] = new Vertex(mapDataRow[columnIndex], rowIndex, columnIndex);
                }
            }

            return vertexData;
        }

        private void ValidateMapData(string mapData)
        {
            if (string.IsNullOrEmpty(mapData))
            {
                throw new ArgumentException("mapData cannot be null or empty");
            }

            if (!Regex.IsMatch(mapData, "^[W.PF;]*$"))
            {
                throw new ArgumentException("mapData can only contain these characters: 'W', '.', 'P', 'F', ';'");
            }

            if (mapData.Count(ch => ch == 'P') != 1)
            {
                throw new ArgumentException("mapData must contain only one player");
            }

            if (mapData.Count(ch => ch == 'F') == 0)
            {
                throw new ArgumentException("mapData does not include any flag to reach out");
            }

            string[] mapDataRows = mapData.Split(';');
            if (mapDataRows.Any(dr => dr.Length != mapDataRows[0].Length))
            {
                throw new ArgumentException("All rows of mapData should have the same length");
            }

            if (mapDataRows.Length > MaxMapSize || mapDataRows[0].Length > MaxMapSize)
            {
                throw new ArgumentException($"The maximum allowed size of a the map is {MaxMapSize}x{MaxMapSize}");
            }

        }
    }

    [TestClass]
    public class ShortestPathTest
    {
        [TestMethod]
        public void WhenMapDataIsNullThrowException()
        {
            string mapData = "..F;PW.;...";

            string expectedPath = "URR";

            string actual = (new ShortestPathService()).ShortestPath(mapData);

            Assert.AreEqual(actual, expectedPath);
        }
    }
}
