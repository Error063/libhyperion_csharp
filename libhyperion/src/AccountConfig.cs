using Newtonsoft.Json.Linq;

namespace libhyperion;

public class AccountConfig
{
    private JObject accounts;
    private string filePath;
    public AccountConfig(string filePathInner=null)
    {
        if (filePathInner == null)
        {
            filePathInner = string.Format("{0}/.libhyperion/configs/accounts.json", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        filePath = filePathInner;
        string content = "";
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
            }
        }
        catch (Exception e){}
        bool flag = false;
        if (content != "")
        {
            accounts = JObject.Parse(content);
            try
            {
                string currentMd5 = BaseUtils.CalculateMD5(accounts["account"].ToString());
                flag = currentMd5 != accounts["account_hash"].ToString();
            }catch{}
        }

        if (!flag)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                accounts = new JObject();
                accounts["account"] = new JObject();
                accounts["account_hash"] = BaseUtils.CalculateMD5(accounts["account"].ToString());
                byte[] writeContents = System.Text.Encoding.UTF8.GetBytes(accounts.ToString());
                fileStream.Write(writeContents);
            }
        }
    }

    public JArray GetExistAccount()
    {
        JArray existAccount = new JArray();
        foreach (var item in (JObject)accounts["account"])
        {
            existAccount.Add(item.Key);
        }

        return existAccount;
    }

    public bool HasAccount(string uid)
    {
        JArray existAccount = GetExistAccount();
        foreach (var item in existAccount)
        {
            if (item.ToString().Equals(uid))
            {
                return true;
            }
        }

        return false;
    }

    public string GetAccountCookieString(string uid=null, int stokenLevel=1, int ltokenLevel=1)
    {
        string cookieString = "";
        JObject selectedAccount;
        JArray existAccount = GetExistAccount();
        if (existAccount.Count <= 0)
        {
            return "";
        }
        if (uid == null)
        {
            uid = existAccount[0].ToString();
        }
        selectedAccount = (JObject)accounts["account"][uid];
        try
        {
            foreach (var item in selectedAccount)
            {
                if (item.Key.ToLower() == "stoken" && item.Value.GetType() == typeof(JArray))
                {
                    cookieString += string.Format("{0}={1};", item.Key, item.Value[stokenLevel - 1]);
                }else if (item.Key.ToLower() == "ltoken" && item.Value.GetType() == typeof(JArray))
                {
                    cookieString += string.Format("{0}={1};", item.Key, item.Value[ltokenLevel - 1]);
                }
                else
                {
                    cookieString += string.Format("{0}={1};", item.Key, item.Value);
                }

            }
        }
        catch (Exception e){}

        return cookieString;
    }

    public void WriteAccount(JObject account)
    {
        string accountId = account["account_id"]!.ToString();
        accounts["account"]![accountId] = account;
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            accounts["account_hash"] = BaseUtils.CalculateMD5(accounts["account"].ToString());
            byte[] writeContents = System.Text.Encoding.UTF8.GetBytes(accounts.ToString());
            fileStream.Write(writeContents);
        }

    }

    public void DeleteAccount(string uid)
    {
        try
        {
            accounts["account"]![uid]!.Remove();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                accounts["account_hash"] = BaseUtils.CalculateMD5(accounts["account"].ToString());
                byte[] writeContents = System.Text.Encoding.UTF8.GetBytes(accounts.ToString());
                fileStream.Write(writeContents);
            }
        }
        catch
        {}
    }
}