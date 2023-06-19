namespace Utils
{
    public static class MathUtils
    {
        public static int Modulo(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}