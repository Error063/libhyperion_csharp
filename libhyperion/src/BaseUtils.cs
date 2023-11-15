using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace libhyperion;

public class BaseUtils
{
    public static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }
    public static string GenerateRandomString(int n)
    {
        var random = new Random();
        const string characters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(characters, n)
            .Select(s => s[random.Next(s.Length)]).ToArray()).ToLower();
    }
    public static string GenerateRandomNumberString(int n)
    {
        var random = new Random();
        const string characters = "0123456789";
        return new string(Enumerable.Repeat(characters, n)
            .Select(s => s[random.Next(s.Length)]).ToArray()).ToLower();
    }
    public static string CalculateMD5(string text)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(text);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // Convert each byte to a hexadecimal string
            }

            return sb.ToString();
        }
    }
    public static string GenerateMysDS1(string salt = "lk2")
    {
        string appSalt = "";
        switch (salt.ToLower())
        {
            case "lk2":
                appSalt = AppVars.SaltLK2;
                break;
            case "k2":
                appSalt = AppVars.SaltK2;
                break;
            case "4x":
                appSalt = AppVars.Salt4X;
                break;
            case "6x":
                appSalt = AppVars.Salt6X;
                break;
            case "prod":
                appSalt = AppVars.SaltPROD;
                break;
        }
        string n = appSalt;
        string i = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        string r = GenerateRandomString(6);
        string c = CalculateMD5($"salt={n}&t={i}&r={r}");
        return $"{i},{r},{c}";
    }
    public static string GenerateMysDS2(string query = "", string body = "", string salt = "4x")
    {
        if (salt.ToLower() == "4x")
            salt = AppVars.Salt4X;
        else if (salt.ToLower() == "6x")
            salt = AppVars.Salt6X;
        else if (salt.ToLower() == "prod")
            salt = AppVars.SaltPROD;

        long t = DateTimeOffset.Now.ToUnixTimeSeconds();
        int r = new Random().Next(100001, 200000);

        if (!string.IsNullOrEmpty(body))
        {
            body = JsonConvert.SerializeObject(body, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        if (!string.IsNullOrEmpty(query))
            query = string.Join("&", query.Split('&').OrderBy(s => s));

        string main = $"salt={salt}&t={t}&r={r}&b={body}&q={query}";
        string ds = CalculateMD5(main);

        return $"{t},{r},{ds}";
    }
    public static Dictionary<string, string> GenerateHeader(string client = "4", bool withCookie = true,
        int agro = 1, string query = "", string body = "",
        string saltAgro1 = "lk2", string saltAgro2 = "4x", string referer = "https://www.miyoushe.com/",
        int stokenVer = 1, int ltokenVer = 1, string cookie="")
    {

        string userAgent = client == "2" ? "okhttp/4.8.0" :
            $"Mozilla/5.0 (Linux; Android 12; vivo-s7 Build/RKQ1.211119.001; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/105.0.5195.79 Mobile Safari/537.36 miHoYoBBS/{AppVars.MysVersion}";

        string ds = agro == 1 ? GenerateMysDS1(saltAgro1) : GenerateMysDS2(query, body, saltAgro2);

        return new Dictionary<string, string>
        {
            { "Cookie", withCookie ? cookie : "" },
            { "User-Agent", userAgent },
            { "Dnt", "1" },
            { "DS", ds },
            { "x-rpc-client_type", client },
            { "x-rpc-app_version", AppVars.MysVersion },
            { "X-Requested-With", "com.mihoyo.hyperion" },
            { "Origin", "https://webstatic.mihoyo.com" },
            { "x-rpc-device_name", "vivo s7" },
            { "x-rpc-device_model", "vivo-s7" },
            { "x-rpc-sys_version", "12" },
            { "x-rpc-channel", "miyousheluodi" },
            { "x-rpc-verify_key", "bll8iq97cem8" },
            { "Referer", referer }
        };
    }

    public static Dictionary<string, string> AppendFp(Dictionary<string, string> header)
    {
        string deviceId = GenerateRandomString(16);
        header.Add("x-rpc-device_id", deviceId);
        JObject body = new JObject()
        {
            { "device_id", header["x-rpc-device_id"] },
            { "seed_id", GenerateRandomString(16).ToLower()},
            { "seed_time", GetTimeStamp() },
            { "platform", "2" },
            { "device_fp", GenerateRandomNumberString(10) },
            { "app_name", "bbs_cn" },
            {
                "ext_fields",
                @"{""productName"":""mmm"", ""board"": ""fghjm"", ""ramCapacity"": ""114514"", ""deviceInfo"": ""eftyh"", ""hardware"": ""ertyh"", ""display"": ""ertyh"", ""buildTime"": ""16918185684"", ""hostname"": ""gdhntyrn"", ""brand"": ""hetyhertyhomo""}"
            }
        };
        HttpUtils httpUtils = new HttpUtils();
        string fp = JObject.Parse(httpUtils.PostJsonAsync(ApiUrls.getFp, body.ToString()!).Result.ToString())["data"]["device_fp"].ToString()!;
        header.Add("x-rpc-device_fp", fp);

        return header;
    }
    public static string Encrypt(string message)
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportFromPem(AppVars.PublicKey);

            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(message);
            byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false);

            return Convert.ToBase64String(encryptedData);
        }
    }
}