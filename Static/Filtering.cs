namespace Multiplayer.Static
{
    internal class TrieNode
    {
        public Dictionary<char, TrieNode> Children = new();
        public bool IsEndOfWord = false;
    }

    internal static class Filtering
    {
        private static readonly TrieNode Root = new();

        internal static void AddCurse(string word)
        {
            var node = Root;

            foreach (char c in word.ToLower())
            {
                if (!node.Children.ContainsKey(c)) node.Children[c] = new();
                node = node.Children[c];
            }

            node.IsEndOfWord = true;
        }

        /// <summary>
        /// Replaces every curse in the given <paramref name="text"/> with *.
        /// </summary>
        /// <param name="text">Text to filter.</param>
        /// <returns>Filtered text.</returns>
        internal static string Filter(string text)
        {
            var lower = text.ToLower();
            var result = text.ToCharArray();

            for (int i = 0; i < lower.Length; i++)
            {
                var node = Root;
                int j = i;

                while (j < lower.Length && node.Children.ContainsKey(lower[j]))
                {
                    node = node.Children[lower[j]];

                    if (node.IsEndOfWord)
                    {
                        for (int k = i; k <= j; k++) result[k] = '*';
                        break;
                    }

                    j++;
                }
            }

            return new string(result);
        }
    }
}
