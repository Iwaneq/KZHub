﻿namespace KZHub.WebClient.DTOs.Card
{
    public class CreatePointDTO
    {
        public TimeOnly Time { get; set; }
        public string? Title { get; set; }
        public string? ZastepMember { get; set; }
    }
}