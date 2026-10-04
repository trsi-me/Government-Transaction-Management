using System.Drawing;

namespace STOT.Utils
{
    public static class FontHelper
    {
        public static Font GetArabicFont(float size, FontStyle style = FontStyle.Regular)
        {
            try
            {
                return new Font("IBM Plex Sans Arabic", size, style, GraphicsUnit.Point);
            }
            catch
            {
                try
                {
                    return new Font("Segoe UI", size, style, GraphicsUnit.Point);
                }
                catch
                {
                    return new Font("Arial", size, style, GraphicsUnit.Point);
                }
            }
        }
    }
}

