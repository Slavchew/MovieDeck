namespace MovieDeck.Web.Controllers
{
    using System.Data;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    using MovieDeck.Common;
    using MovieDeck.Services.Data;
    using MovieDeck.Web.ViewModels.Actors;
    using MovieDeck.Web.ViewModels.Directors;

    public class DirectorsController : BaseController
    {
        private readonly IDirectorsService directorsService;
        private readonly IWebHostEnvironment environment;

        public DirectorsController(
            IDirectorsService directorsService,
            IWebHostEnvironment environment)
        {
            this.directorsService = directorsService;
            this.environment = environment;
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateDirectorInputModel input)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(input);
            }

            string userId = null;
            if (this.User.Identity.IsAuthenticated)
            {
                userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }

            await this.directorsService.CreateAsync(input, userId, $"{this.environment.WebRootPath}/images");

            return this.Redirect("/");
        }

        [Route("[controller]/{id:int}")]
        public IActionResult ById(int id)
        {
            var model = this.directorsService.GetById<SingleDirectorViewModel>(id);
            if (model == null)
            {
                return this.NotFound();
            }

            model.Images = this.directorsService.GetDirectorPhotosForSingleDirectorPage(model.OriginalId);
            return this.View(model);
        }
    }
}
