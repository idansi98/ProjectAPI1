using System;
using System.Collections.Generic;

namespace ProjectAPI1.Models;

public partial class Problem
{
    public string? Name { get; set; }

    public string Boxes { get; set; } = null!;

    public string ContainerDimensions { get; set; } = null!;

    public string? ExtraPreferences { get; set; }

    public int Id { get; set; }
}
