using System;

namespace WinRepoSearch.Core.Models
{
    public record InnerItem
    {
        private DateTimeOffset _timestamp;
        private string _message;

        public DateTimeOffset Timestamp { get => _timestamp; init => _timestamp = value; }
        public string Message { get => _message; init => _message = value; }

        public InnerItem(DateTimeOffset timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Timestamp, Message);
        }

        public void Deconstruct(out DateTimeOffset timestamp, out string message)
        {
            timestamp = Timestamp;
            message = Message;
        }
    }
}
