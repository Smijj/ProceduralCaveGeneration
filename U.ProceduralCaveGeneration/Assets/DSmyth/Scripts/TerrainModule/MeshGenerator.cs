using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule {
    public class MeshGenerator : MonoBehaviour {

        [Header("Settings")]
        //[SerializeField] private int m_MapSize = 10;
        //[SerializeField] private int m_IsoValue = 1;
        [SerializeField] private int m_SquareSize = 1;
        [SerializeField] private bool m_Is3D = true;

        [Header("Elements")]
        [SerializeField] private MeshFilter m_MeshFilter;
        [SerializeField] private MeshFilter m_WallMeshFilter;
        
        private SquareGrid m_SquareGrid;
        private List<List<int>> m_Outlines = new List<List<int>>();
        private HashSet<int> m_CheckedVertices = new HashSet<int>();

        [Header("Debug")]
        [SerializeField] private Mesh m_Mesh;
        [SerializeField] private Mesh m_WallMesh;
        [SerializeField] private bool m_DrawGizmos = false;


        private void Start() {
            if (!m_MeshFilter) m_MeshFilter = GetComponent<MeshFilter>();
            if (!m_Mesh) m_Mesh = GetComponent<Mesh>();
            m_Mesh = new Mesh();
        }

        private void OnDrawGizmos() {
            if (!Application.isPlaying || !m_DrawGizmos) return;

            for (int x = 0; x < m_SquareGrid.Squares.GetLength(0); x++) {
                for (int y = 0; y < m_SquareGrid.Squares.GetLength(1); y++) {
                    
                    Gizmos.color = m_SquareGrid.Squares[x, y].TopLeft.Active ? Color.black : Color.white;
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].TopLeft.Position, Vector3.one * 0.4f);

                    Gizmos.color = m_SquareGrid.Squares[x, y].TopRight.Active ? Color.black : Color.white;
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].TopRight.Position, Vector3.one * 0.4f);

                    Gizmos.color = m_SquareGrid.Squares[x, y].BottomRight.Active ? Color.black : Color.white;
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].BottomRight.Position, Vector3.one * 0.4f);

                    Gizmos.color = m_SquareGrid.Squares[x, y].BottomLeft.Active ? Color.black : Color.white;
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].BottomLeft.Position, Vector3.one * 0.4f);

                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].CentreTop.Position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].CentreRight.Position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].CentreBottom.Position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(m_SquareGrid.Squares[x, y].CentreLeft.Position, Vector3.one * 0.15f);

                }
            }
        }

        public void GenerateMesh(int[,] map) {

            m_Outlines.Clear();
            m_CheckedVertices.Clear();

            m_SquareGrid = new SquareGrid(map, m_SquareSize);

            m_Mesh = new Mesh();
            m_Mesh.vertices = m_SquareGrid.Vertices.ToArray();
            m_Mesh.triangles = m_SquareGrid.Triangles.ToArray();
            m_Mesh.uv = m_SquareGrid.GetUVs();
            m_Mesh.RecalculateNormals();

            m_MeshFilter.mesh = m_Mesh;

            if (m_Is3D) {
                CreateWallMesh();
            } else {
                Generate2DColliders();
            }
        }

        private void CreateWallMesh() {
            CalculateMeshOutlines();

            List<Vector3> wallVertices = new List<Vector3>();
            List<int> wallTriangles = new List<int>();
            m_WallMesh = new Mesh();
            float wallHeight = 1f;

            foreach (List<int> outline in m_Outlines) {
                for (int i = 0; i < outline.Count - 1; i++) {
                    int startIndex = wallVertices.Count;
                    wallVertices.Add(m_SquareGrid.Vertices[outline[i]]/* + Vector3.up * wallHeight / 2*/);      // top Left vertex
                    wallVertices.Add(m_SquareGrid.Vertices[outline[i + 1]]/* + Vector3.up * wallHeight / 2*/);  // top right vertex
                    wallVertices.Add(m_SquareGrid.Vertices[outline[i]] - Vector3.up * wallHeight);      // bottom left vertex
                    wallVertices.Add(m_SquareGrid.Vertices[outline[i + 1]] - Vector3.up * wallHeight);  // bottom right vertex

                    wallTriangles.Add(startIndex + 0);
                    wallTriangles.Add(startIndex + 2);
                    wallTriangles.Add(startIndex + 3);

                    wallTriangles.Add(startIndex + 3);
                    wallTriangles.Add(startIndex + 1);
                    wallTriangles.Add(startIndex + 0);
                }
            }
            m_WallMesh.vertices = wallVertices.ToArray();
            m_WallMesh.triangles = wallTriangles.ToArray();
            //m_WallMesh.RecalculateNormals();

            List<Vector2> verticesV2 = new List<Vector2>();
            for (int i = 0; i < wallVertices.Count; i++) {
                //verticesV2.Add((Vector2)wallVertices[i]);
                verticesV2.Add(new Vector2(wallVertices[i].x, wallVertices[i].y));
            }
            m_WallMesh.uv = verticesV2.ToArray();

            m_WallMeshFilter.mesh = m_WallMesh;
        }

        private void Generate2DColliders() {

            // Clear Colliders
            EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
            for (int i = 0; i < currentColliders.Length; i++) {
                Destroy(currentColliders[i]);
            }

            CalculateMeshOutlines();

            foreach (List<int> outline in m_Outlines) {
                EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
                Vector2[] edgePoints = new Vector2[outline.Count];

                for (int i = 0; i < outline.Count; i++) {
                    //edgePoints[i] = (Vector2)m_SquareGrid.Vertices[outline[i]];
                    edgePoints[i] = new Vector2(m_SquareGrid.Vertices[outline[i]].x, m_SquareGrid.Vertices[outline[i]].z);
                }
                edgeCollider.points = edgePoints;
            }
        }

        private void CalculateMeshOutlines() {
            for (int vertexIndex = 0; vertexIndex < m_SquareGrid.Vertices.Count; vertexIndex++) {
                if (m_CheckedVertices.Contains(vertexIndex)) continue;

                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1) {
                    m_CheckedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int> { vertexIndex };
                    m_Outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, m_Outlines.Count - 1);  // Recursively finds the outline to an area of the map
                    m_Outlines[m_Outlines.Count - 1].Add(vertexIndex);  // after the outline has been found, re-add the first vertex of this outline back to the list, to complete the loop.
                }
            }
        }

        private void FollowOutline(int vertexIndex, int outlineIdex) {
            m_Outlines[outlineIdex].Add(vertexIndex);
            m_CheckedVertices.Add(vertexIndex);
            int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
            if (nextVertexIndex != -1) {
                FollowOutline(nextVertexIndex, outlineIdex);
            }
        }

        private int GetConnectedOutlineVertex(int vertexIndex) {
            List<Triangle> trianglesContainingVertex = m_SquareGrid.TriangleDict[vertexIndex];

            for (int i = 0; i < trianglesContainingVertex.Count; i++) {
                Triangle triangle = trianglesContainingVertex[i];

                for (int j = 0; j < 3; j++) {
                    int vertexB = triangle[j];
                    if (vertexB == vertexIndex || m_CheckedVertices.Contains(vertexB)) continue;
                    if (IsOutlineEdge(vertexIndex, vertexB)) {
                        return vertexB;
                    }
                }
            }

            return -1;
        }

        private bool IsOutlineEdge(int vertexA, int vertexB) {
            List<Triangle> trianglesContainingVertexA = m_SquareGrid.TriangleDict[vertexA];
            int sharedTriangleCount = 0;

            for (int i = 0; i < trianglesContainingVertexA.Count; i++) {
                if (trianglesContainingVertexA[i].Contains(vertexB)) {
                    sharedTriangleCount++;
                    if (sharedTriangleCount > 1) {
                        break;
                    }
                }
            }
            return sharedTriangleCount == 1;
        }
    }
}