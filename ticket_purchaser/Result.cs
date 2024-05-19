using System;

namespace res
{
    public class Result<TVal, TErr>
    {
        public readonly TVal? Value;
        public readonly TErr? Error;
        private readonly bool isSuccess = false;

        public Result(TVal value)
        {
            isSuccess = true;
            Value = value;
            Error = default;
        }

        public Result(TErr error)
        {
            isSuccess = false;
            Value = default;
            Error = error;
        }

        public Result<TVal, TErr> Match(Func<TVal, Result<TVal, TErr>> onSucces, Func<TErr, Result<TVal, TErr>> onFailure)
        {
            return isSuccess ? onSucces(Value!) : onFailure(Error!);
        }

        public void Match(Action<TVal> onSucces, Action<TErr> onFailure)
        {
            if (isSuccess)
                onSucces(Value!);
            else 
                onFailure(Error!);
        }

        public object Match(Func<TVal, object> onSucces, Func<TErr, object> onFailure)
        {
            return isSuccess ? onSucces(Value!) : onFailure(Error!);
        }
    }

    public sealed record ResultError<TErrorType>(TErrorType Code, string Message)
    {
    }
}
