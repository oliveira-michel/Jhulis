namespace Jhulis.Core
{
    public class RuleSettings
    {
        public ContentEnvelopeConfig ContentEnvelope { get; set; }
        public DescriptionConfig Description { get; set; }
        public ErrorResponseFormatConfig ErrorResponseFormat { get; set; }
        public Http200WithoutPaginationConfig Http200WithoutPagination { get; set; }
        public IdPropertyResponseConfig IdPropertyResponse { get; set; }
        public MessagesEnvelopeFormatConfig MessagesEnvelopeFormat { get; set; }
        public NestingDepthConfig NestingDepth { get; set; }
        public PaginationEnvelopeFormatConfig PaginationEnvelopeFormat { get; set; }
        public PathCaseConfig PathCase { get; set; }
        public PathParameterConfig PathParameter { get; set; }
        public PathWithCrudNamesConfig PathWithCrudNames { get; set; }
        public PropertyCaseConfig PropertyCase { get; set; }
        public PropertyStartingWithTypeConfig PropertyStartingWithType { get; set; }
        public StringCouldBeNumberConfig StringCouldBeNumber { get; set; }
        public ValidResponseCodesConfig ValidResponseCodes { get; set; }
        public VersionFormatConfig VersionFormat { get; set; }
        public HealthCheckConfig HealthCheckPaths { get; set; }
    }

    public class ContentEnvelopeConfig
    {
        public string EnvelopeName { get; set; }
    }

    public class DescriptionConfig
    {
        public int MinDescriptionLength { get; set; }
        public int MidDescriptionLength { get; set; }
        public int LargeDescriptionLength { get; set; }
        public bool TestDescriptionInPaths { get; set; }
        public bool TestDescriptionInOperation { get; set; }
    }

    public class ErrorResponseFormatConfig
    {
        public string ObligatoryErrorProperties { get; set; }
        public string NonObligatoryErrorProperties { get; set; }
    }

    public class Http200WithoutPaginationConfig
    {
        public string PaginationEnvelopeName { get; set; }
        public string ContentEnvelopeName { get; set; }
    }
    
    public class IdPropertyResponseConfig
    {
        public string IdPropertyName { get; set; }    
    }
    
    public class MessagesEnvelopeFormatConfig
    {
        public string MessagesEnvelopeProperties { get; set; }
        public string MessagesEnvelopePropertyName { get; set; }
    }

    public class NestingDepthConfig
    {
        public int Depth { get; set; }
    }
    
    public class PaginationEnvelopeFormatConfig
    {
        public string PropertiesInPagination { get; set; }
        public string PaginationEnvelopePropertyName { get; set; }
    }
    
    public class PathCaseConfig
    {
        public string CaseType { get; set; }
        public bool CaseTypeTolerateNumber { get; set; }
        public string Example { get; set; }
    }
    
    public class PathParameterConfig
    {
        public double MatchEntityNamePercentage { get; set; }
        public string Regex { get; set; }
        public string PrefixToRemove { get; set; }
        public string SufixToRemove { get; set; }
        public string HumanReadeableFormat { get; set; }
        public string CaseType { get; set; }
        public string Example { get; set; }
    }
   
    public class PathWithCrudNamesConfig
    {
        public string WordsToAvoid { get; set; }
    }
    
    public class PropertyCaseConfig
    {
        public string CaseType { get; set; }
        public bool CaseTypeTolerateNumber { get; set; }
        public string Example { get; set; }
    }
    
    public class PropertyStartingWithTypeConfig
    {
        public string WordsToAvoid { get; set; } 
    }

    public class StringCouldBeNumberConfig
    {
        public string CurrencySymbols { get; set; }
    }

    public class ValidResponseCodesConfig
    {
        public string ValidHttpCodes { get; set; }
    }
    
    public class VersionFormatConfig {
        public string RegexExpectedFormat { get; set; }
        public string HumanReadeableFormat { get; set; }
        public string Example { get; set; }
    }

    public class HealthCheckConfig
    {
        public string Regex { get; set; }
    }
}
