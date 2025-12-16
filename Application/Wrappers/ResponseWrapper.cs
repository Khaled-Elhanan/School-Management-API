namespace Application.Wrappers;

public class ResponseWrapper : IResponseWrapper
{
    public bool IsSuccessful { get; set; }
    public List<string> Messages { get; set; } = new();

    // Success
    public static ResponseWrapper Success(string message = null)
    {
        return new ResponseWrapper
        {
            IsSuccessful = true,
            Messages = message != null ? new List<string> { message } : new List<string>()
        };
    }

    public static ResponseWrapper Success(List<string> messages)
    {
        return new ResponseWrapper
        {
            IsSuccessful = true,
            Messages = messages
        };
    }

    // Fail
    public static ResponseWrapper Fail()
    {
        return new ResponseWrapper
        {
            IsSuccessful = false,
            Messages = new List<string>()
        };
    }

    public static ResponseWrapper Fail(string message)
    {
        return new ResponseWrapper
        {
            IsSuccessful = false,
            Messages = new List<string> { message }
        };
    }

    public static ResponseWrapper Fail(List<string> messages)
    {
        return new ResponseWrapper
        {
            IsSuccessful = false,
            Messages = messages
        };
    }

    // Async versions
    public static Task<ResponseWrapper> SuccessAsync(string message = null)
        => Task.FromResult(Success(message));

    public static Task<ResponseWrapper> FailAsync()
        => Task.FromResult(Fail());

    public static Task<ResponseWrapper> FailAsync(string message)
        => Task.FromResult(Fail(message));
}


public class ResponseWrapper<T> : ResponseWrapper, IResponseWrapper<T>
{
    public T Data { get; set; }

    // Success
    public static ResponseWrapper<T> Success(T data, string message = null)
    {
        return new ResponseWrapper<T>
        {
            IsSuccessful = true,
            Data = data,
            Messages = message != null ? new List<string> { message } : new List<string>()
        };
    }

    public static ResponseWrapper<T> Success(T data, List<string> messages)
    {
        return new ResponseWrapper<T>
        {
            IsSuccessful = true,
            Data = data,
            Messages = messages
        };
    }

    // Fail
    public static ResponseWrapper<T> Fail()
    {
        return new ResponseWrapper<T>
        {
            IsSuccessful = false,
            Messages = new List<string>(),
            Data = default
        };
    }

    public static ResponseWrapper<T> Fail(string message)
    {
        return new ResponseWrapper<T>
        {
            IsSuccessful = false,
            Messages = new List<string> { message },
            Data = default
        };
    }

    public static ResponseWrapper<T> Fail(List<string> messages)
    {
        return new ResponseWrapper<T>
        {
            IsSuccessful = false,
            Messages = messages,
            Data = default
        };
    }

    // Async
    public static Task<ResponseWrapper<T>> SuccessAsync(T data, string message = null)
        => Task.FromResult(Success(data, message));

    public static Task<ResponseWrapper<T>> FailAsync()
        => Task.FromResult(Fail());

    public static Task<ResponseWrapper<T>> FailAsync(string message)
        => Task.FromResult(Fail(message));
}

