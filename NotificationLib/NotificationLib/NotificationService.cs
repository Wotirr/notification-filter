namespace NotificationLib;

public class NotificationService
{
    public IEnumerable<Notification> FilterAndSort(
        IEnumerable<Notification> notifications,
        NotificationFilterOptions options)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(options);

        return Sort(Filter(notifications.Where(n => n != null), options), options);
    }

    private static IEnumerable<Notification> Filter(
        IEnumerable<Notification> source, NotificationFilterOptions options)
    {
        if (options.IsRead.HasValue)
            source = source.Where(n => n.IsRead == options.IsRead.Value);

        if (options.Types is { Length: > 0 })
            source = source.Where(n => options.Types.Contains(n.Type));

        if (options.MinPriority.HasValue)
            source = source.Where(n => n.Priority >= options.MinPriority.Value);

        if (!string.IsNullOrWhiteSpace(options.SearchText))
        {
            var text = options.SearchText.Trim();
            source = source.Where(n =>
                (n.Title?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (n.Content?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return source;
    }

    private static IEnumerable<Notification> Sort(
           IEnumerable<Notification> source, NotificationFilterOptions options)
    {
        if (options.SortBy is null)
            return source;

        var desc = options.Descending;

        return options.SortBy switch
        {
            SortNotificationBy.Date => desc
                ? source.OrderByDescending(n => n.CreatedAt)
                : source.OrderBy(n => n.CreatedAt),
            SortNotificationBy.Priority => desc
                ? source.OrderByDescending(n => n.Priority)
                : source.OrderBy(n => n.Priority),
            SortNotificationBy.Title => desc
                ? source.OrderByDescending(n => n.Title, StringComparer.OrdinalIgnoreCase)
                : source.OrderBy(n => n.Title, StringComparer.OrdinalIgnoreCase),
            _ => throw new ArgumentOutOfRangeException(nameof(options), "Unknown sort field.")
        };
    }
}