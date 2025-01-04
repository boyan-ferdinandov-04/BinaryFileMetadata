namespace BinaryFileMetadata
{
    public static class StringImplementations
    {
        public static string[] Split(string input, char delimiter)
        {
            if (IsNullOrWhiteSpace(input))
                return new string[0];

            int partsCount = 1;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == delimiter)
                {
                    partsCount++;
                }
            }

            string[] result = new string[partsCount];
            int startIndex = 0;
            int partIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == delimiter)
                {
                    result[partIndex++] = Substring(input, startIndex, i - startIndex);
                    startIndex = i + 1;
                }
            }

            // Last part
            result[partIndex] = Substring(input, startIndex, input.Length - startIndex);

            return result;
        }

        public static int IndexOf(string input, char character)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == character)
                {
                    return i;
                }
            }
            return -1;
        }

        public static string Substring(string input, int start, int length)
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = input[start + i];
            }
            return new string(result);
        }

        public static int CustomCompare(string a, string b)
        {
            if (a == null && b == null)
                return 0;
            if (a == null) 
                return -1;
            if (b == null) 
                return 1;

            int length = a.Length < b.Length ? a.Length : b.Length;
            for (int i = 0; i < length; i++)
            {
                if (a[i] != b[i])
                {
                    return a[i] - b[i];
                }
            }
            return a.Length - b.Length;
        }

        public static bool IsNullOrWhiteSpace(string str)
        {
            if (str == null) return true;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ' && str[i] != '\t' && str[i] != '\r' && str[i] != '\n')
                {
                    return false;
                }
            }
            return true;
        }

        public static string ToLower(string input)
        {
            char[] result = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c >= 'A' && c <= 'Z')
                {
                    c = (char)(c + 32);
                }
                result[i] = c;
            }
            return new string(result);
        }

        public static bool StartsWith(string input, string prefix)
        {
            if (prefix == null) 
                prefix = string.Empty;
            if (input == null) 
                return prefix.Length == 0;
            if (prefix.Length > input.Length) 
                return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (input[i] != prefix[i]) 
                    return false;
            }
            return true;
        }
    }
}
