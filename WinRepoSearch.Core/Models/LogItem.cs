using System;
using System.Collections.Generic;

namespace WinRepoSearch.Core.Models
{
    public record LogItem
    {
        internal IEnumerable<SearchResult> _result= Array.Empty<SearchResult>();
        private InnerItem[] _log = Array.Empty<InnerItem>();

        public IEnumerable<SearchResult> Result { get => _result; init => _result = value; }
        public InnerItem[] Log { get => _log; set => _log = value; }

        public static LogItem Empty => new LogItem(Array.Empty<SearchResult>(), Array.Empty<InnerItem>());

        public LogItem(IEnumerable<SearchResult> result, InnerItem[] log)
        {
            Result = result;
            Log = log;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Result, Log);
        }

        public void Deconstruct(out IEnumerable<SearchResult>? result, out InnerItem[]? log)
        {
            result = this.Result;
            log = this.Log;
        }
    }
}
