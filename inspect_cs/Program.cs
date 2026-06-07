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
            var types = assembly.GetTypes().Where(t => t.Name.EndsWith("Home"));
            foreach (var type in types) {
                Console.WriteLine("Type: " + type.FullName);
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
                    Console.WriteLine("Method: " + method.Name);
                }
            }
        }
    }
}
