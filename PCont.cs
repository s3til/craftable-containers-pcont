using Microsoft.AspNetCore.Razor.TagHelpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.IO;
using System.Reflection;
using Path = System.IO.Path;
using Range = SemanticVersioning.Range;
namespace _PCont;
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.s3til.pcont";
    public override string Name { get; init; } = "PCont";
    public override string Author { get; init; } = "s3til";
    public override SemanticVersioning.Version Version { get; init; } = new("1.2.0");
    public override Range SptVersion { get; init; } = new("~4.0.8");
    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; } = true;
    public override string? Url { get; init; }
    public override List<string>? Contributors { get; init; }
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Range>? ModDependencies { get; init; }
}
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class PCon(ISptLogger<PCon> logger, DatabaseService databaseService, ModHelper modHelper, DatabaseServer databaseServer, JsonUtil jsonUtil) : IOnLoad
{
    private ModConfig? _modConfig;
    public async Task OnLoad()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblypath = modHelper.GetAbsolutePathToModFolder(assembly);
        var configPath = Path.Combine(assemblypath, "config");
        _modConfig = modHelper.GetJsonDataFromFile<ModConfig>(configPath, "barter.jsonc");
        await CreateHideoutRecipes(assembly);
        if (_modConfig.EditTrader) 
        {
            EditTraders();//add barter
            if (_modConfig.ContainerForTrade == "alpha") //check config validity
            { }
            else if (_modConfig.ContainerForTrade == "beta")
            { }
            else if (_modConfig.ContainerForTrade == "gamma")
            { }
            else if (_modConfig.ContainerForTrade == "unheardgamma")
            { }
            else
            { logger.Error("The container config is incorrect and will default to alpha"); }

            logger.Debug("Barter enabled at Ragman1");
        }
        //message success
        logger.Success("craftable containers loaded successfully. happy crafting!");
        await Task.CompletedTask;
    }

    private DatabaseTables? _database; //grabbing database for later 
    public async Task CreateHideoutRecipes(Assembly assembly)
    {
        var assemblypath = modHelper.GetAbsolutePathToModFolder(assembly);
        var recpath = Path.Combine("config", "recipes");
        var finalPath = Path.Combine(assemblypath, recpath); //get config file paths
        _database = databaseServer.GetTables(); //loadup database tables
        var recipies = new List<HideoutProduction>(); //get the recipies ready
        var configfiles = Directory.GetFiles(finalPath, "*.jsonc", SearchOption.AllDirectories); //get the configfiles
        foreach (var recipe in configfiles) //foreach recipe
        {
                var recFile = await File.ReadAllTextAsync(recipe); //read the config file
                var contcraft = jsonUtil.Deserialize<HideoutProduction>(recFile); //deserialize it
                if (contcraft != null) //make sure it isnt blank
                {
                    recipies.Add(contcraft); //add it to our list of recipies to add
                    logger.Success($"loaded {Path.GetFileName(recipe)} successfully"); //celebrate
                }
                else
                {
                    logger.Warning($"error in {Path.GetFileName(recipe)}"); //don't celebrate
                }
                if (recipies.Count == 7)
                    logger.Info("added all new crafts successfully"); //celebrate again!
            }
foreach (var container in recipies) //load up the recipies into the game
            {  
            var recipeExists = _database.Hideout.Production.Recipes != null &&
                               _database.Hideout.Production.Recipes.Any(r => r.Id == container.Id);
            if (recipeExists)
            {
                continue;
            } //make sure they dont crash the game

            _database.Hideout.Production.Recipes?.Add(container); //add your handiwork into the hideout
            }
        }
private void EditTraders() //SVM taught me this one :) thanks GhostFenixx for all your hard work!!
    {
        MongoId uid = new(); //im too busy to make a new MongoID
        string barterContainer = _modConfig.ContainerForTrade;
        barterContainer = barterContainer switch
        {
            "alpha" => "544a11ac4bdc2d470e8b456a",
            "beta" => "5857a8b324597729ab0a0e7d",
            "gamma" => "5857a8bc2459772bad15db29",
            "unheardgamma" => "665ee77ccf2d642e98220bca",
            _ => "544a11ac4bdc2d470e8b456a"
        };
        Item item = new() //item syntax
        {
            Upd = new Upd
            {
                UnlimitedCount = false,
                StackObjectsCount = 1 //sell One (1)
            },
            Id = uid,
            Template = ItemTpl.SECURE_WAIST_POUCH, //sell waist pouch
            ParentId = "hideout",
            SlotId = "hideout"
        };
        List<List<BarterScheme>> barter = //thanks to SVM source code for me realizing how the fuck to write this!! i genuinely had no idea
           new()
       {
            new List<BarterScheme>
            {
                new BarterScheme
                {
                    Count = 1, //accept One (1) alpha container
                    Template = barterContainer //accept ALPHA container in barter
                }
            }
       };
        var traders = databaseService.GetTraders();
        traders["5ac3b934156ae10c4430e83c"].Assort.Items.Add(item);
        traders["5ac3b934156ae10c4430e83c"].Assort.BarterScheme.Add(uid, barter);
        traders["5ac3b934156ae10c4430e83c"].Assort.LoyalLevelItems.Add(uid, 1);
        //add to all three needed areas (rando 5ac thing is Ragman's trader id)
    }
    public class ModConfig
    {
        public Boolean EditTrader { get; set; }
        public required string ContainerForTrade { get; set; }
    }
}