using System;
using System.Reflection;
using System.Linq;

namespace inspect_cs
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.LoadFrom(@"D:\MuseDashStuff\Mod_SourceCode\MultiPlayer\bin\Debug\net6.0\Assembly-CSharp.dll");
            var type = assembly.GetType("MusicInfo");
            if (type == null) type = assembly.GetType("Assets.Scripts.PeroTools.Commons.MusicInfo") ?? assembly.GetTypes().FirstOrDefault(t => t.Name == "MusicInfo");
            
            if (type != null) {
                foreach (var prop in type.GetProperties()) {
                    Console.WriteLine("Property: " + prop.Name);
                }
                foreach (var field in type.GetFields()) {
                    Console.WriteLine("Field: " + field.Name);
                }
            } else {
                Console.WriteLine("MusicInfo not found");
            }
        }
    }
}
