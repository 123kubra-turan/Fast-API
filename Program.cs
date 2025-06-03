using System;
using System.Windows.Forms;

namespace Stok_Kontrol_V2
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana giriş noktasıdır.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Tüm uygulama genelinde beklenmeyen hataları yakalamak için global handler eklenebilir.
            Application.ThreadException += (sender, args) =>
            {
                MessageBox.Show("Beklenmeyen bir hata oluştu:\n\n" + args.Exception.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // İsteğe bağlı: loglama yapılabilir.
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = args.ExceptionObject as Exception;
                MessageBox.Show("Kritik bir hata oluştu. Uygulama kapatılıyor.\n\n" + ex?.Message,
                    "Kritik Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // İsteğe bağlı: loglama yapılabilir.
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Giriş formu veya splash screen eklenmek istenirse burada gösterilebilir.
            Application.Run(new Form1());
        }
    }
}
