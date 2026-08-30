using System.Text.Json;

namespace PlayBoard.ClassCollection
{
    public class GuessTheWord
    {
        private readonly string _wordCollectionPath;
        private readonly Random _random;

        public GuessTheWord()
        {
            _random = new Random();
            var projectRoot = GetProjectRootPath();
            _wordCollectionPath = Path.Combine(projectRoot, "DataCollection", "WordCollection.js");
        }

        private static string GetProjectRootPath() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        public string GetNewWord()
        {
            var model = LoadWordCollectionModel();
            if (model.Count == 0)
            {
                //return string.Empty;
                return "line number 25, word collection not working";
            }

            var keys = model.Keys.ToList();
            int groupKey = keys[_random.Next(keys.Count)];
            var wordsInGroup = model[groupKey];

            int wordIndex = _random.Next(wordsInGroup.Count);
            return wordsInGroup[wordIndex].ToUpper();
        }

        public List<CharacterInfo> CompareGuess(string guess, string targetWord)
        {
            string TargetWord = targetWord.ToUpper();
            string Guess = guess.ToUpper();
            List<int> greenIndices = new List<int>();
            List<int> grayIndices = new List<int>();
            List<int> yellowIndices = new List<int>();
            List<CharacterInfo> characters = new List<CharacterInfo>();
            for (int i = 0; i < Guess.Length; i++)
            {
                CharacterInfo info = new CharacterInfo();
                char chr = Guess[i];

                if (!TargetWord.Contains(chr))
                {
                    grayIndices.Add(i);
                    info.Char = chr;
                    info.Index = i;
                    info.Status = "GRAY";
                    characters.Add(info);
                    continue;
                }

                if (TargetWord[i] == chr)
                {
                    greenIndices.Add(i);
                    info.Char = chr;
                    info.Index = i;
                    info.Status = "GREEN";
                    characters.Add(info);

                    continue;
                }
            }
            for (int i = 0; i < Guess.Length; i++)
            {
                CharacterInfo info = new CharacterInfo();
                char chr = Guess[i];
                if (greenIndices.Contains(i) || grayIndices.Contains(i))
                {
                    continue;
                }
                int notFound = 1;
                for (int j = 0; j < TargetWord.Length; j++)
                {
                    if (greenIndices.Contains(j) || yellowIndices.Contains(j))
                    {
                        continue;
                    }
                    if (chr == TargetWord[j])
                    {
                        yellowIndices.Add(j);
                        info.Char = chr;
                        info.Index = i;
                        info.Status = "YELLOW";
                        characters.Add(info);
                        notFound = 0;
                        break;
                    }
                }
                if (notFound == 1)
                {
                    info.Char = chr;
                    info.Index = i;
                    info.Status = "GRAY";
                    characters.Add(info);
                }
            }

            greenIndices.Clear();
            yellowIndices.Clear();
            return characters.OrderBy(x => x.Index).ToList();
        }

        public List<CharacterInfo> CompareGuessOptimized(string guess, string targetWord)
        {
            int guessLen = guess.Length;
            int targetLen = targetWord.Length;

            bool[] isGreen = new bool[guessLen];
            bool[] isGray = new bool[guessLen];
            bool[] usedInTarget = new bool[targetLen]; // marks target positions already matched (green or yellow)

            var characters = new List<CharacterInfo>(guessLen);

            // First pass: mark GRAY (char not present) and GREEN (same position)
            for (int i = 0; i < guessLen; i++)
            {
                char chr = guess[i];
                if (!targetWord.Contains(chr))
                {
                    isGray[i] = true;
                    characters.Add(new CharacterInfo { Char = chr, Index = i, Status = "GRAY" });
                    continue;
                }

                if (i < targetLen && targetWord[i] == chr)
                {
                    isGreen[i] = true;
                    usedInTarget[i] = true;
                    characters.Add(new CharacterInfo { Char = chr, Index = i, Status = "GREEN" });
                    continue;
                }
            }

            // Second pass: for remaining guess positions, try to find a matching unused target position -> YELLOW; otherwise GRAY
            for (int i = 0; i < guessLen; i++)
            {
                if (isGreen[i] || isGray[i])
                    continue;

                char chr = guess[i];
                bool foundYellow = false;

                for (int j = 0; j < targetLen; j++)
                {
                    if (usedInTarget[j])
                        continue;

                    if (targetWord[j] == chr)
                    {
                        usedInTarget[j] = true;
                        characters.Add(new CharacterInfo { Char = chr, Index = i, Status = "YELLOW" });
                        foundYellow = true;
                        break;
                    }
                }

                if (!foundYellow)
                {
                    characters.Add(new CharacterInfo { Char = chr, Index = i, Status = "GRAY" });
                }
            }

            return characters;
        }

        private Dictionary<int, List<string>> LoadWordCollectionModel()
        {
            if (string.IsNullOrWhiteSpace(_wordCollectionPath) || !File.Exists(_wordCollectionPath))
                return new Dictionary<int, List<string>>();

            string json = File.ReadAllText(_wordCollectionPath);

            try
            {
                var raw = JsonSerializer.Deserialize<Dictionary<int, List<string>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return raw ?? new Dictionary<int, List<string>>();
            }
            catch (JsonException)
            {
                return new Dictionary<int, List<string>>();
            }
        }
    }
}