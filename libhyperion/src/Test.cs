using Newtonsoft.Json.Linq;

namespace libhyperion;

class Program
{
    public static void Main()
    {
        AccountConfig accountConfig = new AccountConfig();
        BBS bbs = new BBS(accountConfig, "311526738");
        for (int i = 0; i < 10; i++)
        {
            foreach (var item in bbs.GetPosts("2", "feed"))
            {
                Console.WriteLine(string.Format("{0}: {1}", item["post_id"], item["title"]));
            }
        }

    }
}