namespace MovieDeck.Web.ViewModels.Directors
{
    using System;
    using System.Collections.Generic;

    using AutoMapper;

    using MovieDeck.Common;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;
    using MovieDeck.Web.ViewModels.Actors;
    using MovieDeck.Web.ViewModels.Images;

    public class SingleDirectorViewModel : IMapFrom<Director>, IHaveCustomMappings
    {
        public string FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string Biography { get; set; }

        public string BiographyShort =>
            this.Biography != null ?
            this.Biography.Length > 300
                ? this.Biography.Substring(0, 300) + "..."
                : this.Biography
            : null;

        public string PhotoUrl { get; set; }

        public int OriginalId { get; set; }

        public IEnumerable<DirectorMovieViewModel> Movies { get; set; }

        public IEnumerable<DirectorImageViewModel> Images { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Director, SingleDirectorViewModel>()
                .ForMember(x => x.PhotoUrl, opt =>
                    opt.MapFrom(x => x.PhotoPath == null ?
                        GlobalConstants.BlankProfilePicture :
                            x.PhotoPath.Contains("-") ?
                            $"/images/{GlobalConstants.DirectorsImagesFolder}/" + x.PhotoPath :
                            string.Format(GlobalConstants.RemoteImagesUrl, x.PhotoPath)));
        }
    }
}
