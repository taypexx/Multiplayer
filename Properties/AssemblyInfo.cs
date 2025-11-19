using System.Reflection;
using MelonLoader;
using Constants = Multiplayer.Static.Constants;

[assembly: MelonInfo(typeof(Multiplayer.Main), Constants.ModName, Constants.Version, Constants.Authors)]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
[assembly: MelonColor(255, 255, 0, 255)]

[assembly: AssemblyTitle(Constants.ModName)]
[assembly: AssemblyDescription("Muse Dash Multiplayer client mod that allows you to play against other players. ")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Muse Dash Modding Community")]
[assembly: AssemblyProduct(Constants.ModName)]
[assembly: AssemblyCopyright("Copyright © " + Constants.Authors + " 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion(Constants.Version + ".0")]
[assembly: AssemblyFileVersion(Constants.Version + ".0")]