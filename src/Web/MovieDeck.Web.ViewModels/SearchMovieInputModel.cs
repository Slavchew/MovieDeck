namespace MovieDeck.Web.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using Microsoft.AspNetCore.Mvc.Rendering;

    public class SearchMovieInputModel
    {
        public string Search { get; set; }

        [DisplayName("Genres")]
        public int[] GenresIds { get; set; }

        public IEnumerable<SelectListItem> GenresItems { get; set; }

        public int? FromYear { get; set; }

        public int? ToYear { get; set; }
    }
}
