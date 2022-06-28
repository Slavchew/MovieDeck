﻿namespace MovieDeck.Data.Models
{
    using System;

    using MovieDeck.Data.Common.Models;
    using MovieDeck.Data.Models.Enums;

    public class BasePersonModel<T> : BaseDeletableModel<T>
    {
        public string FullName { get; set; }

        public DateTime BirthDate { get; set; }

        public string Biography { get; set; }

        public string PhotoUrl { get; set; }
    }
}