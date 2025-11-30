using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.Interfaces.Authantication;

namespace RaSed.API.Controllers
{
    [Route("api/section")]
    [ApiController]
    [Authorize]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSections()
        {
            try
            {
                var sections = await _sectionService.GetAllSectionsAsync();

                return Ok(new
                {
                    isSuccessful = true,
                    data = sections
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    error = "An unexpected error occurred while fetching sections.",
                    details = ex.Message
                });
            }
        }
    }
}
