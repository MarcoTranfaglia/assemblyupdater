namespace AssemblyUpdater.Utility
{
    public static class Constants
    {
        public static readonly string SOLUTION_REGEX = "*.sln";
        public static readonly string VERSION_REGEX = @"[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{5}";
        public static readonly string[] FILE_NAMES_CHECK = new string[]
{
            "AssemblyInfo.cs",
};
    }
}
