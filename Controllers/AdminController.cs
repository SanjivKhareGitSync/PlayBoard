using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlayBoard.ClassCollection;
using PlayBoard.Services;

namespace PlayBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IUserStore _userStore;
        private readonly GuessTheWord _guessTheWord = new GuessTheWord();

        public AdminController(IUserStore userStore)
        {
            _userStore = userStore;
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userStore.GetAllUsernamesAsync();
            return Ok(users);
        }

        [HttpGet("Words")]
        public IActionResult GetAllWords()
        {
            var words = _guessTheWord.GetAllWords();
            return Ok(words);
        }
    }
}
