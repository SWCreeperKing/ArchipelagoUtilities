using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace KaitoKid.ArchipelagoUtilities.Net.CustomAssets
{
    public static class Downloader
    {
        public static readonly string DownloadDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ArchipelagoUtilities");

        public static readonly string CustomAssetsDirectory = Path.Combine(DownloadDirectory, "Custom Assets");

        private const string WEB_DOWNLOAD_URL =
            "https://raw.githubusercontent.com/agilbert1412/ArchipelagoUtilities/main/KaitoKid.ArchipelagoUtilities.Net/KaitoKid.ArchipelagoUtilities.Net.CustomAssets";

        private static Dictionary<string, string> _hashDictionary;
        private static Dictionary<string, ItemSpriteAlias[]> _cachedAliases;

        public static bool TryGetItemImagePath(out string imagePath, Func<string, ItemSpriteAlias[]> jsonToAliasConversion, string game, string item = "")
        {
            try
            {
                imagePath = GetItemImagePath(jsonToAliasConversion, game, item);
            }
            catch
            {
                imagePath = "";
                return false;
            }

            return true;
        }

        public static string GetItemImagePath(Func<string, ItemSpriteAlias[]> jsonToAliasConversion, string game, string item = "")
        {
            if (_hashDictionary is null)
            {
                GetFolderHash();
            }

            Debug.Assert(_hashDictionary != null, nameof(_hashDictionary) + " != null");
            if (!_hashDictionary.ContainsKey($"Custom Assets/{game}/{game}.png"))
            {
                throw new ArgumentException($"Assets for the game [{game}] doesn't exist");
            }

            if (!Directory.Exists(Path.Combine(CustomAssetsDirectory, game)))
            {
                Directory.CreateDirectory(Path.Combine(CustomAssetsDirectory, game));
                DownloadFile($"Custom Assets/{game}/aliases.json");
            }

            item = CheckAliases(game, item, jsonToAliasConversion);

            var imageName =
                $"{(!_hashDictionary.ContainsKey($"Custom Assets/{game}/{game}_{item}.png") ? game : $"{game}_{item}")}.png";
            var imageFile = Path.Combine(CustomAssetsDirectory, game, imageName);

            if (!File.Exists(imageFile))
            {
                DownloadFile($"Custom Assets/{game}/{imageName}");
            }

            return imageFile;
        }

        private static void GetFolderHash()
        {
            using (var client = new WebClient())
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                var githubCustomAssetHashesData =
                    client.DownloadData($"{WEB_DOWNLOAD_URL}/Custom Assets/Hash Data.txt");
                var githubCustomAssetHashes = string.Join("", githubCustomAssetHashesData.Select(b => (char)b));
                _hashDictionary = githubCustomAssetHashes
                                 .Split('\n')
                                 .Select(s => s.Split(':'))
                                 .ToDictionary(arr => arr[0], arr => arr[1]);
            }
        }

        private static void DownloadFile(string path)
        {
            using (var client = new WebClient())
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;

                var fileFullPath = Path.Combine(DownloadDirectory, path);
                client.DownloadFile($"{WEB_DOWNLOAD_URL}/{path}", fileFullPath);
                _hashDictionary[path] = Hasher.HashFile(fileFullPath);
            }
        }

        private static string CheckAliases(string game, string item, Func<string, ItemSpriteAlias[]> jsonToAliasConversion)
        {
            if (!_cachedAliases.ContainsKey(game))
            {
                var aliasPath = Path.Combine(DownloadDirectory, game, "aliases.json");
                _cachedAliases[game] = jsonToAliasConversion(File.ReadAllText(aliasPath));
            }
            
            return _cachedAliases[game].Any(alias => alias.ContainsAlias(item))
                ? _cachedAliases[game].First(alias => alias.ContainsAlias(item)).AliasName
                : item;
        }
    }
}