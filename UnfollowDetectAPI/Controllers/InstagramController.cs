using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using UnfollowDetectAPI.Result;
using InstaSharper.Classes.Models;

namespace UnfollowDetectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstagramController : ControllerBase
    {
        private IConfiguration _config;

        public InstagramController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IDataResult<InstaUserShortList>> Get(string username)
        {
            return await InstagramManager.GetUnfollowers(username, _config);
        }
    }
}
