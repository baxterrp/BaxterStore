using System.Collections.Generic;

namespace BaxterStore.Data.Exceptions
{
    public class ApiExceptionContainer
    {
        public List<string> Messages { get; }
        public ApiExceptionContainer(string message)
        {
            Messages = new List<string>
            {
                message
            };
        }

        public void AppendMessage(string message)
        {
            Messages.Add(message);
        }
    }
}
