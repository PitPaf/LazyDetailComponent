namespace pza.Tools
{
    public class BaseStrings
    {
        public static string s_select = "Select";
        public static string s_lazy = "Lazy";
        public static string s_center = "Center";
        public static string s_custom = "Custom";
        public static string s_savePrefix = @"<-Save current text as default prefix->";
        
    }

    internal class StringTools
    {
        internal static string NoBrackets(string name)
        {
            if (name.Contains("<")) name = name.Remove(name.IndexOf("<"), 1);
            if (name.Contains(">")) name = name.Remove(name.IndexOf(">"), 1);
            return name;
        }
    }
}