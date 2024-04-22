using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule {
    public class SquareGrid {
        
        public Square[,] Squares => m_Squares;
        private Square[,] m_Squares;

        public List<Vector3> Vertices => m_Vertices;
        private List<Vector3> m_Vertices = new List<Vector3>();
        public List<int> Triangles => m_Triangles;
        private List<int> m_Triangles = new List<int>();
        public Dictionary<int, List<Triangle>> TriangleDict => m_TriangleDict;
        private Dictionary<int, List<Triangle>> m_TriangleDict = new Dictionary<int, List<Triangle>>();
        public Vector2[] UVs => m_UVs;
        private Vector2[] m_UVs;

        // Constructor
        public SquareGrid(int[,] map, float squareSize, int uvTileAmount = 1) {

            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            // Create all the control nodes (corner vertices)
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    // Calculate Position of current control node
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            // Create all the squares in the grid
            m_Squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++) {
                for (int y = 0; y < nodeCountY - 1; y++) {
                    m_Squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }


            // Calculate the Vertices and Triangles. This data is needed to create meshes
            m_Vertices.Clear();
            m_Triangles.Clear();
            m_TriangleDict.Clear();
            for (int x = 0; x < m_Squares.GetLength(0); x++) {
                for (int y = 0; y < m_Squares.GetLength(1); y++) {
                    Square currentSquare = m_Squares[x, y];
                    //currentSquare.Interpolate(isoValue);
                    TriangulateSquare(currentSquare);
                }
            }

            // Create UV's for texturing
            m_UVs = new Vector2[m_Vertices.Count];
            for (int i = 0; i < m_Vertices.Count; i++) {
                float percentX = Mathf.InverseLerp(-nodeCountX / 2 * squareSize, nodeCountX / 2 * squareSize, m_Vertices[i].x) * uvTileAmount;
                float percentY = Mathf.InverseLerp(-nodeCountY / 2 * squareSize, nodeCountY / 2 * squareSize, m_Vertices[i].z) * uvTileAmount;
                m_UVs[i] = new Vector2(percentX, percentY);
            }
        }


        private void TriangulateSquare(Square square) {
            switch (square.Configuration) {
                case 0:
                    break;

                /// 1 points
                // Bottom Left
                case 1:
                    MeshDataFromPoints(square.CentreLeft, square.CentreBottom, square.BottomLeft);
                    break;
                // Bottom Right
                case 2:
                    MeshDataFromPoints(square.BottomRight, square.CentreBottom, square.CentreRight);
                    break;
                // Top Right
                case 4:
                    MeshDataFromPoints(square.TopRight, square.CentreRight, square.CentreTop);
                    break;
                // Top Left
                case 8:
                    MeshDataFromPoints(square.TopLeft, square.CentreTop, square.CentreLeft);
                    break;

                /// 2 points
                // Bottom Left & Bottom Right
                case 3:
                    MeshDataFromPoints(square.CentreRight, square.BottomRight, square.BottomLeft, square.CentreLeft);
                    break;
                // Top Right & Bottom Right
                case 6:
                    MeshDataFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.CentreBottom);
                    break;
                // Top Left & Bottom Left
                case 9:
                    MeshDataFromPoints(square.TopLeft, square.CentreTop, square.CentreBottom, square.BottomLeft);
                    break;
                // Top Left & Top Right
                case 12:
                    MeshDataFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreLeft);
                    break;

                // Bottom Left & Top Right
                case 5:
                    MeshDataFromPoints(square.CentreTop, square.TopRight, square.CentreRight, square.CentreBottom, square.BottomLeft, square.CentreLeft);
                    break;
                // Top Left & Bottom Right
                case 10:
                    MeshDataFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight, square.CentreBottom, square.CentreLeft);
                    break;

                /// 3 points
                // Top Right & Bottom Right & Bottom Left
                case 7:
                    MeshDataFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.BottomLeft, square.CentreLeft);
                    break;
                // Top Left & Bottom Right & Bottom Left
                case 11:
                    MeshDataFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight, square.BottomLeft);
                    break;
                // Top Left & Top Right & Bottom Left
                case 13:
                    MeshDataFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreBottom, square.BottomLeft);
                    break;
                // Top Left & Top Right & Bottom Right
                case 14:
                    MeshDataFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CentreBottom, square.CentreLeft);
                    break;

                /// 4 points
                // All vertices
                case 15:
                    MeshDataFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
                    // Optimization for if the code for calculating the mesh's outlines is moved into this class
                    //m_Checkedm_Vertices.Add(square.TopLeft.VertexIndex);
                    //m_Checkedm_Vertices.Add(square.TopRight.VertexIndex);
                    //m_Checkedm_Vertices.Add(square.BottomRight.VertexIndex);
                    //m_Checkedm_Vertices.Add(square.BottomLeft.VertexIndex);
                    break;
            }
        }

        private void MeshDataFromPoints(params Node[] points) {
            AssignVertices(points);

            for (int i = 0; i < points.Length - 2; i++)
                CreateTriangle(points[0], points[i + 1], points[i + 2]);
        }

        private void AssignVertices(Node[] points) {
            for (int i = 0; i < points.Length; i++) {
                // Only add the vertice if it hasnt already been added
                if (points[i].VertexIndex == -1) {
                    points[i].VertexIndex = m_Vertices.Count;
                    m_Vertices.Add(points[i].Position);
                }
            }
        }

        private void CreateTriangle(Node a, Node b, Node c) {
            m_Triangles.Add(a.VertexIndex);
            m_Triangles.Add(b.VertexIndex);
            m_Triangles.Add(c.VertexIndex);

            Triangle triangle = new Triangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
            AddTriangleToDictionary(triangle.VertexIndexA, triangle);
            AddTriangleToDictionary(triangle.VertexIndexB, triangle);
            AddTriangleToDictionary(triangle.VertexIndexC, triangle);

        }

        private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
            if (m_TriangleDict.ContainsKey(vertexIndexKey)) {
                m_TriangleDict[vertexIndexKey].Add(triangle);
            } else {
                List<Triangle> triangleList = new List<Triangle> {
                    triangle
                };
                m_TriangleDict.Add(vertexIndexKey, triangleList);
            }
        }
    }
}