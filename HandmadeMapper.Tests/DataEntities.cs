// ReSharper disable ClassNeverInstantiated.Global
namespace HandmadeMapper.Tests
{
    public class Thing
    {
        public Thing()
        {
            
        }

        public Thing(string name)
        {
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string VerySecretString { get; set; } = null!;

        public Cat FavoriteCat { get; set; } = null!;

        public Thing BestFriend { get; set; } = null!;
    }


    public class ThingDto
    {
        public ThingDto()
        {

        }

        public ThingDto(string name)
        {
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public CatDto FavoriteCat { get; set; } = null!;

        public ThingDto BestFriend { get; set; } = null!;
    }

    public class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CutenessLevel { get; set; }
        public decimal Weight { get; set; }

        public Thing Owner { get; set; } = null!;
    }

    public class CatDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CutenessLevel { get; set; }

        public ThingDto Owner { get; set; } = null!;
    }

}