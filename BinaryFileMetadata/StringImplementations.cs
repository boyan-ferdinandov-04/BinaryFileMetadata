namespace BinaryFileMetadata
{
    public class StringImplementations
    {
        public static string[] Split(string input, char delimiter)
        {
            int partsCount = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == delimiter)
                {
                    partsCount++;
                }
            }
            partsCount++; 

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

        public static int CustomHash(string input)
        {
            int hash = 0;
            for (int i = 0; i < input.Length; i++)
            {
                hash = (hash * 31 + input[i]) % int.MaxValue;
            }
            return hash;
        }

        public static string FormatFileListing(string fileName, int fileSize)
        {
            const int padding = 10;
            char[] formattedLine = new char[padding + 5];

            int i;
            for (i = 0; i < fileName.Length && i < padding; i++)
            {
                formattedLine[i] = fileName[i];
            }
            for (; i < padding; i++)
            {
                formattedLine[i] = ' ';
            }

            string sizeString = fileSize + "B;";
            for (int j = 0; j < sizeString.Length; j++)
            {
                formattedLine[padding + j] = sizeString[j];
            }

            return new string(formattedLine);
        }
    }
}
