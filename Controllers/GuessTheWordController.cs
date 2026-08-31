using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayBoard.ClassCollection;
using PlayBoard.Services;

namespace PlayBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GuessTheWordController : ControllerBase
    {
        private readonly IGameStateStore _gameStateStore;

        public GuessTheWordController(IGameStateStore gameStateStore)
        {
            _gameStateStore = gameStateStore;
        }

        [HttpGet("GetComparision")]
        public IActionResult Get(string guess)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var currentWord = _gameStateStore.GetCurrentWord(username);
            if (string.IsNullOrEmpty(currentWord))
                return BadRequest("No active word for this user. Call GetNewWord first.");

            //if(guess.Length != currentWord.Length)
            //    return BadRequest("Guess length does not match the current word length.");

            GuessTheWord guessTheWord = new GuessTheWord();
            var charInfo = guessTheWord.CompareGuess(guess, currentWord);
            return Ok(charInfo);
        }

        [HttpGet("GetNewWord")]
        public IActionResult Get()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            GuessTheWord guessTheWord = new GuessTheWord();
            string word = guessTheWord.GetNewWord();

            if (string.IsNullOrEmpty(word))
            {
                return NotFound("Some error occurred.");
            }

            _gameStateStore.SetCurrentWord(username, word);
            return Ok(word);
        }
    }
}