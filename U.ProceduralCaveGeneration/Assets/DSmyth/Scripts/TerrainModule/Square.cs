namespace DSmyth.TerrainModule {
    public class Square {
        public ControlNode TopRight, BottomRight, BottomLeft, TopLeft;
        public Node CentreTop, CentreRight, CentreBottom, CentreLeft;
        public int Configuration;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft) {
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
            TopLeft = topLeft;

            CentreTop = TopLeft.Right;
            CentreRight = BottomRight.Above;
            CentreBottom = BottomLeft.Right;
            CentreLeft = BottomLeft.Above;

            if (topLeft.Active)
                Configuration += 8;
            if (topRight.Active)
                Configuration += 4;
            if (bottomRight.Active)
                Configuration += 2;
            if (bottomLeft.Active)
                Configuration += 1;
        }

        //public void Interpolate(float isoValue) {

        //    float topLerp = Mathf.InverseLerp(TopLeft.Value, TopRight.Value, isoValue);
        //    CentreTop.Position = TopLeft.Position + (TopRight.Position - TopLeft.Position) * topLerp;

        //    float rightLerp = Mathf.InverseLerp(TopRight.Value, BottomRight.Value, isoValue);
        //    CentreRight.Position = TopRight.Position + (BottomRight.Position - TopRight.Position) * rightLerp;

        //    float bottomLerp = Mathf.InverseLerp(BottomLeft.Value, BottomRight.Value, isoValue);
        //    CentreBottom.Position = BottomLeft.Position + (BottomRight.Position - BottomLeft.Position) * bottomLerp;

        //    float leftLerp = Mathf.InverseLerp(TopLeft.Value, BottomLeft.Value, isoValue);
        //    CentreLeft.Position = TopLeft.Position + (BottomLeft.Position - TopLeft.Position) * leftLerp;
        //}
    }
}