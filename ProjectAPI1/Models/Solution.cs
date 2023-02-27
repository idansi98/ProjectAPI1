using System;
using System.Collections.Generic;

namespace ProjectAPI1.Models;

public partial class Solution
{
    public int ProblemId { get; set; }

    public int ProfileId { get; set; }

    public string Placements { get; set; } = null!;

    public int Id { get; set; }
}
