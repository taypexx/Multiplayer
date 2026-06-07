using System;
using System.Reflection;
using System.Collections.Generic;

namespace inspect_cs
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.LoadFrom(@"d:\MuseDashStuff\Mod_SourceCode\MultiPlayer\Dependencies\CustomAlbums.dll");
            var type = assembly.GetType("CustomAlbums.Patches.HiddenSupportPatch");
            if (type == null) {
                Console.WriteLine("Type not found");
                return;
            }
            var field = type.GetField("LoadedHiddens", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) {
                Console.WriteLine("Field LoadedHiddens not found");
                return;
            }
            var loadedHiddens = field.GetValue(null) as HashSet<string>;
            if (loadedHiddens == null) {
                Console.WriteLine("LoadedHiddens is null");
                return;
            }
            Console.WriteLine("LoadedHiddens count: " + loadedHiddens.Count);
            foreach (var h in loadedHiddens) {
                Console.WriteLine("LoadedHidden: " + h);
            }
        }
    }
}
