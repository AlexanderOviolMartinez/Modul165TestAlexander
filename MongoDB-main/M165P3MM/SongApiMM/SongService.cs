using MongoDB.Driver;
using System;
using System.Collections.Generic;
using SongApiMM.Models;

public class SongService : ISongService
{
    public string GetDatabases()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Song> Get()
    {
        return new List<Song>
        {
            new Song { Id = 1, Title = "Song 1", Artist = "Artist 1" },
            new Song { Id = 2, Title = "Song 2", Artist = "Artist 2" }
        };
    }

    public Song Get(string id)
    {
        throw new NotImplementedException();
    }

    public void Create(Song song)
    {
        throw new NotImplementedException();
    }

    public void Update(string id, Song song)
    {
        throw new NotImplementedException();
    }

    public void Remove(string id)
    {
        throw new NotImplementedException();
    }
}