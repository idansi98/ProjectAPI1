using ProjectAPI1.Models;

namespace ProjectAPI1.Classes
{


#nullable enable
    public class Box
    {
        public string? Description { get; set; }
        public Dimensions Dimensions { get; set; } = default!;
        public string? Restrictions { get; set; }
        public int OrderInList { get; set; }
        public string? Preferences { get; set; }
        public Box(string? description, Dimensions dimensions, string? restrictions, int orderInList, string? preferences)
        {
            Description = description;
            Dimensions = dimensions;
            Restrictions = restrictions;
            OrderInList = orderInList;
            Preferences = preferences;
        }
    }
#nullable disable

    public class Dimensions : IEquatable<Dimensions>
    {
        public float Height { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public Dimensions(float width, float height, float length)
        {
            Height = height;
            Width = width;
            Length = length;
        }
        public bool Equals(Dimensions other)
        {
            if (other == null)
            {
                return false;
            }
            return Height == other.Height && Width == other.Width && Length == other.Length;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Dimensions);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Height.GetHashCode();
                hash = hash * 27 + Width.GetHashCode();
                hash = hash * 31 + Length.GetHashCode();
                return hash;
            }
        }
    }

    public class Position
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class BoxPlacement
    {
        public Box Box { get; set; }
        public Position Position { get; set; }
    }

    public class Rotation
    {
        public int RightToLeft { get; set; }
        public int UpDown { get; set; }
        public int TopBottom { get; set; }
    }

#nullable enable
    public class Problem
    {
        public string? Name { get; set; }
        public List<Box> Boxes { get; set; } = default!;
        public Dimensions ContainerDimensions { get; set; } = default!;
        public string? ExtraPreferences { get; set; }
        public int Id { get; set; }
        public Problem(string? name, List<Box> boxes, Dimensions containerDimensions, string? extraPreferences)
        {
            Name = name;
            Boxes = boxes;
            ContainerDimensions = containerDimensions;
            ExtraPreferences = extraPreferences;
        }
        public Problem()
        {
        }
    }
#nullable disable

#nullable enable
    public class Solution
    {
        public int ProblemId { get; set; }
        public int ProfileId { get; set; }
        public List<BoxPlacement> Placements { get; set; } = default!;
        public int Id { get; set; }
    }
#nullable disable
    public class Profile
    {
        public string Name { get; set; } = default!;
        public string Algorithm { get; set; } = default!;
        public int Id { get; set; }
        public string? ExtraSettings { get; set; }
        public int IsOutdated { get; set; }
        public int IsDefault { get; set; }
        public User User { get; set; }
        public Profile(string name, string algo, string extraSettings, int isOutdated, int isDefault, User user)
        {
            Name = name;
            Algorithm = algo;
            ExtraSettings = extraSettings;
            IsOutdated = isOutdated;
            IsDefault = isDefault;
            User = user;
        }
        public Profile()
        {

        }
    }

    public class ProblemProfile
    {
        public Profile Profile { get; set; }
        public Problem Problem { get; set; }
        public ProblemProfile(Profile profile, Problem problem)
        {
            Profile = profile;
            Problem = problem;
        }
    }
}

