using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Domain
{
    public class Result<T>
    {
        public bool Succeeded { get; set; } = true;
        public string Error { get; set; }
        public T Value { get; set; }

        public Result() { }

        public Result(T value)
        {
            Succeeded = true;
            Value = value;
        }

        public Result<T> Abort(string error)
        {
            Error = error;
            Succeeded = false;
            return this;
        }

        public void Merge(Result<T> result)
        {
            if (!result.Succeeded)
            {
                Succeeded = false;
                Error = result.Error;
                Value = result.Value;
            }
        }
    }

    public class Result
    {
        public bool Succeeded { get; set; } = true;
        public string Error { get; set; }

        public Result Abort(string error)
        {
            Error = error;
            Succeeded = false;
            return this;
        }

        public void Merge(Result result)
        {
            if (!result.Succeeded)
            {
                Succeeded = false;
                Error = result.Error;
            }
        }
    }
}
