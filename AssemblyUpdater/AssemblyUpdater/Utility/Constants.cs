namespace AssemblyUpdater.Utility
{
    public static class Constants
    {
        public static readonly string NO_ASSEMBLY_VERSION = "0.0.0";
        public static readonly string SOLUTION_REGEX = "*.sln";
        public static readonly string[] FILE_NAMES_CHECK = new string[]
{
            "AssemblyInfo.cs"
};

        public static class Messages
        {
            public static readonly string VERSIONS_MISMATCH = "There is a version mismatch between: {0}";
            public static readonly string NO_SLN_FILES = "The selected directory does not contain .sln files";
            public static readonly string VERSIONS_MISMATCH_FILE = "There is a version mismatch in file {0} between AssemblyVersion: {1} and AssemblyFileVersion: {2}";
            public static readonly string NO_ASSEMBLY_INFO = "File: {0} does not contain any AssemblyInfo";
            public static readonly string INCORRECT_PATH = "Path {0} is not correct correct";
            public static readonly string ASSEMBLIES_ALREADY_UPDATED = "The assemblies are already updated. No changes are necessary.";
            public static readonly string OPERATION_COMPLETED = "Operation completed successfully.";
            public static readonly string CANNOT_READ_ASSEMBLY = "Cannot read current assembly version.";


    }


    }
}
