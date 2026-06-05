using System;
using System.IO;
using System.Reflection;

class Program
{
    static void Main()
    {
        string mlNet6 = @"E:\SteamLibrary\steamapps\common\Muse Dash\MelonLoader\net6";
        string il2cppAssemblies = @"E:\SteamLibrary\steamapps\common\Muse Dash\MelonLoader\Il2CppAssemblies";
        string modsDir = @"E:\SteamLibrary\steamapps\common\Muse Dash\Mods";

        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string name = new AssemblyName(args.Name).Name;
            string net6Path = Path.Combine(mlNet6, name + ".dll");
            if (File.Exists(net6Path)) return Assembly.LoadFrom(net6Path);

            string il2cppPath = Path.Combine(il2cppAssemblies, name + ".dll");
            if (File.Exists(il2cppPath)) return Assembly.LoadFrom(il2cppPath);

            string modsPath = Path.Combine(modsDir, name + ".dll");
            if (File.Exists(modsPath)) return Assembly.LoadFrom(modsPath);

            return null;
        };

        try
        {
            string path = Path.Combine(modsDir, "CustomAlbums.dll");
            Assembly asm = Assembly.LoadFrom(path);
            Console.WriteLine("Loaded CustomAlbums successfully!");

            foreach (Type type in asm.GetTypes())
            {
                if (type.Namespace != null && type.Namespace.StartsWith("CustomAlbums") && 
                    !type.Namespace.Contains("NAudio") && !type.Namespace.Contains("SixLabors"))
                {
                    Console.WriteLine($"Type: {type.FullName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }
}
