namespace HupSoonCheong.MVC.Models
{
    public class Container
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Photo> Photos { get; set; } = new List<Photo>();
    }

    public class Photo
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int ContainerId { get; set; }
        public Container Container { get; set; }
    }
}
