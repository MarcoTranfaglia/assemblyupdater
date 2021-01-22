namespace AssemblyUpdater.Utility
{
    public static class Constants
    {
        public static readonly string VERSION_TEXTBOX_REGEX = "[0-9]+\\.[0-9]+(?:\\.[0-9]+)?";
        public static readonly string FRAMEWORK_TEXTBOX_REGEX = "v[0-9]\\.[0-9]";

        public static readonly string NO_ASSEMBLY_VERSION = "0.0.0";
        public static readonly string SOLUTION_REGEX = "*.sln";
        public static readonly string[] ASSEMBLYINFO_FILES = new string[]
{
            "AssemblyInfo.cs",
            "AssemblyInfo.cpp"
};



    }
}
