using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public List<CharacterInfo> Get(string question, string guess)
        {
            GuessTheWord guessTheWord = new GuessTheWord();
            var charInfo = guessTheWord.GetCharacters(guess,question);
            return charInfo;
        }

        [HttpGet("GetNewWord")]
        public string Get()
        {
            GuessTheWord guessTheWord = new GuessTheWord();
            string charInfo = guessTheWord.GetNewWord();
            return charInfo;
        }
    }
}
