using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace libhyperion;

public class BBS
{
    private string CookieString = "";
    private string CurrentUid = "";
    private AccountConfig _currentAccountConfig;
    private HttpUtils _httpUtils = new HttpUtils();
    public BBS(AccountConfig accountConfig, string uid="0")
    {
        JArray existAccount = accountConfig.GetExistAccount();
        if (uid.Equals("0") && existAccount.Count > 0)
        {
            uid = existAccount[0].ToString()!;
            CookieString = accountConfig.GetAccountCookieString(uid);
            CurrentUid = uid;
        }
        if (accountConfig.HasAccount(uid))
        {
            CookieString = accountConfig.GetAccountCookieString(uid);
            CurrentUid = uid;
        }
        _currentAccountConfig = accountConfig;
    }

    private JArray PerfectArticleSet(JArray oldSet)
    {
        JArray newSet = new JArray();

        foreach (var item in oldSet)
        {
            try
            {
                JObject setTmp = new JObject();
                setTmp["post_id"] = item["post"]!["post_id"]!;
                setTmp["title"] = item["post"]!["subject"]!;
                setTmp["describe"] = item["post"]!["content"]!.ToString()!.Substring(0, 50) +
                                     (item["post"]!["content"]!.ToString()!.Length > 50 ? "..." : "");
                try
                {
                    setTmp["cover"] = item["post"]!["cover"]!.ToString()!.Equals("")
                        ? item["post"]!["image"]![0]!.ToString()
                        : item["post"]!["cover"]!.ToString();
                }
                catch (Exception e)
                {
                    setTmp["cover"] = "";
                }

                setTmp["authorAvatar"] = item["user"]!["avatar_url"]!;
                setTmp["uid"] = item["user"]!["uid"]!;
                setTmp["authorName"] = item["user"]!["nickname"]!;
                if (item["user"]!["certification"]!["label"]!.ToString()!.Length > 0)
                {
                    setTmp["authorDescribe"] = item["user"]!["certification"]!["label"]!;
                }
                else
                {
                    if (item["user"]!["introduce"]!.ToString()!.Length > 15)
                    {
                        setTmp["authorDescribe"] = item["user"]!["introduce"]!.ToString()!.Substring(0, 15) + "...";
                    }
                    else
                    {
                        setTmp["authorDescribe"] = item["user"]!["introduce"]!.ToString()!;
                    }
                }
                setTmp["type"] = item["post"]!["view_type"]!;
                setTmp["upvote"] = item["self_operation"]!["attitude"]!;
                setTmp["collect"] = item["self_operation"]!["is_collected"]!;
                newSet.Add(setTmp);
            }
            catch (Exception e)
            {
                continue;
            }
        }
        return newSet;
    }

    private string ReplaceAll(string contents, JObject emotionDict)
    {
        contents = Regex.Replace(contents, @"_\((.*?)\)",
            m => $"<img class=\"emoticon-image emotionIcon\" src=\"{emotionDict[m.Groups[1].Value]}\">");

        return contents;
    }


    public JObject GetEmotions(int gid=2)
    {
        JObject emotionObject = new JObject();
        string raw = _httpUtils.GetAsync(string.Format(ApiUrls.emoticon_set, gid)).Result;
        JObject rawObj = JObject.Parse(raw);
        foreach (var item in rawObj["data"]!["list"]!)
        {
            foreach (var emoItem in item["list"]!)
            {
                emotionObject.Add(emoItem["name"]!.ToString()!, emoItem["icon"]!.ToString()!);
            }
        }
        return emotionObject;
    }

    public JArray GetGame()
    {
        JArray gameSet = new JArray();
        foreach (var item in JObject.Parse(_httpUtils.GetAsync(ApiUrls.getGameList).Result)["data"]!["list"]!)
        {
            gameSet.Add(new JObject
            {
                { "id", item["id"] },
                { "op_name", item["op_name"] },
                { "en_name", item["en_name"] },
                { "name", item["name"] },
            });
        }
        return gameSet;
    }

    public JObject GetGame(string game)
    {
        JArray gameSet = GetGame();
        foreach (var item in gameSet)
        {
            if (item["id"]!.ToString()!.Equals(game) || item["name"]!.ToString()!.Equals(game) || item["en_name"]!.ToString()!.Equals(game) || item["op_name"]!.ToString()!.Equals(game))
            {
                return (JObject)item;
            }
        }
        return null!;
    }

    public JObject GetArticle(string postId)
    {
        var header = BaseUtils.AppendFp(BaseUtils.GenerateHeader(cookie:CookieString));
        JObject articleObj = JObject.Parse(_httpUtils.GetAsync(string.Format(ApiUrls.getPostFull, postId), header).Result);
        if (articleObj["retcode"]!.ToString()!.Equals("0"))
        {
            articleObj["data"]!["post"]!["post"]!["content"] = ReplaceAll(articleObj["data"]!["post"]!["post"]!["content"]!.ToString()!, GetEmotions());
        }

        return articleObj;
    }

    public JArray GetPosts(string gid, string pageType, uint page = 1, int maxSize = 20)
    {
        string apiUrl;
        if (pageType.Equals("recommend"))
        {
            apiUrl = string.Format(ApiUrls.webHome, gid, page.ToString(), maxSize.ToString());
        }else if (pageType.Equals("feed"))
        {
            apiUrl = string.Format(ApiUrls.feedPosts, gid);
        }
        else
        {
            string type = "1";
            switch (pageType)
            {
                case "announce":
                    type = "1";
                    break;
                case "activity":
                    type = "2";
                    break;
                case "information":
                    type = "3";
                    break;
                default:
                    type = "1";
                    break;
            }
            apiUrl = string.Format(ApiUrls.getNewsList, gid, type, pageType, ((page - 1) * maxSize).ToString());
        }

        var header = BaseUtils.AppendFp(BaseUtils.GenerateHeader(client: "2", cookie: CookieString));
        JArray rawArticlesSet = (JArray)
            JObject.Parse(_httpUtils.GetAsync(apiUrl, header).Result.ToString()!)["data"]![
                pageType.Equals("recommend") ? "recommended_posts" : "list"]!;
        return PerfectArticleSet(rawArticlesSet);
    }
}