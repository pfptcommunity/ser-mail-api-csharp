using System.Reflection;

namespace Proofpoint.SecureEmailRelay.Mail
{
    /// <summary>
    /// Attribute to associate a string value with an enum field.
    /// </summary>
    /// <remarks>
    /// This attribute is used to decorate enum fields with a specific string, such as a URI or identifier,
    /// which can be retrieved at runtime using reflection.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    internal class StringValueAttribute : Attribute
    {
        /// <summary>
        /// Gets the string value associated with the attribute.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The string value to associate with the enum field.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public StringValueAttribute(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// Represents different regions for the Secure Email Relay service.
    /// </summary>
    /// <remarks>
    /// Each enum value is decorated with a <see cref="StringValueAttribute"/> that specifies
    /// the corresponding region's API endpoint URI.
    /// </remarks>
    public enum Region
    {
        /// <summary>
        /// United States region, associated with the endpoint "mail-us.ser.proofpoint.com".
        /// </summary>
        [StringValue("mail-us.ser.proofpoint.com")]
        US,

        /// <summary>
        /// European Union region, associated with the endpoint "mail-eu.ser.proofpoint.com".
        /// </summary>
        [StringValue("mail-eu.ser.proofpoint.com")]
        EU,

        /// <summary>
        /// Australia region, associated with the endpoint "mail-aus.ser.proofpoint.com".
        /// </summary>
        [StringValue("mail-aus.ser.proofpoint.com")]
        AU
    }

    /// <summary>
    /// Provides extension methods for the <see cref="Region"/> enum.
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the string value associated with a <see cref="Region"/> enum value.
        /// </summary>
        /// <param name="region">The <see cref="Region"/> enum value to get the string for.</param>
        /// <returns>The string value associated with the enum, as defined by the <see cref="StringValueAttribute"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when no <see cref="StringValueAttribute"/> is found for the specified <paramref name="region"/>.</exception>
        public static string GetStringValue(this Region region)
        {
            var field = region.GetType().GetField(region.ToString());
            var attribute = field?.GetCustomAttribute<StringValueAttribute>();
            return attribute?.Value ?? throw new ArgumentException($"No StringValueAttribute found for {region}");
        }
    }
}