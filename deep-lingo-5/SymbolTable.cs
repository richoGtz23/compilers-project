using System;
using System.Text;
using System.Collections.Generic;

namespace DeepLingo {
    public class SymbolTable {
        public HashSet<string> data;
        
        public SymbolTable(){
            data = new HashSet<string>();
        }
         public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("Symbol Table:\n");
            sb.Append("====================\n");
            foreach (var entry in data) {
                sb.Append(String.Format("{0}, ", entry));
            }
            sb.Append("\n====================\n");
            return sb.ToString();
        }
        
        public void Add(string nvalue){
            data.Add(nvalue);
        }
         public bool Contains(string key) {
            return data.Contains(key);
        }

        //-----------------------------------------------------------
        public IEnumerator<string> GetEnumerator() {
            return data.GetEnumerator();
        }
        
    }
}