namespace PlayBoard.Services
{
    public interface IGameStateStore
    {
        void SetCurrentWord(string username, string word);
        string? GetCurrentWord(string username);
    }
}
