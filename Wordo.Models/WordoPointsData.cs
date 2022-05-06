namespace Wordo.Models;

public class WordoPointsData
{
    public List<UserPoint> UserPoints { get; set; } =
        new List<UserPoint>();

    public class UserPoint
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
    }
}