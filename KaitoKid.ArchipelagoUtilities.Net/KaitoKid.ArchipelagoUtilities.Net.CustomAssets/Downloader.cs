using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace KaitoKid.ArchipelagoUtilities.Net.CustomAssets
{
    public static class Downloader
    {
        public static readonly string DownloadDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ArchipelagoUtilities");

        public static readonly string CustomAssetsFolderPath = Path.Combine(DownloadDirectory, "Custom Assets");

        private const string WebDownloadUrl =
            "https://raw.githubusercontent.com/agilbert1412/ArchipelagoUtilities/main/KaitoKid.ArchipelagoUtilities.Net/KaitoKid.ArchipelagoUtilities.Net.CustomAssets";

        public static void CheckDownload()
        {
            var zipPath = Path.Combine(DownloadDirectory, "Custom Assets.zip");
            var customAssetsHashFile = Path.Combine(DownloadDirectory, "Custom Assets Hash.txt");

            if (!Directory.Exists(CustomAssetsFolderPath))
            {
                Directory.CreateDirectory(DownloadDirectory);
                Directory.CreateDirectory(CustomAssetsFolderPath);

                using (var client = new WebClient())
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    client.DownloadFile($"{WebDownloadUrl}/Custom Assets.zip", zipPath);
                }

                File.WriteAllText(customAssetsHashFile, Hasher.HashFile(zipPath));
                ZipFile.ExtractToDirectory(zipPath, CustomAssetsFolderPath);
                File.Delete(zipPath);

                return;
            }

            using (var client = new WebClient())
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;

                var githubCustomAssetFolderHashData = client.DownloadData($"{WebDownloadUrl}/Custom Assets Hash.txt");
                var githubCustomAssetFolderHash = string.Join("", githubCustomAssetFolderHashData.Select(b => (char)b));

                if (githubCustomAssetFolderHash == File.ReadAllText(customAssetsHashFile)) return;

                var localCustomAssetHashDictionary = Hasher.HashDirectory(DownloadDirectory);
                var githubCustomAssetHashesData = client.DownloadData($"{WebDownloadUrl}/Custom Assets/Hash Data.txt");
                var githubCustomAssetHashes = string.Join("", githubCustomAssetHashesData.Select(b => (char)b));
                var githubCustomAssetHashDictionary = githubCustomAssetHashes
                                                     .Split('\n')
                                                     .Select(s => s.Split(':'))
                                                     .ToDictionary(arr => arr[0], arr => arr[1]);

                var failed = false;
                foreach (var file in githubCustomAssetHashDictionary.Keys.Union(localCustomAssetHashDictionary.Keys))
                {
                    var onGithub = githubCustomAssetHashDictionary.ContainsKey(file);
                    var onLocal = localCustomAssetHashDictionary.ContainsKey(file);

                    if (onLocal && !onGithub)
                    {
                        File.Delete(Path.Combine(DownloadDirectory, file));
                        localCustomAssetHashDictionary.Remove(file);
                        continue;
                    }

                    if (onLocal && githubCustomAssetHashDictionary[file] == localCustomAssetHashDictionary[file])
                        continue;

                    try
                    {
                        var fileFullPath = Path.Combine(DownloadDirectory, file);
                        if (onLocal) File.Delete(fileFullPath);
                        client.DownloadFile($"{WebDownloadUrl}/{file}", fileFullPath);
                        localCustomAssetHashDictionary[file] = Hasher.HashFile(fileFullPath);
                    }
                    catch
                    {
                        failed = true;
                        break;
                    }
                }


                File.WriteAllText(Path.Combine(CustomAssetsFolderPath, "Hash Data.txt"),
                    localCustomAssetHashDictionary.HashDictionaryToString());

                if (failed)
                {
                    // potentially reached rate limit :(
                    Console.WriteLine("Webclient failed to complete task, will try again later");
                }
                else
                {
                    // if there wasn't a problem then that means update succeeded :seeker_pog:
                    File.WriteAllText(customAssetsHashFile, githubCustomAssetFolderHash);
                }
            }
        }

        public static string GetImageDirectory(string game, string item = "")
        {
            var gameFolder = Path.Combine(CustomAssetsFolderPath, game);
            if (!Directory.Exists(gameFolder) || !File.Exists(Path.Combine(gameFolder, $"{game}.png")))
                throw new ArgumentException($"Assets for the game [{game}] doesn't exist");

            var imageName = $"{(item == "" ? game : $"{game}_{item}")}.png";
            if (!File.Exists(Path.Combine(gameFolder, imageName))) imageName = game;

            return Path.Combine(gameFolder, imageName);
        }
    }
}