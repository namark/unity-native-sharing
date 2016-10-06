using UnityEngine;

/*
 * https://github.com/ChrisMaire/unity-native-sharing
 */


public class NativeShare : MonoBehaviour
{
    public string _screenshotName = "screenshot.png";
    public string _subject = "";
    public string _text = "";
    public string _url = "";

    public void ShareScreenshotWithText()
    {
        string screenShotPath = Application.persistentDataPath + "/" + _screenshotName;
        Application.CaptureScreenshot(_screenshotName);

        Share(_text, _subject, _url, screenShotPath);
    }

    public void ShareText()
    {
        Share(_text, _subject, _url);
    }

    public static void Share(string shareText, string subject = "", string url = "", string imagePath = "")
    {

#if UNITY_EDITOR
        Debug.Log("Sharing");
        Debug.Log(" Text: " + shareText);
        Debug.Log(" Subject: " + subject);
        Debug.Log(" URL: " + url);
        Debug.Log(" Image: " + imagePath);
        return;
#endif

#if UNITY_ANDROID
        var intentClass = new AndroidJavaClass("android.content.Intent");
        var intentObject = new AndroidJavaObject("android.content.Intent");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        if ( !System.String.IsNullOrEmpty(imagePath) )
        {
            var uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + imagePath);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
            intentObject.Call<AndroidJavaObject>("setType", "image/png");
        }
        else
        {
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
        }

        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);

        var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, subject);
        currentActivity.Call("startActivity", jChooser);
#elif UNITY_IOS
        CallSocialShareAdvanced(shareText, subject, url, imagePath);
#else
        throw new UnityException("No sharing implemented for this platform.");
#endif

    }

#if UNITY_IOS
    public struct ConfigStruct
    {
        public string title;
        public string message;
    }

    [DllImport("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);

    public struct SocialSharingStruct
    {
        public string text;
        public string url;
        public string image;
        public string subject;
    }

    [DllImport("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);

    public static void CallSocialShare(string title, string message)
    {
        ConfigStruct conf = new ConfigStruct();
        conf.title = title;
        conf.message = message;
        showAlertMessage(ref conf);
    }

    public static void CallSocialShareAdvanced(string defaultTxt, string subject, string url, string img)
    {
        SocialSharingStruct conf = new SocialSharingStruct();
        conf.text = defaultTxt;
        conf.url = url;
        conf.image = img;
        conf.subject = subject;

        showSocialSharing(ref conf);
    }
#endif

}
