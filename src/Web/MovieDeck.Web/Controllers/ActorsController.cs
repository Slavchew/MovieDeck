﻿namespace MovieDeck.Web.Controllers
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
    using MovieDeck.Web.ViewModels.Movies;

    public class ActorsController : BaseController
    {
        private readonly IActorsService actorsService;
        private readonly IWebHostEnvironment environment;

        public ActorsController(
            IActorsService actorsService,
            IWebHostEnvironment environment)
        {
            this.actorsService = actorsService;
            this.environment = environment;
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateActorInputModel input)
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

            await this.actorsService.CreateAsync(input, userId, $"{this.environment.WebRootPath}/images");

            return this.Redirect("/");
        }

        [Route("[controller]/{id:int}")]
        public IActionResult ById(int id)
        {
            var model = this.actorsService.GetById<SingleActorViewModel>(id);
            if (model == null)
            {
                return this.NotFound();
            }

            model.Images = this.actorsService.GetActorPhotosForSingleActorPage(model.OriginalId);
            return this.View(model);
        }
    }
}
