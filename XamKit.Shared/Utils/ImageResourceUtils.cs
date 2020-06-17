namespace XamKit
{
    public class ImageResourceUtils
    {
        public static string DefaultAssemblyName { get; private set; }
        public static string DefaultFolderPath { get; private set; }

        /// <summary>
        /// Format AssemblyName;Folder. If res: syntax used before icon, then default path is not used.
        /// </summary>
        public static void SetDefaultImageLocation(string assemblyName, string folderPath)
        {
            DefaultAssemblyName = assemblyName;
            DefaultFolderPath = folderPath;
        }
    }
}
