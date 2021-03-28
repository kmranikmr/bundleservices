using Scrypt;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utils
{
    public class Api
    {
        public static string GenerateHashedKey(string apiKey)
        {
            ScryptEncoder encoder = new ScryptEncoder();

            string hashsedKey = encoder.Encode(apiKey);

            return hashsedKey;
        }

        public static bool MatchHashedKey(string apiKeyFromUser, string hashedApiKeyFromDb)
        {
            ScryptEncoder encoder = new ScryptEncoder();

            bool areEquals = encoder.Compare(apiKeyFromUser, hashedApiKeyFromDb);

            return areEquals;
        }        
    }
}
