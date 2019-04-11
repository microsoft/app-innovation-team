using System;

namespace RF.ContentSearch.Domain.Exceptions
{
    public class BusinessException : Exception
    {
        public int ResultId { get; set; }

        public BusinessException()
        {
        }

        public BusinessException(int resultId)
        {
            this.ResultId = resultId;
        }

        public BusinessException(string message)
            : base(message)
        {
        }

        public BusinessException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}