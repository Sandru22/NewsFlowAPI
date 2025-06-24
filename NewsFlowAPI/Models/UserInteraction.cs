using Humanizer;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System;
using System.Collections.Generic;

namespace NewsFlowAPI.Models;

public partial class UserInteraction
{
    public int InteractionId { get; set; }
    public string UserId { get; set; }
    public int NewsId { get; set; }
    public byte InteractionType { get; set; }
    public DateTime InteractionDate { get; set; }
    public NewsItem NewsItem { get; set; }
    public User User { get; set; }
}
