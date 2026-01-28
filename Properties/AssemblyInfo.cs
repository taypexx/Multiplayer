using System.Reflection;
using MelonLoader;
using Constants = Multiplayer.Static.Constants;

[assembly: MelonInfo(typeof(Multiplayer.Main), Constants.ModName, Constants.Version, Constants.Authors, $"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/home")]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
[assembly: MelonColor(255, 255, 0, 255)]
[assembly: MelonAuthorColor(255, 255, 0, 127)]
[assembly: MelonAdditionalDependencies("FavGirl")]
[assembly: MelonIncompatibleAssemblies("PracticeMod")]

[assembly: AssemblyTitle(Constants.ModName)]
[assembly: AssemblyDescription("Multiplayer client mod")]
[assembly: AssemblyCompany("Muse Dash Modding Community")]
[assembly: AssemblyProduct(Constants.ModName)]
[assembly: AssemblyCopyright("© " + Constants.Authors + " 2026")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]

[assembly: AssemblyVersion(Constants.Version + ".0")]
[assembly: AssemblyFileVersion(Constants.Version + ".0")]