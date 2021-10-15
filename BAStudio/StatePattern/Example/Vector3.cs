namespace statepatterncsharp.BAStudio.StatePattern.Example
{
    public struct Vector3
    {
        
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void SetY (float y) => this.y = y;

        public static Vector3 operator + (Vector3 left, Vector3 right) 
        {
            return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
        }
        public static Vector3 operator - (Vector3 left, Vector3 right) 
        {
            return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
        }
    }

}