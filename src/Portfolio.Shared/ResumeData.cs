using Portfolio.Shared.Models;

namespace Portfolio.Shared;

/// <summary>
/// The single source of truth for the site's content. The API serves it and the Blazor client
/// falls back to it when the API is unreachable, so the site is fully readable as a static build.
/// Edit here and both ends update.
/// </summary>
public static class ResumeData
{
    // Change this to your GitHub profile once the repo is up.
    public const string GitHubUrl = "https://github.com/KrisGlns";
    public const string LinkedInUrl = "https://www.linkedin.com/in/chris-galanis/";
    public const string Email = "chris_glns@hotmail.com";
    public const string Phone = "+30 698 190 8319";

    /// <summary>First day of the first full-time .NET role, used for the "years shipping" counter.</summary>
    private static readonly DateOnly DotnetCareerStart = new(2023, 11, 1);

    public static Resume Current { get; } = new()
    {
        Profile = new Profile
        {
            FullName = "Christos Galanis",
            Title = ".NET Developer",
            Location = "Athens, Greece",
            Email = Email,
            Phone = Phone,
            Tagline = "I build enterprise software on .NET — from Blazor front ends to the SQL underneath.",
            About =
            [
                "I'm a .NET developer based in Athens, currently at InTTrust building internal tools with .NET, Blazor and MSSQL Server, alongside enterprise applications for external customers.",
                "My background runs from DevSecOps and log analysis into enterprise application development — DevExpress XAF, REST APIs, PostgreSQL, Docker — with end-to-end ownership of testing and QA before anything reaches a customer.",
                "I'm also finishing an MSc in Advanced Information Systems at the University of Piraeus, and I actively fold AI-assisted development into my daily workflow, evaluating agent skills and MCP servers to move faster without giving up rigour."
            ],
            CvUrl = "files/Christos-Galanis-CV.pdf"
        },

        Highlights =
        [
            new Highlight($"{YearsSince(DotnetCareerStart)}+", "Years shipping .NET"),
            new Highlight("2", "Companies"),
            new Highlight("MSc", "In progress, University of Piraeus"),
            new Highlight("C2", "English proficiency")
        ],

        Experience =
        [
            new ExperienceItem
            {
                Company = "InTTrust",
                Role = ".NET Developer",
                Location = "Athens, Greece",
                Period = "Feb 2026 — Present",
                Current = true,
                Achievements =
                [
                    "Build internal tooling with .NET, Blazor and MSSQL Server.",
                    "Develop enterprise applications for external customers using .NET and Dapper.",
                    "Work closely with the front-end and Oracle APEX teams on incoming feature requests.",
                    "Own testing and QA end to end, before delivery to the customer.",
                    "Drive adoption of AI-assisted development — evaluating agent skills and MCP servers to streamline daily workflows."
                ],
                Stack = [".NET", "REST APIs", "MVC", "Blazor", "Dapper", "MSSQL Server", "Git", "Azure Dev Ops"]
            },
            new ExperienceItem
            {
                Company = "Incadea",
                Role = "Junior .NET Developer",
                Location = "Athens, Greece",
                Period = "Nov 2023 — Jan 2026",
                Achievements =
                [
                    "Developed enterprise applications with .NET and DevExpress XAF.",
                    "Designed and consumed REST APIs backed by PostgreSQL databases.",
                    "Worked day to day in Git, with Docker for local and shared environments.",
                    "Contributed unit tests and manual QA passes across delivered features."
                ],
                Stack = [".NET", "DevExpress XAF", "REST APIs", "PostgreSQL", "Git", "Docker", "Azure Dev Ops"]
            },
            new ExperienceItem
            {
                Company = "Logstail",
                Role = "DevSecOps Engineer — Internship",
                Location = "Athens, Greece",
                Period = "Jul 2022 — Oct 2022",
                Achievements =
                [
                    "Analysed application and security logs with OpenSearch and the ELK stack.",
                    "Used Docker and Kubernetes for running and inspecting containerised services.",
                    "Worked in Linux as the everyday operating system."
                ],
                Stack = ["OpenSearch", "ELK Stack", "Docker", "Kubernetes", "Linux", "Git"]
            },
            // new ExperienceItem
            // {
            //     Company = "Hospitality Industry",
            //     Role = "Customer-facing roles",
            //     Location = "Athens, Greece",
            //     Period = "Nov 2017 — Sep 2020",
            //     Achievements =
            //     [
            //         "Three years of customer service in fast-moving, demanding environments.",
            //         "Built the communication and organisational habits I still lean on when gathering requirements."
            //     ],
            //     Stack = ["Communication", "Organisation"]
            // }
        ],

        Education =
        [
            new EducationItem
            {
                Institution = "University of Piraeus",
                Department = "Department of Digital Systems",
                Degree = "MSc, Advanced Information Systems",
                Location = "Piraeus, Greece",
                Period = "Oct 2025 — Present",
                Current = true
            },
            new EducationItem
            {
                Institution = "University of Piraeus",
                Department = "Department of Digital Systems",
                Degree = "BSc, Computer Science",
                Location = "Piraeus, Greece",
                Period = "Oct 2016 — Jun 2022",
                Grade = "GPA 6.82"
            }
        ],

        // Levels are a self-assessment (0-100) and only drive the width of the meter bars.
        Skills =
        [
            new SkillCategory
            {
                Name = "Languages & Runtime",
                Icon = "code",
                Skills =
                [
                    new Skill("C#", 99),
                    new Skill(".NET Framework", 99),
                    new Skill("LINQ", 90),
                    new Skill("SQL", 85),
                    // new Skill("Java", 60)
                ]
            },
            new SkillCategory
            {
                Name = "Web & Frameworks",
                Icon = "layers",
                Skills =
                [
                    new Skill("ASP.NET Core", 99),
                    new Skill("REST APIs", 99),
                    new Skill("MVC", 90),
                    new Skill("Blazor", 80),
                    new Skill("Dapper", 50),
                    // new Skill("DevExpress XAF", 78)
                ]
            },
            new SkillCategory
            {
                Name = "Data",
                Icon = "database",
                Skills =
                [
                    new Skill("MSSQL Server", 99),
                    new Skill("Relational modelling", 90),
                    // new Skill("PostgreSQL", 75),
                ]
            },
            new SkillCategory
            {
                Name = "Platform & Tooling",
                Icon = "terminal",
                Skills =
                [
                    new Skill("Git", 99),
                    new Skill("Azure DevOps", 99),
                    // new Skill("Docker", 70),
                    // new Skill("Linux", 65),
                    // new Skill("OpenSearch / ELK", 55),
                    // new Skill("Kubernetes", 45)
                ]
            },
            new SkillCategory
            {
                Name = "Practice",
                Icon = "sparkles",
                Skills =
                [
                    new Skill("AI-assisted development", 95),
                    new Skill("Testing & QA", 72),
                    // new Skill("Microservices", 65)
                ]
            }
        ],

        Links =
        [
            new SocialLink("Email", $"mailto:{Email}", "mail"),
            new SocialLink("LinkedIn", LinkedInUrl, "linkedin"),
            new SocialLink("GitHub", GitHubUrl, "github")
        ]
    };

    private static int YearsSince(DateOnly start)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var years = today.Year - start.Year;
        if (start.AddYears(years) > today) years--;
        return Math.Max(years, 0);
    }
}
