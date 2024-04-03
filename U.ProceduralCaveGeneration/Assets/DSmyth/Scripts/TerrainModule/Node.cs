using UnityEngine;

namespace DSmyth.TerrainModule {
    public class Node {
        public Vector3 Position;
        public int VertexIndex = -1;

        public Node(Vector3 _pos) {
            Position = _pos;
        }
    }
}