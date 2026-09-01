using System.Collections.Concurrent;

namespace PlayBoard.Services
{
    public class InMemoryGameStateStore : IGameStateStore
    {
        private readonly ConcurrentDictionary<string, string> _currentWords = new(StringComparer.OrdinalIgnoreCase);

        public void SetCurrentWord(string username, string word)
        {
            _currentWords[username] = word;
        }

        public string? GetCurrentWord(string username)
        {
            return _currentWords.TryGetValue(username, out var word) ? word : null;
        }
    }
}
