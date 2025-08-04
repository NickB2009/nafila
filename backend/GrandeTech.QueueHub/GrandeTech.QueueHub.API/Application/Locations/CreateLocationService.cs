using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Grande.Fila.API.Application.Locations.Requests;
using Grande.Fila.API.Application.Locations.Results;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Domain.Locations;
using DomainLocation = Grande.Fila.API.Domain.Locations.Location;

namespace Grande.Fila.API.Application.Locations;

public class CreateLocationService
{
    private readonly ILocationRepository _locationRepository;

    public CreateLocationService(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
    }

    public async Task<CreateLocationResult> ExecuteAsync(
        CreateLocationRequest request, 
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        var result = new CreateLocationResult();

        // Validate input
        var validationErrors = ValidateRequest(request);
        if (validationErrors.Any())
        {
            result.FieldErrors = validationErrors;
            return result;
        }

        try
        {
            // Generate location slug
            var locationSlug = GenerateLocationSlug(request.BusinessName, request.Address.State);

            // Check for existing slug
            var existingLocation = await _locationRepository.GetBySlugAsync(locationSlug, cancellationToken);
            if (existingLocation != null)
            {
                // Try with a suffix
                var counter = 1;
                string uniqueSlug;
                do
                {
                    uniqueSlug = $"{locationSlug}-{counter}";
                    existingLocation = await _locationRepository.GetBySlugAsync(uniqueSlug, cancellationToken);
                    counter++;
                } while (existingLocation != null && counter <= 10);

                if (existingLocation != null)
                {
                    result.Errors.Add("Unable to generate unique location identifier. Please try a different business name.");
                    return result;
                }
                locationSlug = uniqueSlug;
            }

            // Create address value object
            var address = Address.Create(
                street: request.Address.Street,
                number: "", // Extract from street if needed
                complement: "",
                neighborhood: "",
                city: request.Address.City,
                state: request.Address.State,
                country: request.Address.Country,
                postalCode: request.Address.PostalCode
            );

            // Parse business hours - for now, use default 8AM-6PM
            var openingTime = TimeSpan.Parse("08:00");
            var closingTime = TimeSpan.Parse("18:00");

            if (request.BusinessHours.ContainsKey("Monday"))
            {
                var hours = request.BusinessHours["Monday"];
                if (TryParseBusinessHours(hours, out var opening, out var closing))
                {
                    openingTime = opening;
                    closingTime = closing;
                }
            }

            // Note: The Location constructor will automatically create WeeklyBusinessHours
            // with Monday-Saturday using these hours and Sunday closed by default.
            // In the future, we can enhance this to read individual day hours from request.BusinessHours.

            // For now, use a default organization ID - this will be from the authenticated user's context
            var organizationId = Guid.NewGuid();

            // Create location
            var location = new DomainLocation(
                name: request.BusinessName,
                slug: locationSlug,
                description: request.Description ?? "",
                organizationId: organizationId,
                address: address,
                contactPhone: request.ContactPhone,
                contactEmail: request.ContactEmail,
                openingTime: openingTime,
                closingTime: closingTime,
                maxQueueSize: request.MaxQueueCapacity,
                lateClientCapTimeInMinutes: 15, // Default
                createdBy: currentUserId
            );

            // Save to repository
            await _locationRepository.AddAsync(location, cancellationToken);

            // Return success result
            result.Success = true;
            result.LocationId = location.Id.ToString();
            result.LocationSlug = locationSlug;
            result.BusinessName = request.BusinessName;

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"An error occurred while creating the location: {ex.Message}");
            return result;
        }
    }

    private Dictionary<string, string> ValidateRequest(CreateLocationRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.BusinessName))
            errors["BusinessName"] = "Business name is required.";

        if (!string.IsNullOrWhiteSpace(request.ContactEmail) && !IsValidEmail(request.ContactEmail))
            errors["ContactEmail"] = "Contact email format is invalid.";

        if (string.IsNullOrWhiteSpace(request.ContactPhone))
            errors["ContactPhone"] = "Contact phone is required.";

        if (string.IsNullOrWhiteSpace(request.Address.Street))
            errors["Address.Street"] = "Street address is required.";

        if (string.IsNullOrWhiteSpace(request.Address.City))
            errors["Address.City"] = "City is required.";

        if (string.IsNullOrWhiteSpace(request.Address.State))
            errors["Address.State"] = "State is required.";

        if (string.IsNullOrWhiteSpace(request.Address.PostalCode))
            errors["Address.PostalCode"] = "Postal code is required.";

        if (string.IsNullOrWhiteSpace(request.Address.Country))
            errors["Address.Country"] = "Country is required.";

        if (request.MaxQueueCapacity <= 0)
            errors["MaxQueueCapacity"] = "Maximum queue capacity must be greater than zero.";

        return errors;
    }

    private string GenerateLocationSlug(string businessName, string state)
    {
        // Remove accents and special characters
        var normalizedName = RemoveAccents(businessName.ToLowerInvariant());
        
        // Replace spaces and special characters with hyphens
        normalizedName = Regex.Replace(normalizedName, @"[^a-z0-9]+", "-");
        
        // Remove leading/trailing hyphens
        normalizedName = normalizedName.Trim('-');
        
        // Add state abbreviation
        var stateAbbrev = state.ToLowerInvariant();
        
        return $"{normalizedName}-{stateAbbrev}";
    }

    private string RemoveAccents(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool TryParseBusinessHours(string hoursString, out TimeSpan opening, out TimeSpan closing)
    {
        opening = default;
        closing = default;

        if (string.IsNullOrWhiteSpace(hoursString))
            return false;

        var parts = hoursString.Split('-');
        if (parts.Length != 2)
            return false;

        return TimeSpan.TryParse(parts[0].Trim(), out opening) && 
               TimeSpan.TryParse(parts[1].Trim(), out closing);
    }
}
