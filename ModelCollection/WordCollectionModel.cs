namespace PlayBoard
{
    public class WordCollectionModel
    {
        public Dictionary<int, List<string>> Groups { get; }

        public WordCollectionModel(Dictionary<int, List<string>> groups)
        {
            Groups = groups ?? new Dictionary<int, List<string>>();
        }

        // Convenience: get flattened list of all words
        public IEnumerable<string> AllWords() => Groups.Values.SelectMany(x => x);
    }

}
