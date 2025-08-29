using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

namespace KaitoKid.ArchipelagoUtilities.Net.CustomAssets
{
    internal static class Hasher
    {
        public static MD5 MD5Hasher = MD5.Create();

        public static void HashCustomAssets()
        {
            Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "../../../"));
            HashAndWriteDirectory();
        }

        public static Dictionary<string, string> HashDirectory(string directory = "")
        {
            var hashDictionary = new Dictionary<string, string>();

            foreach (var folder in Directory.GetDirectories(Path.Combine(directory, "Custom Assets")))
            {
                foreach (var filePath in Directory.GetFiles(folder))
                {
                    hashDictionary[filePath.Replace(directory, "")] = HashFile(filePath);
                }
            }

            return hashDictionary;
        }

        public static void HashAndWriteDirectory(string directory = "")
        {
            File.WriteAllText(Path.Combine(directory, "Hash Data.txt"),
                HashDirectory(directory).HashDictionaryToString());
        }

        public static string HashFile(string filePath)
        {
            var file = File.Open(filePath, FileMode.Open);
            var hash = string.Join("", MD5Hasher.ComputeHash(file).Select(b => b.ToString("X2")));
            file.Close();
            return hash;
        }

        public static string HashDictionaryToString(this Dictionary<string, string> hashDictionary)
        {
            return string.Join("\n", hashDictionary.Select(kv => $"{kv.Key}:{kv.Value}"));
        }
    }
}