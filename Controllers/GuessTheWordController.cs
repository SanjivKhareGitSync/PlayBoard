using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayBoard.ClassCollection;

namespace PlayBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GuessTheWordController : ControllerBase
    {
        [HttpGet("GetComparision")]
        public IActionResult Get(string question, string guess)
        {
            GuessTheWord guessTheWord = new GuessTheWord();
            var charInfo = guessTheWord.CompareGuess(guess, question);
            return Ok(charInfo);
        }

        [HttpGet("GetNewWord")]
        public IActionResult Get()
        {
            GuessTheWord guessTheWord = new GuessTheWord();
            string charInfo = guessTheWord.GetNewWord();

            if(string.IsNullOrEmpty(charInfo))
            {
                return NotFound("Some error occurred.");
            }

            return Ok(charInfo);
        }
    }
}
