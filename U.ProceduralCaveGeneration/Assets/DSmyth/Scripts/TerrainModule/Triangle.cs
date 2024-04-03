namespace DSmyth.TerrainModule {
    public struct Triangle {
        public int VertexIndexA;
        public int VertexIndexB;
        public int VertexIndexC;
        int[] Vertices;

        public Triangle(int a, int b, int c) {
            VertexIndexA = a; VertexIndexB = b; VertexIndexC = c;

            Vertices = new int[3];
            Vertices[0] = a;
            Vertices[1] = b;
            Vertices[2] = c;
        }

        public int this[int i] {
            get {
                return Vertices[i];
            }
        }

        public bool Contains(int vertexIndex) {
            return vertexIndex == VertexIndexA || vertexIndex == VertexIndexB || vertexIndex == VertexIndexC;
        }
    }
}