using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule {
    public class SquareGrid {
        public Square[,] Squares;

        public List<Vector3> Vertices = new List<Vector3>();
        public List<int> Triangles = new List<int>();
        public Dictionary<int, List<Triangle>> TriangleDict = new Dictionary<int, List<Triangle>>();

        public SquareGrid(int[,] map, float squareSize) {

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
            Squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++) {
                for (int y = 0; y < nodeCountY - 1; y++) {
                    Squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }


            Vertices.Clear();
            Triangles.Clear();
            TriangleDict.Clear();
            // Calculate data needed to create meshes
            for (int x = 0; x < Squares.GetLength(0); x++) {
                for (int y = 0; y < Squares.GetLength(1); y++) {
                    Square currentSquare = Squares[x, y];
                    //currentSquare.Interpolate(isoValue);
                    TriangulateSquare(currentSquare);
                }
            }
        }

        /// <summary>
        /// Calculates the uv texture coordinates by converting the Vertices array into a Vector2 array
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetUVs() {
            List<Vector2> verticesV2 = new List<Vector2>();
            for (int i = 0; i < Vertices.Count; i++) {
                //verticesV2.Add((Vector2)Vertices[i]);
                verticesV2.Add(new Vector2(Vertices[i].x, Vertices[i].z));
            }
            return verticesV2.ToArray();
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
                if (points[i].VetexIndex == -1) {
                    points[i].VetexIndex = Vertices.Count;
                    Vertices.Add(points[i].Position);
                }
            }
        }

        private void CreateTriangle(Node a, Node b, Node c) {
            Triangles.Add(a.VetexIndex);
            Triangles.Add(b.VetexIndex);
            Triangles.Add(c.VetexIndex);

            Triangle triangle = new Triangle(a.VetexIndex, b.VetexIndex, c.VetexIndex);
            AddTriangleToDictionary(triangle.VertexIndexA, triangle);
            AddTriangleToDictionary(triangle.VertexIndexB, triangle);
            AddTriangleToDictionary(triangle.VertexIndexC, triangle);

        }

        private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle) {
            if (TriangleDict.ContainsKey(vertexIndexKey)) {
                TriangleDict[vertexIndexKey].Add(triangle);
            } else {
                List<Triangle> triangleList = new List<Triangle> {
                    triangle
                };
                TriangleDict.Add(vertexIndexKey, triangleList);
            }
        }
    }
}