using System;
using System.IO;
using System.Reflection;

namespace Terraria.FirstApril {
    internal class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Вас приветствует " + Assembly.GetExecutingAssembly().FullName);

            if (!File.Exists("Terraria.exe")) {
                Console.WriteLine("Terraria не обнаружена. Поместите все файлы в папку с игрой");
                Console.ReadKey();
                return;
            }

            Injector.Inject(args);
        }
    }

    internal class Injector {
        public static void Inject(string[] args) {
            var terraria = typeof(Terraria.Program).Assembly;

            // Загружаем библиотеки, запакованные в EXE Teraria. Иначе будет исключение
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs sargs) {
                string resourceName = new AssemblyName(sargs.Name).Name + ".dll";
                string text = Array.Find(terraria.GetManifestResourceNames(), (string element) => element.EndsWith(resourceName));
                if (text == null) return null;
            
                Assembly assembly;
                using (Stream manifestResourceStream = terraria.GetManifestResourceStream(text)) {
                    byte[] array = new byte[manifestResourceStream.Length];
                    manifestResourceStream.Read(array, 0, array.Length);
                    assembly = Assembly.Load(array);
                }
            
                Console.WriteLine("Сборка загружена: " + assembly.FullName);
                return assembly;
            };

            // Последующие патч(и) нарушают порядок вызова статических конструкторов, поэтому инициализируем критично важное извне
            // Тут по-любому есть другие подводные камни, но мне лень. Заодно меняем папку сохранений
            Terraria.Program.SavePath = "./TerraDOOM Saves/";
            
            TerraDoom doom = null;
            Main.OnPreDraw += _ => {
                if (doom == null) doom = new TerraDoom();
                doom.Handle();
            };

            terraria.EntryPoint.Invoke(null, new object[] { args });
        }
    }
}
