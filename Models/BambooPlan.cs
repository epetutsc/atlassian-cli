using System.Text.Json.Serialization;

namespace AtlassianCli.Models;

/// <summary>
/// Represents a Bamboo plan.
/// </summary>
public class BambooPlan
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("shortKey")]
    public string? ShortKey { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("projectKey")]
    public string? ProjectKey { get; set; }

    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("buildName")]
    public string? BuildName { get; set; }

    [JsonPropertyName("averageBuildTimeInSeconds")]
    public double? AverageBuildTimeInSeconds { get; set; }

    [JsonPropertyName("link")]
    public BambooLink? Link { get; set; }

    [JsonPropertyName("isFavourite")]
    public bool IsFavourite { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("isBuilding")]
    public bool IsBuilding { get; set; }

    [JsonPropertyName("stages")]
    public BambooStagesList? Stages { get; set; }

    [JsonPropertyName("branches")]
    public BambooBranchesList? Branches { get; set; }

    [JsonPropertyName("variableContext")]
    public BambooVariableContext? VariableContext { get; set; }
}
