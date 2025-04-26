using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace KaitoKid.ArchipelagoUtilities.Net.ItemSprites
{
    public class ArchipelagoItemSprites
    {
        private const string SPRITE_FOLDER_NAME = "Custom Assets";

        private ILogger _logger;
        private string _spritesFolder;
        private Dictionary<string, List<ItemSprite>> _spritesByGame;
        private Dictionary<string, List<ItemSprite>> _spritesByItemName;
        private Dictionary<string, Dictionary<string, ItemSprite>> _spritesByGameByItemName;

        public ArchipelagoItemSprites(ILogger logger)
        {
            _logger = logger;
            _spritesFolder = Directory.EnumerateDirectories(Directory.GetCurrentDirectory(), SPRITE_FOLDER_NAME, SearchOption.AllDirectories).FirstOrDefault();

            LoadCustomSprites();
        }

        private void LoadCustomSprites()
        {
            if (string.IsNullOrWhiteSpace(_spritesFolder))
            {
                _logger.LogError("Could not find Custom Assets Directory");
                return;
            }

            var files = Directory.EnumerateFiles(_spritesFolder, "*.png", SearchOption.AllDirectories);
            _spritesByGame = new Dictionary<string, List<ItemSprite>>();
            _spritesByItemName = new Dictionary<string, List<ItemSprite>>();
            _spritesByGameByItemName = new Dictionary<string, Dictionary<string, ItemSprite>>();
            foreach (var file in files)
            {
                var sprite = new ItemSprite(_logger, file);
                var cleanGame = CleanName(sprite.Game);
                var cleanItem = CleanName(sprite.Item);
                if (!_spritesByGame.ContainsKey(cleanGame))
                {
                    _spritesByGame.Add(cleanGame, new List<ItemSprite>());
                }
                if (!_spritesByItemName.ContainsKey(cleanItem))
                {
                    _spritesByItemName.Add(cleanItem, new List<ItemSprite>());
                }
                if (!_spritesByGameByItemName.ContainsKey(cleanGame))
                {
                    _spritesByGameByItemName.Add(cleanGame, new Dictionary<string, ItemSprite>());
                }

                _spritesByGame[cleanGame].Add(sprite);
                _spritesByItemName[cleanItem].Add(sprite);
                if (!_spritesByGameByItemName[cleanGame].ContainsKey(cleanItem))
                {
                    _spritesByGameByItemName[cleanGame].Add(cleanItem, sprite);
                }
            }
        }

        public bool TryGetCustomAsset(ScoutedLocation scoutedLocation, string myGameName, out ItemSprite sprite)
        {
            var myGame = CleanName(myGameName);
            var game = CleanName(scoutedLocation.GameName);
            var item = CleanName(scoutedLocation.ItemName);
            if (_spritesByGameByItemName.TryGetValue(game, out var itemsInCorrectGame))
            {
                if (itemsInCorrectGame.TryGetValue(item, out sprite))
                {
                    return true;
                }
            }
            if (_spritesByGameByItemName.TryGetValue(myGame, out var itemsInMyGame))
            {
                if (itemsInMyGame.TryGetValue(item, out sprite))
                {
                    return true;
                }
            }

            if (_spritesByItemName.TryGetValue(item, out var spritesWithCorrectName) && spritesWithCorrectName.Any())
            {
                var random = new Random(scoutedLocation.GetSeed());
                var index = random.Next(0, spritesWithCorrectName.Count);
                sprite = spritesWithCorrectName[index];
                return true;
            }

            if (_spritesByGameByItemName.TryGetValue(game, out itemsInCorrectGame))
            {
                if (itemsInCorrectGame.TryGetValue(string.Empty, out sprite))
                {
                    return true;
                }
            }

            sprite = null;
            return false;
        }

        public string CleanName(string name)
        {
            var charsToIgnore = new string[] { " ", "_", ":", "'" };
            name = name.ToLower();
            foreach (var charToIgnore in charsToIgnore)
            {
                name = name.Replace(charToIgnore, "");
            }

            return name;
        }
    }
}
