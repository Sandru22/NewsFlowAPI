using System;
using System.Collections.Generic;

namespace NewsFlowAPI.Models;

public partial class UserPreference
{
    public int PreferenceId { get; set; }

    public string? UserId { get; set; }

    public string? Category { get; set; }

    public double? Score { get; set; }

    public virtual User? User { get; set; }
}
