namespace Mvc_CRUD.Common;

    public class Results<T>
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public T? Data { get; set; }

        public static Results<T> Ok(T data) => new Results<T> { Success = true, Data = data };

        public static Results<T> Fail(string error) => new Results<T> { Success = false, Error = error };
    }

