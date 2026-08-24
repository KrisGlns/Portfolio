namespace Portfolio.Shared.Models;

/// <summary>Everything the site renders, in one serializable graph.</summary>
public sealed record Resume
{
    public required Profile Profile { get; init; }
    public required IReadOnlyList<ExperienceItem> Experience { get; init; }
    public required IReadOnlyList<EducationItem> Education { get; init; }
    public required IReadOnlyList<SkillCategory> Skills { get; init; }
    public required IReadOnlyList<Highlight> Highlights { get; init; }
    public required IReadOnlyList<SocialLink> Links { get; init; }
}

public sealed record Profile
{
    public required string FullName { get; init; }
    public required string Title { get; init; }
    public required string Location { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required string Tagline { get; init; }
    public required IReadOnlyList<string> About { get; init; }
    public required string CvUrl { get; init; }
}

public sealed record ExperienceItem
{
    public required string Company { get; init; }
    public required string Role { get; init; }
    public required string Location { get; init; }
    public required string Period { get; init; }
    public bool Current { get; init; }
    public required IReadOnlyList<string> Achievements { get; init; }
    public required IReadOnlyList<string> Stack { get; init; }
}

public sealed record EducationItem
{
    public required string Institution { get; init; }
    public required string Department { get; init; }
    public required string Degree { get; init; }
    public required string Location { get; init; }
    public required string Period { get; init; }
    public string? Grade { get; init; }
    public bool Current { get; init; }
}

public sealed record SkillCategory
{
    public required string Name { get; init; }
    /// <summary>Name of the icon rendered by <c>Icon.razor</c>.</summary>
    public required string Icon { get; init; }
    public required IReadOnlyList<Skill> Skills { get; init; }
}

/// <summary>A skill and how strongly it is claimed (0-100), used for the meter bars.</summary>
public sealed record Skill(string Name, int Level);

/// <summary>A headline number shown in the hero strip.</summary>
public sealed record Highlight(string Value, string Label);

public sealed record SocialLink(string Name, string Url, string Icon);
