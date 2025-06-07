using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GoogleMobileAdsScript : MonoBehaviour
{
    // Start is called before the first frame update
    public static GoogleMobileAdsScript This;

    #if UNITY_ANDROID
        private const string RewardAdUnitId = "ca-app-pub-8572986188460654/1641841964";
        private const string InterstitialAdUnitId = "ca-app-pub-8572986188460654/8551715306";
    #elif UNITY_IOS
        private const string RewardAdUnitId = "ca-app-pub-8572986188460654/6347982548";
        private const string InterstitialAdUnitId = "ca-app-pub-8572986188460654/7238633639";
    #else
        private const string RewardAdUnitId = "ca-app-pub-8572986188460654/6347982548";
        private const string InterstitialAdUnitId = "ca-app-pub-8572986188460654/7238633639";
    #endif

    private RewardedAd _rewardedAd;
    private InterstitialAd _interstitialAd;

    public void Start()
    {
        This = this;
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        // // Set your device as a test device. for test purpose only
        // string testDeviceId = "1634f9bc7e1af04b3b5b6159c6181acf";
        // RequestConfiguration requestConfiguration = new RequestConfiguration
        //     .Builder()
        //     .SetTestDeviceIds(new List<string>() { testDeviceId })
        //     .build();
        // MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        Debug.Log("Begin init google ad sdk");
        MobileAds.Initialize(_ =>
        {
            Debug.Log("google ad sdk Initialization complete");
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }


    /// <summary>
    /// Loads the interstitial ad.
    /// 加载插页式广告
    /// </summary>
    private void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // // create our request used to load the ad.
        // var adRequest = new AdRequest.Builder()
        //     .AddKeyword("unity-admob-sample")
        //     .Build();

        // *** new new new
        var adRequest = new AdRequest
        {
            Keywords = { "unity-admob-sample" }
        };

        // send the request to load the ad.
        InterstitialAd.Load(InterstitialAdUnitId, adRequest,
            (ad, error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError($"interstitial ad failed to load an ad with error : {error}");
                    return;
                }

                Debug.Log($"Interstitial ad loaded with response : {ad.GetResponseInfo()}");
                _interstitialAd = ad;
            });
    }

    //展示插页式广告
    public void ShowInterstitialAd(UnityAction action = null)
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
            RegisterEventHandlers(_interstitialAd);
            RegisterReloadHandler(_interstitialAd);
            action?.Invoke();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    //监听插页式广告事件
    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        // 在广告预计赚到钱时筹集
        ad.OnAdPaid += adValue => Debug.Log($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
        // Raised when an impression is recorded for an ad.
        // 当广告的印象被记录时触发。
        ad.OnAdImpressionRecorded += () => Debug.Log("Interstitial ad recorded an impression.");
        // Raised when a click is recorded for an ad.
        // 当为广告记录点击时引发。
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        // 当广告打开全屏内容时引发。
        ad.OnAdFullScreenContentOpened += () => Debug.Log("Interstitial ad full screen content opened.");
        // Raised when the ad closed full screen content.
        // 广告关闭时引发全屏内容。
        ad.OnAdFullScreenContentClosed += () => Debug.Log("Interstitial ad full screen content closed.");
        // Raised when the ad failed to open full screen content.
        // 当广告无法打开全屏内容时引发。
        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError($"Interstitial ad failed to open full screen content with error : {error}");
        };
    }

    //预加载下一个插页式广告
    private void RegisterReloadHandler(InterstitialAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError($"Interstitial ad failed to open full screen content with error : {error}");

            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitialAd();
        };
    }

    /// <summary>
    /// Loads the rewarded ad.
    /// 加载激励广告
    /// </summary>
    private void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // // create our request used to load the ad.
        // var adRequest = new AdRequest.Builder().Build();

        var adRequest = new AdRequest();  // *** new new new

        // send the request to load the ad.
        RewardedAd.Load(RewardAdUnitId, adRequest,
            (ad, error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError($"Rewarded ad failed to load an ad with error : {error}");
                    return;
                }

                Debug.Log($"Rewarded ad loaded with response : {ad.GetResponseInfo()}");

                _rewardedAd = ad;
            });
    }

    public bool CheckRewardedAd()
    {
        return _rewardedAd != null && _rewardedAd.CanShowAd();
    }

    /// <summary>
    /// 通过奖励回调展示激励广告
    /// </summary>
    /// <returns></returns>
    public void ShowRewardedAd(string type, UnityAction action = null)
    {
        if (CheckRewardedAd())
        {
            RegisterEventHandlers(_rewardedAd);
            RegisterReloadHandler(_rewardedAd);
            _rewardedAd.Show(reward =>
            {
                // TODO: Reward the user.
                Debug.Log($"Rewarded ad rewarded the user. Type: {reward.Type}, amount: {reward.Amount}.");
                action?.Invoke();
            });
        }
        else
        {
            Debug.Log("rewardedAd not load");
            //InitScript.Instance.CheckRewardedAds(false);
        }
    }

    /// <summary>
    /// 监听激励广告事件
    /// </summary>
    /// <param name="ad"></param>
    /// <returns></returns>
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += adValue => Debug.Log($"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}.");
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () => Debug.Log("Rewarded ad recorded an impression.");
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () => Debug.Log("Rewarded ad full screen content opened.");
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () => Debug.Log("Rewarded ad full screen content closed.");
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += error =>
            Debug.LogError($"Rewarded ad failed to open full screen content with error : {error}");
    }

    /// <summary>
    /// 预加载下一个激励广告
    /// </summary>
    /// <param name="ad"></param>
    /// <returns></returns>
    private void RegisterReloadHandler(RewardedAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError($"Rewarded ad failed to open full screen content with error : {error}");

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }
}
