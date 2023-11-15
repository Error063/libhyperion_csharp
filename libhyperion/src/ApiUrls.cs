namespace libhyperion;

public class ApiUrls
{
    public static string api_base { get; } = "https://bbs-api.miyoushe.com/";
    public static string api_static_base = "https://bbs-api-static.miyoushe.com/";
    public static string api_takumi_base = "https://api-takumi.mihoyo.com/";
    public static string api_account_base = "https://webapi.account.mihoyo.com/";
    public static string api_passport_base = "https://passport-api.mihoyo.com/";

    // Static
    public static string static_base = "https://bbs-static.miyoushe.com/";
    public static string defaultAvatar = static_base + "avatar/avatarDefault.png";

    // Gets
    public static string Cookie_url = string.Format("{0}Api/cookie_accountinfo_by_loginticket?login_ticket={{0}}", api_account_base);
    public static string Cookie_url2 = string.Format("{0}auth/api/getMultiTokenByLoginTicket?login_ticket={{0}}&token_types=3&uid={{1}}", api_takumi_base);
    public static string Cookie_url3 = string.Format("{0}auth/api/getCookieAccountInfoBySToken?stoken={{0}}&uid={{1}}", api_takumi_base);
    public static string mmt = string.Format("{0}Api/create_mmt?scene_type=1&now={{0}}", api_account_base);
    public static string getPostReplies = string.Format("{0}post/api/getPostReplies?gids={{0}}&is_hot={{1}}&post_id={{2}}&size={{3}}&last_id={{4}}&order_type={{5}}&only_master={{6}}", api_base);
    public static string getSubReplies = string.Format("{0}post/api/getSubReplies?post_id={{0}}&floor_id={{1}}&last_id={{2}}&size={{3}}", api_base);
    public static string getRootReplyInfo = string.Format("{0}post/api/getRootReplyInfo?post_id={{0}}&reply_id={{1}}", api_base);
    public static string webHome = string.Format("{0}apihub/wapi/webHome?gids={{0}}&page={{1}}&page_size={{2}}", api_base);
    public static string getPostFull = string.Format("{0}post/api/getPostFull?post_id={{0}}", api_base);
    public static string feedPosts = string.Format("{0}post/api/feeds/posts?gids={{0}}&last_id=&fresh_action=1&is_first_initialize=true&filter=", api_base);
    public static string emoticon_set = string.Format("{0}misc/api/emoticon_set?gid={{0}}", api_base);
    public static string getNewsList = string.Format("{0}post/wapi/getNewsList?gids={{0}}&type={{1}}&page_size={{2}}&last_id={{3}}", api_base);
    public static string searchPosts = string.Format("{0}post/wapi/searchPosts?gids={{0}}&keyword={{1}}&last_id={{2}}&size={{3}}", api_base);
    public static string getUserFullInfo = string.Format("{0}user/api/getUserFullInfo?uid={{0}}", api_base);
    public static string userPost = string.Format("{0}post/wapi/userPost?offset={{0}}&size={{1}}&uid={{2}}", api_base);
    public static string userReply = string.Format("{0}post/wapi/userReply?offset={{0}}&size={{1}}&uid={{2}}", api_base);
    public static string history = string.Format("{0}painter/api/history/list?offset={{0}}", api_base);
    public static string getAllGamesForums = api_base + "apihub/wapi/getAllGamesForums";
    public static string getGameList = api_base + "apihub/api/getGameList";
    public static string getForumPostList = string.Format("{0}post/wapi/getForumPostList?forum_id={{0}}&gids={{1}}&is_good={{2}}&is_hot={{3}}&page_size={{4}}&sort_type={{5}}&last_id={{6}}", api_base);
    public static string dailyNote_genshin_widget = "https://api-takumi-record.mihoyo.com/game_record/app/genshin/aapi/widget/v2";
    public static string dailyNote_hkrpg_widget = "https://api-takumi-record.mihoyo.com/game_record/app/hkrpg/aapi/widget";
    public static string createVerification = api_base + "misc/api/createVerification?is_high=true";

    // Posts
    public static string login_pwd = api_account_base + "Api/login_by_password";
    public static string login_sms = api_account_base + "Api/login_by_mobilecaptcha";
    public static string follow = api_base + "timeline/api/follow";
    public static string unfollow = api_base + "timeline/api/unfollow";
    public static string upvotePost = api_base + "apihub/sapi/upvotePost";
    public static string collectPost = api_base + "post/api/collectPost";
    public static string releaseReply = api_base + "post/api/releaseReply";
    public static string deleteReply = api_base + "post/api/deleteReply";
    public static string upvoteReply = api_base + "apihub/sapi/upvoteReply";
    public static string Cookie_url4 = api_passport_base + "account/ma-cn-session/app/getTokenBySToken";
    public static string getFp = "https://public-data-api.mihoyo.com/device-fp/api/getFp";
    public static string verifyVerification = api_base + "misc/api/verifyVerification";
    public static string send_sms = api_account_base + "Api/create_mobile_captcha";
}