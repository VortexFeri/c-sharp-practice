using System;

namespace res
{
    public class Result<TVal, TErr>
    {
        public readonly TVal? Value;
        public readonly TErr? Error;
        public readonly bool IsSuccess = false;

        public Result(TVal value)
        {
            IsSuccess = true;
            Value = value;
            Error = default;
        }

        public Result(TErr error)
        {
            IsSuccess = false;
            Value = default;
            Error = error;
        }

        public Result<TVal, TErr> Match(Func<TVal, Result<TVal, TErr>> onSucces, Func<TErr, Result<TVal, TErr>> onFailure)
        {
            return IsSuccess ? onSucces(Value!) : onFailure(Error!);
        }

        public void Match(Action<TVal> onSucces, Action<TErr> onFailure)
        {
            if (IsSuccess)
                onSucces(Value!);
            else 
                onFailure(Error!);
        }

        public object Match(Func<TVal, object> onSucces, Func<TErr, object> onFailure)
        {
            return IsSuccess ? onSucces(Value!) : onFailure(Error!);
        }
    }

    public sealed record ResultError<TErrorType>(TErrorType Code, string Message)
    {
    }
}
