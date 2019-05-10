using System;

namespace Calculate_Probabilities
{
    public class Probabilities_type
    {
        public Probabilities_type(string format)
        {
            var split = format.Split('x');
            SetCount = GetStrToSetField(split[0]);
            SetMemberCount = GetStrToSetField(split[1]);

        }

        private int GetStrToSetField(string v)
        {
            if (v.Equals("RND", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Random(DateTime.Now.Millisecond).Next(99);
            }
            else
                return Convert.ToInt32(v);
        }

        public int SetMemberCount { get; set; } = 3;
        public int SetCount { set; get; } = 3;
    }
}
