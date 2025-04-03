using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Terraria.FirstApril {
    internal class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Вас приветствует " + Assembly.GetExecutingAssembly().FullName);

            if (!File.Exists("Terraria.exe")) {
                Console.WriteLine("Terraria не обнаружена. Поместите все файлы в папку с игрой");
                Console.ReadKey();
                return;
            }

            Injector.Inject();
        }
    }

    internal class Injector {
        public static void Inject() {
            var terraria = typeof(Terraria.Program).Assembly;

            // Загружаем библиотеки, запакованные в EXE Teraria. Иначе будет исключение
            // Код спионерен из нее же
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

            TerraDoom doom = null;
            Main.OnPreDraw += _ => {
                if (doom == null) doom = new TerraDoom();
                doom.Handle();
            };

            Task.Run(() => terraria.EntryPoint.Invoke(null, new object[] { Environment.GetCommandLineArgs() })).Wait();
        }
    }
}
