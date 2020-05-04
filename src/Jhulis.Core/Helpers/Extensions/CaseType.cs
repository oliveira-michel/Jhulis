using System.ComponentModel;

namespace Jhulis.Core.Helpers.Extensions
{
    public enum CaseType
    {
        None,
        [Description("camelCase")]
        CamelCase,
        [Description("kebab-case")]
        KebabCase,
        [Description("PascalCase")]
        PascalCase,
        [Description("snake_case")]
        SnakeCase
    }
}
