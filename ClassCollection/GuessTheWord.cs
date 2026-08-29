using System.Text.Json;

namespace PlayBoard.ClassCollection
{
    public class GuessTheWord
    {
        private string _wordCollectionPath =  Path.Combine(AppContext.BaseDirectory, "DataCollection","WordCollection.js");
        private string[] _wordCollection;
        private Random _rnd;

        public GuessTheWord()
        {
            _rnd = new Random();
            var projectRoot = GetProjectRootPath();
            _wordCollectionPath = Path.Combine(projectRoot, "DataCollection", "WordCollection.js");

            if (File.Exists(_wordCollectionPath))
            {
                var jsonString = File.ReadAllText(_wordCollectionPath);
                _wordCollection = jsonString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim())
                                            .ToArray();
            }
            else
            {
                _wordCollection = Array.Empty<string>();
            }
        }
        private static string GetProjectRootPath()=> Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        public string GetNewWord()
        {
            var model = LoadWordCollectionModel();
            if (model.Count() == 0)
            {
                return string.Empty;
            }
            int index1 = _rnd.Next(model.Keys.Min(), model.Keys.Max());
            int index2 = _rnd.Next(model[index1].Count());
            return model[index1][index2].ToUpper();
        }
        public List<CharacterInfo> GetCharacters(string guess, string question)
        {
            string Question = question.ToUpper();
            string Guess = guess.ToUpper();
            List<int> jumpGreen = new List<int>();
            List<int> jumpGray = new List<int>();
            List<int> jumpYellow = new List<int>();
            List<CharacterInfo> characters = new List<CharacterInfo>();
            for (int i = 0; i < Guess.Length; i++)
            {
                CharacterInfo info = new CharacterInfo();
                char chr = Guess[i];

                if (!Question.Contains(chr))
                {
                    jumpGray.Add(i);
                    info.Char = chr;
                    info.index = i;
                    info.status = "GRAY";
                    characters.Add(info);
                    continue;
                }

                if (Question[i] == chr)
                {
                    jumpGreen.Add(i);
                    info.Char = chr;
                    info.index = i;
                    info.status = "GREEN";
                    characters.Add(info);

                    continue;
                }
            }
            for (int i = 0; i < Guess.Length; i++)
            {
                CharacterInfo info = new CharacterInfo();
                char chr = Guess[i];
                if (jumpGreen.Contains(i) || jumpGray.Contains(i))
                {
                    continue;
                }
                int notFound = 1;
                for (int j = 0; j < Question.Length; j++)
                {
                    if (jumpGreen.Contains(j) || jumpYellow.Contains(j))
                    {
                        continue;
                    }
                    if (chr == Question[j])
                    {
                        jumpYellow.Add(j);
                        info.Char = chr;
                        info.index = i;
                        info.status = "YELLOW";
                        characters.Add(info);
                        notFound = 0;
                        break;
                    }
                }
                if (notFound==1)
                {
                    info.Char = chr;
                    info.index = i;
                    info.status = "GRAY";
                    characters.Add(info);
                }
            }

            jumpGreen.Clear();
            jumpYellow.Clear();
            return characters.OrderBy(x=>x.index).ToList();
        }
        public List<CharacterInfo> GetCharacters2(string guess, string Question)
        {
            int guessLen = guess.Length;
            int questionLen = Question.Length;

            bool[] isGreen = new bool[guessLen];
            bool[] isGray = new bool[guessLen];
            bool[] usedInQuestion = new bool[questionLen]; // marks question positions already matched (green or yellow)

            var characters = new List<CharacterInfo>(guessLen);

            // First pass: mark GRAY (char not present) and GREEN (same position)
            for (int i = 0; i < guessLen; i++)
            {
                char chr = guess[i];
                if (!Question.Contains(chr))
                {
                    isGray[i] = true;
                    characters.Add(new CharacterInfo { Char = chr, index = i, status = "GRAY" });
                    continue;
                }

                if (i < questionLen && Question[i] == chr)
                {
                    isGreen[i] = true;
                    usedInQuestion[i] = true;
                    characters.Add(new CharacterInfo { Char = chr, index = i, status = "GREEN" });
                    continue;
                }
            }

            // Second pass: for remaining guess positions, try to find a matching unused question position -> YELLOW; otherwise GRAY
            for (int i = 0; i < guessLen; i++)
            {
                if (isGreen[i] || isGray[i])
                    continue;

                char chr = guess[i];
                bool foundYellow = false;

                for (int j = 0; j < questionLen; j++)
                {
                    if (usedInQuestion[j])
                        continue;

                    if (Question[j] == chr)
                    {
                        usedInQuestion[j] = true;
                        characters.Add(new CharacterInfo { Char = chr, index = i, status = "YELLOW" });
                        foundYellow = true;
                        break;
                    }
                }

                if (!foundYellow)
                {
                    characters.Add(new CharacterInfo { Char = chr, index = i, status = "GRAY" });
                }
            }

            return characters;
        }
        private Dictionary<int, List<string>> LoadWordCollectionModel()
        {
            if (string.IsNullOrWhiteSpace(_wordCollectionPath) || !System.IO.File.Exists(_wordCollectionPath))
                return new Dictionary<int, List<string>>();

            string json = System.IO.File.ReadAllText(_wordCollectionPath);

            Dictionary<int, List<string>>? raw;
            try
            {
                raw = JsonSerializer.Deserialize<Dictionary<int, List<string>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                return new Dictionary<int, List<string>>();
            }

            var groups = new Dictionary<int, List<string>>(raw?.Count ?? 0);
            return raw ?? new Dictionary<int, List<string>>();
        }
    }
}
