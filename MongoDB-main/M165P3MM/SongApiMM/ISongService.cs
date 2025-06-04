using System.Collections.Generic;
using SongApiMM.Models;

public interface ISongService
{
    String GetDatabases();
    IEnumerable<Song> Get();
    Song Get (string id);
    void Create (Song song);
    public void Update (string id, Song song);
    public void Remove (string id);
}