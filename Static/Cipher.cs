namespace Multiplayer.Static
{
    // Caesar cipher
    internal static class Cipher
    {
        /// <param name="text">A <see cref="string"/> to encrypt.</param>
        /// <param name="shift">Amount of characters to shift by.</param>
        /// <returns>A new encrypted <see cref="string"/>.</returns>
        public static string Encrypt(string text, int shift)
        {
            // Ensure shift is within 0-25 range to prevent excessive rotations
            shift %= 26;
            char[] buffer = text.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char character = buffer[i];

                if (char.IsLetter(character))
                {
                    char offset = char.IsUpper(character) ? 'A' : 'a';
                    // Calculate new position using modulo for wrap-around
                    character = (char)(((character - offset + shift) % 26) + offset);
                }
                buffer[i] = character;
            }

            return new string(buffer);
        }

        /// <param name="ciphertext">A <see cref="string"/> to decrypt.</param>
        /// <param name="shift">Amount of characters to shift by.</param>
        /// <returns>A new decrypted <see cref="string"/>.</returns>
        public static string Decrypt(string ciphertext, int shift)
        {
            // Decryption is simply encryption with a negative shift.
            // The modulo logic handles negative numbers correctly in C# when implemented this way
            // (adding 26 before modulo ensures a positive result if (character - offset - shift) is negative)
            shift %= 26;
            char[] buffer = ciphertext.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char character = buffer[i];

                if (char.IsLetter(character))
                {
                    char offset = char.IsUpper(character) ? 'A' : 'a';
                    // (c - k - 'A' + 26) % 26 ensures positive result before adding offset
                    character = (char)(((character - offset - shift + 26) % 26) + offset);
                }
                buffer[i] = character;
            }

            return new string(buffer);
        }
    }
}
