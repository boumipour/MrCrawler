namespace Utility.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object input)
        {
            return input == null;
        }

        public static bool IsNotNull(this object input)
        {
            return input != null;
        }
    }
}
