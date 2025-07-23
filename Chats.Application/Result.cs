using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application
{
    public class Result
    {
        public int StatusCode { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

        public static Result Success(int statusCode = 200)
            => new Result { StatusCode = statusCode };

        public static Result Failure(int statusCode, string errorMessage)
            => new Result { StatusCode = statusCode, ErrorMessage = errorMessage };
    }
}
