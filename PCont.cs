using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;


namespace _PCont;

// This record holds the various properties for your mod
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.s3til.pcont";
    public override string Name { get; init; } = "PCont";
    public override string Author { get; init; } = "s3til";
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.1");
    public override Range SptVersion { get; init; } = new("4.0.7");
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
public class PCOn(
    WTTServerCommonLib.WTTServerCommonLib wttCommon
) : IOnLoad
{
    public async Task OnLoad()
    {

        // Get your current assembly
        var assembly = Assembly.GetExecutingAssembly();




        // Use WTT-CommonLib services
        await wttCommon.CustomHideoutRecipeService.CreateHideoutRecipes(assembly);

        await Task.CompletedTask;
    }
}
