namespace SmartService.Domain.Services;

/// <summary>
/// Domain service for calculating distance between two geographic coordinates.
/// Currently in skeleton mode - distance calculation is commented out.
/// Uncomment the Haversine formula when ready to enable location features.
/// </summary>
public static class DistanceCalculator
{
    /// <summary>
    /// Calculates distance between two coordinates in kilometers.
    /// Currently returns 0 as placeholder until location features are enabled.
    /// </summary>
    public static double CalculateDistance(double? lat1, double? lon1, double? lat2, double? lon2)
    {
        // Skeleton mode: return 0 if coordinates are not available
        if (!lat1.HasValue || !lon1.HasValue || !lat2.HasValue || !lon2.HasValue)
            return 0;

        // TODO: Uncomment when ready to enable location features
        /*
        const double R = 6371; // Earth radius in kilometers
        
        var dLat = ToRadians(lat2.Value - lat1.Value);
        var dLon = ToRadians(lon2.Value - lon1.Value);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1.Value)) * Math.Cos(ToRadians(lat2.Value)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
        */
        
        return 0; // Placeholder
    }

    // Helper method for Haversine formula (commented out)
    /*
    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
    */
}
