using Pastel;
using System.Drawing;

namespace DataComparer.Extensions
{
    public static class PastelExtension
    {
        public static string  Warning(this string input) => input.Pastel(Color.LightSalmon);

        public static string Info(this string input) => input.Pastel(Color.Aqua); 

        public static string Success(this string input) => input.Pastel(Color.LawnGreen);

        public static string Error(this string input) => input.Pastel(Color.Red);

        public static string ErrorWithBg(this string input) => input.ColorifyWithBg(Color.White,Color.Red);

        public static string InfoWithBg(this string input) => input.ColorifyWithBg(Color.Aqua,Color.DarkBlue);

        public static string WarningWithBg(this string input) => input.ColorifyWithBg(Color.White,Color.DarkOrange);

        public static string SuccessWithBg(this string input) => input.ColorifyWithBg(Color.White, Color.DarkGreen);

        public static string Colorify(this string input,Color c) => input.Pastel(c);

        public static string HotPink(this string input ) => input.Pastel(Color.HotPink);
        public static string OrangeRed(this string input) => input.Pastel(Color.OrangeRed);
        public static string GreenYellow(this string input) => input.Pastel(Color.GreenYellow);
        public static string DarkRed(this string input) => input.Pastel(Color.DarkRed);
        public static string DarkBlue(this string input) => input.Pastel(Color.DarkBlue);

        public static string ColorifyWithBg(this string input, Color font, Color bg ) => input.Pastel(font).PastelBg(bg);

    }
}
