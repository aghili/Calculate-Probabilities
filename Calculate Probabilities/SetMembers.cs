using System;
using System.Collections.Generic;

namespace Calculate_Probabilities
{
    public class SetMembers:IEquatable<SetMembers>
    {
        public SetMembers()
        {
        }
        public SetMembers(string value)
        {
            Members = new List<string>(value.Split(','));
        }

        public SetMembers(SetMembers output)
        {
            this.Members.AddRange( output.Members);
        }

        public List<string> Members { set; get; } = new List<string>();

        public bool Equals(SetMembers other)
        {
            if (Members.Count != other.Members.Count)
                return false;
            for (int index = 0; index < Members.Count; index++)
                if (!Members[index].Equals(other.Members[index]))
                    return false;
            return true;
        }

        public override string ToString()
        {
            return string.Join(",",Members);
        }
    }
}
