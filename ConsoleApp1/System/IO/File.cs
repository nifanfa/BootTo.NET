namespace System.IO
{
    public static unsafe class File
    {
        public static byte[] ReadAllBytes(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer);
            fs.Close();
            return buffer;
        }

        public static bool Exists(string path) => FileSystem.FileExists(path);

        public static void WriteAllBytes(string path, byte[] buffer)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(buffer);
            fs.Flush();
            fs.Close();
        }

        public static void Delete(string path) => FileSystem.Delete(path);
    }
}
