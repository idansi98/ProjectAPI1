using System;
using System.Collections.Generic;

namespace ProjectAPI1.Models;

public partial class User
{
    public int Number { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? TempPassword { get; set; }
}
