using UnityEngine;

namespace DSmyth.TerrainModule {

    public class ControlNode : Node {
        public bool Active;
        public float Value;
        public Node Above, Right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
            Active = _active;
            Above = new Node(Position + Vector3.forward * squareSize / 2f);
            Right = new Node(Position + Vector3.right * squareSize / 2f);
        }
    }
    
}