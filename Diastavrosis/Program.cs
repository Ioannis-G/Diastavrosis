using System;
using System.Globalization;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        while (true)
        {  // User Input Prompt
            string lineA = GetValidInput("Enter coordinates for Line Segment A:");
            string lineB = GetValidInput("Enter coordinates for Line Segment B:");

            // Print inputs for verification
            Console.WriteLine($"Line Segment A: {lineA}");
            Console.WriteLine($"Line Segment B: {lineB}");

            // Parse User Input
            var lineACoords = ParseCoordinates(lineA);
            var lineBCoords = ParseCoordinates(lineB);

            // Intersection Calculation
            var result = CalculateIntersection(lineACoords, lineBCoords);

            // Output Result
            Console.WriteLine(result);

            // Ask user if they want to reset or exit
            Console.WriteLine("Press 'R' to reset and input new lines. Press any other key to exit.");
            var key = Console.ReadKey().Key;
            if (key != ConsoleKey.R)
            {
                break;
            }
            // Clear the console for new input
            Console.Clear();
        }
    }
       
    static string GetValidInput(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine();
            if (IsValidInputFormat(input))
            {
                return input;
            }
            Console.WriteLine("Invalid input format. Please try again.");
        }
    }

    static bool IsValidInputFormat(string input)
    {
        // Regex to match the expected format
        string pattern = @"^[NS]\d{3}\.\d{2}\.\d{2}\.\d{3}:[EW]\d{3}\.\d{2}\.\d{2}\.\d{3}:[NS]\d{3}\.\d{2}\.\d{2}\.\d{3}:[EW]\d{3}\.\d{2}\.\d{2}\.\d{3}$";
        return Regex.IsMatch(input, pattern);
    }

    static (double lat, double lon)[] ParseCoordinates(string input)
    {
        var parts = input.Split(':');
        if (parts.Length != 4)
        {
            throw new FormatException("Invalid coordinate format: Expected 4 parts.");
        }

        // Ensure latitude parts have 'N' or 'S' and longitude parts have 'E' or 'W'
        if (!parts[0].Contains('N') && !parts[0].Contains('S'))
        {
            throw new FormatException("Invalid coordinate format: Missing latitude hemisphere in part 1.");
        }
        if (!parts[1].Contains('E') && !parts[1].Contains('W'))
        {
            throw new FormatException("Invalid coordinate format: Missing longitude hemisphere in part 2.");
        }
        if (!parts[2].Contains('N') && !parts[2].Contains('S'))
        {
            throw new FormatException("Invalid coordinate format: Missing latitude hemisphere in part 3.");
        }
        if (!parts[3].Contains('E') && !parts[3].Contains('W'))
        {
            throw new FormatException("Invalid coordinate format: Missing longitude hemisphere in part 4.");
        }

        return new (double lat, double lon)[]
        {
            ParseCoordinate(parts[0], parts[1]),
            ParseCoordinate(parts[2], parts[3])
        };
    }

    static (double lat, double lon) ParseCoordinate(string latCoord, string lonCoord)
    {
        try
        {

            if (string.IsNullOrEmpty(latCoord) || string.IsNullOrEmpty(lonCoord))
            {
                throw new FormatException("Invalid coordinate format: Coordinate string is empty.");
            }

            if (!latCoord.Contains('N') && !latCoord.Contains('S'))
            {
                throw new FormatException("Invalid coordinate format: Missing latitude hemisphere indicator.");
            }

            if (!lonCoord.Contains('E') && !lonCoord.Contains('W'))
            {
                throw new FormatException("Invalid coordinate format: Missing longitude hemisphere indicator.");
            }

            char latHemisphere = latCoord[0];
            char lonHemisphere = lonCoord[0];

            var latParts = latCoord.Substring(1).Split('.');
            var lonParts = lonCoord.Substring(1).Split('.');

            if (latParts.Length != 4 || lonParts.Length != 4)
            {
                throw new FormatException("Invalid coordinate format: Incomplete coordinate parts.");
            }

            double lat = ConvertToDecimalDegrees(latParts);
            double lon = ConvertToDecimalDegrees(lonParts);

            if (latHemisphere == 'S') lat = -lat;
            if (lonHemisphere == 'W') lon = -lon;

            return (lat, lon);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw new FormatException("Invalid coordinate format.", ex);
        }
    }
    static double ConvertToDecimalDegrees(string[] dmsParts)
    {
        try
        {

            double degrees = double.Parse(dmsParts[0], CultureInfo.InvariantCulture);
            double minutes = double.Parse(dmsParts[1], CultureInfo.InvariantCulture);
            double seconds = double.Parse(dmsParts[2], CultureInfo.InvariantCulture);
            double milliseconds = double.Parse(dmsParts[3], CultureInfo.InvariantCulture);

            return degrees + (minutes / 60) + (seconds / 3600) + (milliseconds / 3600000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in ConvertToDecimalDegrees: {ex.Message}");
            throw new FormatException("Invalid DMS format.", ex);
        }
    }

    static string FormatCoordinates((double lat, double lon) coord)
    {
        string latHemisphere = coord.lat >= 0 ? "N" : "S";
        string lonHemisphere = coord.lon >= 0 ? "E" : "W";
        return $"{latHemisphere}{ConvertToDMS(Math.Abs(coord.lat))}:{lonHemisphere}{ConvertToDMS(Math.Abs(coord.lon))}";
    }

    static string ConvertToDMS(double decimalDegrees)
    {
        int degrees = (int)decimalDegrees;
        double fractional = Math.Abs(decimalDegrees - degrees);
        int minutes = (int)(fractional * 60);
        double secondsFractional = (fractional * 60 - minutes) * 60;
        int seconds = (int)secondsFractional;
        double millisecondsFractional = (secondsFractional - seconds) * 1000;
        int milliseconds = (int)Math.Round(millisecondsFractional);

        // Ensure milliseconds are within the valid range
        if (milliseconds == 1000)
        {
            milliseconds = 0;
            seconds++;
        }

        if (seconds == 60)
        {
            seconds = 0;
            minutes++;
        }

        if (minutes == 60)
        {
            minutes = 0;
            degrees++;
        }

        return $"{degrees:D3}.{minutes:D2}.{seconds:D2}.{milliseconds:D3}";
    }

    static string CalculateIntersection((double lat, double lon)[] lineA, (double lat, double lon)[] lineB)
    {
        // Extract points
        var (x1, y1) = lineA[0];
        var (x2, y2) = lineA[1];
        var (x3, y3) = lineB[0];
        var (x4, y4) = lineB[1];

        // Calculate the denominators
        double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (denom == 0)
        {
            // Lines are parallel, check for coincidence
            if (AreLinesCoincident(lineA, lineB))
            {
                return "The lines are coincident.";
            }
            return "The lines are parallel.";
        }

        // Calculate the intersection point of the infinite lines
        double intersectX = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / denom;
        double intersectY = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / denom;

        // Check if the intersection point is within the bounds of both segments
        if (IsWithinBounds(intersectX, intersectY, x1, y1, x2, y2) && IsWithinBounds(intersectX, intersectY, x3, y3, x4, y4))
        {
            return $"Intersection at: {FormatCoordinates((intersectX, intersectY))}";
        }

        // If not within bounds, return the intersection point of the infinite lines
        return $"The segments do not intersect, but the lines intersect at: {FormatCoordinates((intersectX, intersectY))}";
    }

    static bool AreLinesCoincident((double lat, double lon)[] lineA, (double lat, double lon)[] lineB)
    {
        // Check if any point of lineB lies on lineA
        return IsPointOnLine(lineA, lineB[0]) || IsPointOnLine(lineA, lineB[1]);
    }

    static bool IsPointOnLine((double lat, double lon)[] line, (double lat, double lon) point)
    {
        var (x1, y1) = line[0];
        var (x2, y2) = line[1];
        var (px, py) = point;

        // Check if the point satisfies the line equation
        return (x2 - x1) * (py - y1) == (y2 - y1) * (px - x1);
    }

    static bool IsWithinBounds(double x, double y, double x1, double y1, double x2, double y2)
    {
        return (Math.Min(x1, x2) <= x && x <= Math.Max(x1, x2)) && (Math.Min(y1, y2) <= y && y <= Math.Max(y1, y2));
    }
}
