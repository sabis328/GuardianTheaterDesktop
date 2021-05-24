using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Guardian_Theater_Desktop
{
    public class GuardianCacher
    {

    }

    [Serializable]
    public class CachedUser
    {
        public string DisplayName { get; set; }

        public string AccountIdentifier { get; set; }

        public string TwitchName { get; set; }

        public bool LinkedTwitch { get; set; }

    }
}
