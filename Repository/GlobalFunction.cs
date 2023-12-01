using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace WebApp.Repository
{
    public class GlobalFunction
    {
        private const string SIGNATURE_PARAMETERS = "signature";
        private const int ITERATION_COUNT = 10;
        
        public string JoinParams(Dictionary<string, string> paramMap, string appSecret)
        {
            string result = "";

            foreach (var entry in paramMap)
            {
                result += "&" + entry.Key + "=" + UrlEncryptStandard(entry.Value);
            }

            result += "&" + SIGNATURE_PARAMETERS + "=" + GenerateSignatureStandard(paramMap, appSecret);

            return result;
        }

        public string Stringify(Dictionary<string, string> paramMap, string appSecret)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (var entry in paramMap)
            {
                map[entry.Key] = UrlEncryptStandard(entry.Value);
            }
            map[SIGNATURE_PARAMETERS] = GenerateSignatureStandard(paramMap, appSecret);
            string jsonString = System.Text.Json.JsonSerializer.Serialize(map);
            return jsonString;
        }

        public string GenerateSignatureStandard(Dictionary<string, string> paramMap, string salt)
        {
            List<string> queryStringList = new List<string>();

            foreach (var param in paramMap)
            {
                if (!param.Key.Equals("signature", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        queryStringList.Add($"{param.Key}={param.Value}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }

            queryStringList.Sort();
            string joined = string.Join("&", queryStringList);

            return queryStringList != null && queryStringList.Count > 0
                ? MD5Hash(joined, salt)
                : string.Empty;
        }


        public string UrlEncryptStandard(string input)
        {
            try
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                string encodedString = Convert.ToBase64String(inputBytes);
                return encodedString;
            }
            catch (Exception ex)
            {
                throw new Exception("Error during URL encryption: " + ex.Message);
            }
        }

        public string MD5Hash(string inStr, string salt)
        {
            string tempHashResult = HashStr(inStr + salt, "MD5");
            for (int i = 0; i < ITERATION_COUNT; i++)
            {
                tempHashResult = HashStr(tempHashResult, "MD5");
            }
            return tempHashResult;
        }


        private string HashStr(string inStr, string algorithm)
        {
            MD5? md = null;
            try
            {
                using (md = MD5.Create())
                {
                    if (md == null)
                    {
                        Console.WriteLine("Algorithm not supported: " + algorithm);
                        return string.Empty;
                    }

                    byte[] inByte = Encoding.UTF8.GetBytes(inStr);
                    byte[] outByte = md.ComputeHash(inByte);
                    StringBuilder sb = new StringBuilder();

                    foreach (byte b in outByte)
                    {
                        sb.Append(b.ToString("x2"));
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("inStr: " + inStr + ", algo: " + algorithm);
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public bool ValidEmailDataAnnotations(string input) => new EmailAddressAttribute().IsValid(input);
    }
}
