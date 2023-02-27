using System;
using System.Collections.Generic;

namespace ProjectAPI1.Models;

public partial class Profile
{
    public string? Name { get; set; }

    public string Algorithm { get; set; } = null!;

    public int Id { get; set; }

    public string ExtraSettings { get; set; } = null!;

    public int IsOutdated { get; set; }

    public int IsDefault { get; set; }
}
