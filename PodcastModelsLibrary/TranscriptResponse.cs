using System;
using System.Collections.Generic;
using System.Text;

namespace PodcastModelsLibrary
{
    public class TranscriptResponse
    {
        public Guid SessionGuid { get; set; }
        public string? Text { get; set; }
        public long TranscribedAtInUnixTimeSeconds { get; set; }
    }
}
