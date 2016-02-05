using System;
using System.Configuration;
using System.Text.RegularExpressions;
using ivNet.Club.Enums;

namespace ivNet.Club.Helpers
{
    public class CustomStringHelper
    {
        public static string BuildKey(string[] items)
        {
            var key = String.Join(string.Empty, items).ToLowerInvariant();
            return Regex.Replace(key, "[^0-9a-z]", string.Empty);
        }

        public static string GenerateInitialPassword(string firstname)
        {
            return string.Format("{0}{1}1",
              firstname.ToLowerInvariant(),
              firstname.ToUpperInvariant())
              .Replace(" ", string.Empty);
        }

        public static string GetAppSettingValue(ConfigKey key)
        {
            switch (key)
            {
                case ConfigKey.RecaptchaKey:
                    return ConfigurationManager.AppSettings["RecaptchaPrivateKey"];

                case ConfigKey.RecaptchaUrl:
                    return ConfigurationManager.AppSettings["RecaptchaUrl"];

            }

            return string.Empty;
        }

        //public static string GenerateInitialPassword(Owner owner)
        //{
        //    return string.Format("{0}{1}1",
        //      owner.Firstname.ToLowerInvariant(),
        //      owner.Firstname.ToUpperInvariant())
        //      .Replace(" ", string.Empty);
        //}
    }
}