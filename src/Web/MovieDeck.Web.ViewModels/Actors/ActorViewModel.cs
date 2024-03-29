﻿namespace MovieDeck.Web.ViewModels.Actors
{
    using AutoMapper;
    using MovieDeck.Common;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;

    public class ActorViewModel : IMapFrom<MovieActor>, IHaveCustomMappings
    {
        public int ActorId { get; set; }

        public string ActorFullName { get; set; }

        public string ActorPhotoPath { get; set; }

        public string PhotoUrl { get; set; }

        public string CharacterName { get; set; }

        public int ActorOriginalId { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<MovieActor, ActorViewModel>()
                .ForMember(x => x.PhotoUrl, opt =>
                    opt.MapFrom(x => x.Actor.PhotoPath == null ?
                        GlobalConstants.BlankProfilePicture :
                            x.Actor.PhotoPath.Contains("-") ?
                            $"/images/{GlobalConstants.ActorsImagesFolder}/" + x.Actor.PhotoPath :
                            string.Format(GlobalConstants.RemoteImagesUrl, x.Actor.PhotoPath)));
        }
    }
}
