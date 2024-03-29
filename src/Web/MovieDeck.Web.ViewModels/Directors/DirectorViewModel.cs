﻿namespace MovieDeck.Web.ViewModels.Directors
{
    using AutoMapper;

    using MovieDeck.Common;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;

    public class DirectorViewModel : IMapFrom<MovieDirector>, IHaveCustomMappings
    {
        public int DirectorId { get; set; }

        public string DirectorFullName { get; set; }

        public string DirectorPhotoPath { get; set; }

        public string PhotoUrl { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<MovieDirector, DirectorViewModel>()
                .ForMember(x => x.PhotoUrl, opt =>
                    opt.MapFrom(x => x.Director.PhotoPath == null ?
                        GlobalConstants.BlankProfilePicture :
                            x.Director.PhotoPath.Contains("-") ?
                            $"/images/{GlobalConstants.DirectorsImagesFolder}/" + x.Director.PhotoPath :
                            string.Format(GlobalConstants.RemoteImagesUrl, x.Director.PhotoPath)));
        }
    }
}
