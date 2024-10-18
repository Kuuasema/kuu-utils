using System.Text;
namespace Kuuasema.Utils {
    public static class TextUtils {
        private static StringBuilder stringBuilder = new StringBuilder();
        public static string PadCapitalizedString(string text, bool captializeFirst = false) {
            stringBuilder.Clear();
            bool first = true;
            foreach (char c in text) {
                char _c = c;
                if (char.IsUpper(_c)) {
                    if (!first) {
                        stringBuilder.Append(" ");
                    }
                } else if (first && captializeFirst) {
                    _c = char.ToUpper(_c);
                }
                stringBuilder.Append(_c);
                first = false;
            }
            return stringBuilder.ToString();
        }
    }
}