using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Application
{
    public class Result<T> : Result
    {
        public T? Value { get; private set; }

        public static Result<T> Success(T value, int statusCode = 200)
            => new Result<T> { Value = value, StatusCode = statusCode };

        public static Result<T> Failure(int statusCode, string errorMessage, T? value = default)
            => new Result<T> { StatusCode = statusCode, ErrorMessage = errorMessage, Value = value };
    }
}
