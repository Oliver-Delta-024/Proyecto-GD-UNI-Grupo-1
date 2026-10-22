using System;

/// <summary>
/// Pure run-id lifecycle decisions, separated so they can be verified without
/// Unity PlayerPrefs or an Editor session.
/// </summary>
public static class RunIdV2Lifecycle
{
    private const string PreferencePrefix = "RankingOnline.RunIdV2.";

    public static string PreferenceKeyForMode(string mode)
    {
        string normalizedMode = string.IsNullOrWhiteSpace(mode)
            ? "Predeterminado"
            : mode.Trim();

        return PreferencePrefix + normalizedMode;
    }

    public static string Resolve(bool continuingSavedRun, string savedRunId, Func<string> createRunId)
    {
        if (continuingSavedRun && Guid.TryParseExact(savedRunId, "D", out Guid savedGuid))
        {
            return savedGuid.ToString("D");
        }

        if (createRunId == null)
        {
            throw new ArgumentNullException(nameof(createRunId));
        }

        string newRunId = createRunId();
        if (!Guid.TryParseExact(newRunId, "D", out Guid newGuid))
        {
            throw new InvalidOperationException("The run-id factory must return a UUID in D format.");
        }

        return newGuid.ToString("D");
    }
}
