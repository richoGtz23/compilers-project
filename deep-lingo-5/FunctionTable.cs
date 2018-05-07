using System;
using System.Text;
using System.Collections.Generic;

namespace DeepLingo
{
    public class FunctionTable : IEnumerable<KeyValuePair<string, int>>
    {
        IDictionary<string, int> data = new Dictionary<string, int>();

        //-----------------------------------------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Function Table:\n");
            sb.Append("====================\n");
            foreach (var entry in data)
            {
                sb.Append(String.Format("{0}: {1}\n", entry.Key, entry.Value));
            }
            sb.Append("====================\n");
            return sb.ToString();
        }

        //-----------------------------------------------------------
        public int this[string key]
        {
            get
            {
                return data[key];
            }
            set
            {
                data[key] = value;
            }
        }

        //-----------------------------------------------------------
        public bool Contains(string key)
        {
            return data.ContainsKey(key);
        }

        //-----------------------------------------------------------
        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        //-----------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
