using System;
using System.Windows.Forms;
using STOT.Config;
using STOT.UI;

namespace STOT
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            try
            {
                System.Drawing.Font defaultFont;
                try
                {
                    defaultFont = new System.Drawing.Font("IBM Plex Sans Arabic", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                }
                catch
                {
                    try
                    {
                        defaultFont = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                    }
                    catch
                    {
                        defaultFont = System.Drawing.SystemFonts.DefaultFont;
                    }
                }

                Application.SetDefaultFont(defaultFont);

                var dbManager = new DatabaseManager();
                var loginWindow = new LoginWindow(dbManager);
                Application.Run(loginWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تهيئة التطبيق:\n{ex.Message}\n\n{ex.StackTrace}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

