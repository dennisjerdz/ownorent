using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ownorent
{
    public static class OwnorentHelper
    {
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}