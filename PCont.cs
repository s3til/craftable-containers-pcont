using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using System.Reflection;
using Range = SemanticVersioning.Range;


namespace _PCont;

// This record holds the various properties for your mod
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.s3til.pcont";
    public override string Name { get; init; } = "PCont";
    public override string Author { get; init; } = "s3til";
    public override SemanticVersioning.Version Version { get; init; } = new("1.1.0");
    public override Range SptVersion { get; init; } = new("4.0.7"); //todo figure out how to make tis not brick the mod every update
    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; } = true;
    public override Dictionary<string, Range>? ModDependencies { get; init; } = new()
    {
        { "com.wtt.commonlib", new Range("2.0.5") }
    };
    public override string? Url { get; init; }
    public override List<string>? Contributors { get; init; }
    public override List<string>? Incompatibilities { get; init; }
}

/// <summary>
/// Feel free to use this as a base for your mod
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class PCOn(WTTServerCommonLib.WTTServerCommonLib wttCommon, ISptLogger<PCOn> logger, DatabaseService databaseService) : IOnLoad
{
    public async Task OnLoad()
    {

        // Get your current assembly
        var assembly = Assembly.GetExecutingAssembly();

        // Use WTT-CommonLib services
        await wttCommon.CustomHideoutRecipeService.CreateHideoutRecipes(assembly);
        //add barter
        EditTraders();
        //message success
        logger.Success("craftable containers loaded successfully. happy crafting!");

        await Task.CompletedTask;
    }
private void EditTraders() //SVM taught me this one :) thanks GhostFenixx for all your hard work!!
    {
        MongoId uid = new(); //im too busy to make a new MongoID

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
                    Template = ItemTpl.SECURE_CONTAINER_ALPHA //accept ALPHA container in barter
                }
            }
       };
        var traders = databaseService.GetTraders();
        traders["5ac3b934156ae10c4430e83c"].Assort.Items.Add(item);
        traders["5ac3b934156ae10c4430e83c"].Assort.BarterScheme.Add(uid, barter);
        traders["5ac3b934156ae10c4430e83c"].Assort.LoyalLevelItems.Add(uid, 1);
        //add to all three needed areas (rando 5ac thing is Ragman's trader id)


    }
}