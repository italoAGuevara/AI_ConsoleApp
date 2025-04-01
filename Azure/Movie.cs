﻿
namespace Azure_project
{
    public class Movie
    {
        public string Title { get; set; }

        public string Director { get; set; }

        public int ReleaseYear { get; set; }

        public double Rating { get; set; }

        public bool IsAvailableOnStreaming { get; set; }

        public List<string> Tags { get; set; }
    }

    public class MovieResult
    {
        public List<Movie> Movies { get; set; }
    }

}
