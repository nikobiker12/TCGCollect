using TCGCollect.Importer.OP;

OPCardImporter cardImporter = new OPCardImporter(
    "tcgcollectstgaccount", 
    "xx",
    "raw-db-op");

await cardImporter.Import();

Console.WriteLine("Data Imported !");
