﻿using System;

namespace res
{
    public class Result<Val, Err>
    {
        public readonly Val? Value;
        public readonly Err? Error;

        private readonly bool isSuccess = false;

        public Result(Val value)
        {
            isSuccess = true;
            Value = value;
            Error = default;
        }
        public Result(Err error)
        {
            isSuccess = false;
            Value = default;
            Error = error;
        }

        public Result<Val, Err> Match(Func<Val, Result<Val, Err>> onSucces, Func<Err, Result<Val, Err>> onFailure)
        {
            return isSuccess ? onSucces(Value!) : onFailure(Error!);
        }
    }

    public sealed record Error<ErrorType>(ErrorType Code, string Message) where ErrorType : Enum
    {
    }
}