using Newtonsoft.Json.Linq;

namespace libhyperion;

public static class Authorization
{
    public static JObject GetMmt()
    {
        HttpUtils httpUtils = new HttpUtils();
        string result = httpUtils.GetAsync(string.Format(ApiUrls.mmt, BaseUtils.GetTimeStamp())).Result.ToString();
        JObject resultObject = JObject.Parse(result);
        return (JObject)resultObject["data"]["mmt_data"];
    }

    public static string GetLoginTicketByPassword(string username, string password, string mmtKey, string geetestJson="", bool cryptoPwd=true)
    {
        HttpUtils httpUtils = new HttpUtils();
        string pwd = cryptoPwd ? BaseUtils.Encrypt(password).ToString() : password.ToString();
        var postForm = new Dictionary<string, string>();
        postForm.Add("account", username);
        postForm.Add("password", pwd);
        postForm.Add("is_crypto", cryptoPwd.ToString());
        postForm.Add("mmt_key", mmtKey);
        postForm.Add("source", "user.mihoyo.com");
        postForm.Add("t", BaseUtils.GetTimeStamp());
        if (geetestJson != "")
        {
            try
            {
                JObject geetestJsonJObject = JObject.Parse(geetestJson);
                switch (geetestJsonJObject["version"].ToString())
                {
                    case "gt3":
                        postForm.Add("geetest_challenge", geetestJsonJObject["geetest_challenge"].ToString());
                        postForm.Add("geetest_validate", geetestJsonJObject["geetest_validate"].ToString());
                        postForm.Add("geetest_seccode", geetestJsonJObject["geetest_seccode"].ToString());
                        break;
                    case "gt4":
                        postForm.Add("geetest_v4_data", geetestJsonJObject["geetest_v4_data"].ToString());
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        string result = httpUtils.PostFormAsync(ApiUrls.login_pwd, postForm).Result.ToString();
        JObject resultObject = JObject.Parse(result);
        try
        {
            return resultObject["data"]!["account_info"]!["weblogin_token"]!.ToString();
        }
        catch (Exception e)
        {
            return "";
        }
    }

    public static class GetLoginTicketBySms
    {
        public static void Send(string mobile, string mmtKey, string geetestJson)
        {
            HttpUtils httpUtils = new HttpUtils();
            var postForm = new Dictionary<string, string>();
            postForm.Add("mobile", mobile);
            postForm.Add("mmt_key", mmtKey);
            postForm.Add("action_type", "login");
            postForm.Add("t", BaseUtils.GetTimeStamp());
            if (geetestJson != "")
            {
                try
                {
                    JObject geetestJsonJObject = JObject.Parse(geetestJson);
                    switch (geetestJsonJObject["version"]!.ToString())
                    {
                        case "gt3":
                            postForm.Add("geetest_challenge", geetestJsonJObject["geetest_challenge"].ToString());
                            postForm.Add("geetest_validate", geetestJsonJObject["geetest_validate"].ToString());
                            postForm.Add("geetest_seccode", geetestJsonJObject["geetest_seccode"].ToString());
                            break;
                        case "gt4":
                            postForm.Add("geetest_v4_data", geetestJsonJObject["geetest_v4_data"].ToString());
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            string result = httpUtils.PostFormAsync(ApiUrls.login_pwd, postForm).Result.ToString();
        }

        public static string Verify(string mobile, string code)
        {
            HttpUtils httpUtils = new HttpUtils();
            var postForm = new Dictionary<string, string>();
            postForm.Add("mobile", mobile);
            postForm.Add("mobile_captcha", code);
            postForm.Add("source", "user.mihoyo.com");
            postForm.Add("t", BaseUtils.GetTimeStamp());
            string result = httpUtils.PostFormAsync(ApiUrls.login_pwd, postForm).Result.ToString();
            JObject resultObject = JObject.Parse(result);
            try
            {
                return resultObject["data"]["account_info"]["weblogin_token"].ToString() ?? throw new InvalidOperationException();
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }

    public static void GetMutiTicketByLoginTicket(string loginTicket, AccountConfig accountConfig)
    {
        HttpUtils httpUtils = new HttpUtils();
        JObject loginTmp;
        loginTmp = JObject.Parse(httpUtils.GetAsync(string.Format(ApiUrls.Cookie_url, loginTicket)).Result.ToString());
        if (loginTmp["data"]["msg"].ToString().Equals("成功"))
        {
            string uid = loginTmp["data"]["cookie_info"]["account_id"].ToString();
            string cookieToken = loginTmp["data"]["cookie_info"]["cookie_token"].ToString();
            loginTmp = JObject.Parse(httpUtils.GetAsync(string.Format(ApiUrls.Cookie_url2, loginTicket, uid)).Result.ToString());
            string stokenV1 = loginTmp["data"]["list"][0]["token"].ToString();
            string ltoken = loginTmp["data"]["list"][1]["token"].ToString();
            var requestHeader = new Dictionary<string, string>
            {
                { "Cookie", string.Format("stoken={0};stuid={1};", stokenV1, uid) },
                { "DS", BaseUtils.GenerateMysDS2("prod") },
                {"x-rpc-app_id", "bll8iq97cem8"},
                {"x-rpc-game_biz", "bbs_cn"}
            };
            loginTmp = JObject.Parse(httpUtils.PostFormAsync(ApiUrls.Cookie_url4, new Dictionary<string, string>(), requestHeader).Result.ToString());
            string stokenV2 = loginTmp["data"]["token"]["token"].ToString();
            string mid = loginTmp["data"]["user_info"]["mid"].ToString();
            JObject account = new JObject
            {
                { "login_ticket", loginTicket },
                { "stuid", uid },
                { "stoken", new JArray(stokenV1, stokenV2)},
                { "ltoken", ltoken },
                { "ltuid", uid },
                { "cookie_token", cookieToken },
                { "mid", mid },
                { "account_id", uid }
            };
            accountConfig.WriteAccount(account);
        }
    }
}