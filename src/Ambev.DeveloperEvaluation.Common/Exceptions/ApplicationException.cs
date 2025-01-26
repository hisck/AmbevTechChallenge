namespace Ambev.DeveloperEvaluation.Common.Exceptions
{
    public class AppException : Exception
    {
        public string Type { get; }

        public AppException(string type, string message)
            : base(message)
        {
            Type = type;
        }
    }
    public class ValidationEx : AppException
    {
        public ValidationEx(string message)
            : base("ValidationError", message)
        {
        }
    }

    public class ResourceNotFoundException : AppException
    {
        public ResourceNotFoundException(string message)
            : base("ResourceNotFound", message)
        {
        }
    }

    public class BusinessRuleException : AppException
    {
        public BusinessRuleException(string message)
            : base("BusinessRuleViolation", message)
        {
        }
    }
}
