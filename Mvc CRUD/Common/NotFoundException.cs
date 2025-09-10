namespace Mvc_CRUD.Common;

    public class NotFoundException : Exception
    {
      public NotFoundException(string message) : base(message) { }
    }

