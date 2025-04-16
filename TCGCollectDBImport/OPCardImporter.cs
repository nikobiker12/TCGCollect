using FluentStorage;
using FluentStorage.Blobs;
using System.Text.Json;
using TCGCollect.Core;

namespace TCGCollect.Importer.OP
{
     public class OPCardImporter
    {
        private readonly IBlobStorage _storage;
        private readonly string _container;

        public OPCardImporter(string connectionString, string key, string containerName)
        {
            StorageFactory.Modules.UseAzureBlobStorage();
            // Instanciation du storage pour Azure Blob avec FluentStorage.
            // La méthode exacte peut varier selon la version utilisée.
            _storage = StorageFactory.Blobs.AzureBlobStorageWithSharedKey(connectionString, key);
            _container = containerName;
        }

        /// <summary>
        /// Parcourt les fichiers JSON du "dossier" spécifié dans le conteneur et traite chacun d'eux.
        /// </summary>
        public async Task Import()
        {
            // Récupère la liste des fichiers dans le dossier indiqué.
            // Si folder est une chaîne vide, listera tous les fichiers du conteneur.
            var fileList = await _storage.ListAsync(new ListOptions {FolderPath = _container});

            // Filtre pour ne traiter que les fichiers avec l'extension .json.
            List<Card> cards = new ();
            foreach (var file in fileList)
            {
                // Read the content of each JSON file
                using var stream = await _storage.OpenReadAsync(file);
                using JsonDocument jsonDoc = await JsonDocument.ParseAsync(stream);
                JsonElement root = jsonDoc.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in root.EnumerateArray())
                    {
                        cards.Add(BuildCard(element));
                    }
                }
            }

            using FileStream fs = new FileStream(
                @"c:\dev\opcg-data\cards-imported.json",
                FileMode.Create, // Creates a new file or overwrites an existing file.
                FileAccess.Write,
                FileShare.None
            );
            await JsonSerializer.SerializeAsync(fs, cards, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /* 
    {
        "id": "OP04-119_p1",
        "code": "OP04-119",
        "rarity": "SP CARD",
        "role": "PERSONNAGE",
        "name": "Don Quijote Rosinante",
        "image": "../images/cardlist/card/OP04-119_p1.webp?250214",
        "image_alt": "Don Quijote Rosinante",
        "cost": "8",
        "attribute": "Spécial",
        "power": "8000",
        "counter": "-",
        "color": "Vert",
        "feature": "Marine/Équipage de Don Quijote",
        "effect": "[Tour adverse] Si ce Personnage est épuisé, vos Personnages redressés ayant un coût de base de 5 ne peuvent pas être mis KO par un effet. [Jouée] Vous pouvez épuiser ce Personnage : Jouez jusqu'à 1 carte Personnage verte de votre main ayant un coût de 5.",
        "extension": "-LES NOUVEAUX EMPEREURS- [OP-09]",
        "lang": "fr",
        "set": "op-09"
    },
        */

        /// <summary>
        /// Traitement de l'objet Card importé.
        /// </summary>
        /// <param name="card">L'objet Card issu de la désérialisation du fichier JSON.</param>
        private Card BuildCard(JsonElement element)
        {
            CardPart part = new();
            CardFace face = new();
            face.Parts = new [] {part};
            Card card = new();
            card.Faces = new[] {face};
            card.Game = "OP";

            card.Id = element.GetProperty("id").GetString();
            card.Language = element.GetProperty("lang").GetString();
            card.SetName = element.GetProperty("set").GetString();
            card.Number = element.GetProperty("code").GetString();
            card.Rarity = element.GetProperty("rarity").GetString();
            part.Name = element.GetProperty("name").GetString();
            part.Type = element.GetProperty("role").GetString();
            part.ImageSrc = element.GetProperty("image").GetString();
            part.CardColors = new [] {element.GetProperty("color").GetString()};
            part.PrintedName = element.GetProperty("name").GetString();
            part.PrintedText = element.GetProperty("effect").GetString();
            part.SubTypes = element.GetProperty("feature").GetString();

            // Traitez votre objet Card ici selon vos besoins.
            Console.WriteLine($"Carte importée : {card.Id}");

            return card;
        }
    }
}

