using System.IO;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace KaitoKid.ArchipelagoUtilities.Net.ItemSprites
{
    public class ItemSprite
    {
        private readonly ILogger _logger;
        public string Game { get; }
        public string Item { get; }
        public string FilePath { get; }
        public string FileName { get; }

        public ItemSprite(ILogger logger, string filePath)
        {
            _logger = logger;
            FilePath = filePath;
            var fileInfo = new FileInfo(filePath);
            FileName = fileInfo.Name;
            FileName = FileName.Substring(0, FileName.Length - fileInfo.Extension.Length);
            var parts = FileName.Split('_');

            if (parts.Length == 1)
            {
                Game = parts[0];
                Item = string.Empty;
                return;
            }

            if (parts.Length < 2)
            {
                _logger.LogError("Found a custom sprite with 3 parts instead of 2");
            }

            Game = parts.First();
            Item = string.Join("_", parts.Skip(1));
        }
    }
}
