namespace RansauSysteme.Result
{
    /// <summary>
    /// Represents the result of an operation that can either succeed with data or fail with an error message.
    /// </summary>
    /// <typeparam name="T">The type of data contained in a successful result.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the data returned by the operation, if successful.
        /// </summary>
        public T? Data { get; }

        /// <summary>
        /// Gets the error message if the operation failed.
        /// </summary>
        public string? Error { get; }

        private Result(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result containing the provided data.
        /// </summary>
        /// <param name="data">The data to include in the result.</param>
        /// <returns>A successful Result instance.</returns>
        public static Result<T> Success(T data) => new(true, data, null);

        /// <summary>
        /// Creates a failed result with an optional error message and data.
        /// </summary>
        /// <param name="error">The error message describing why the operation failed.</param>
        /// <param name="data">Optional data to include with the failure.</param>
        /// <returns>A failed Result instance.</returns>
        public static Result<T> Failure(string error = "", T? data = default) => new(false, data, error);

        /// <summary>
        /// Implicitly converts a successful result to its contained data type.
        /// </summary>
        /// <param name="result">The result to convert.</param>
        public static implicit operator T?(Result<T> result) => result.Data;
    }
}