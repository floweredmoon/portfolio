using UnityEngine;

public class Kernel : MonoBehaviour, ILogger
{
    #region Singleton

    static Entry m_Entry;

    public static Entry entry
    {
        get
        {
            return m_Entry;
        }
    }

    public static SceneManager sceneManager
    {
        get;
        private set;
    }

    public static CanvasManager canvasManager
    {
        get;
        private set;
    }

    public static NetworkManager networkManager
    {
        get;
        private set;
    }

    public static TextureManager textureManager
    {
        get;
        private set;
    }

    public static UIManager uiManager
    {
        get;
        private set;
    }

    public static DataLoader dataLoader
    {
        get;
        private set;
    }

    public static ColorManager colorManager
    {
        get;
        private set;
    }

    public static SoundManager soundManager
    {
        get;
        private set;
    }

    public static PacketRequestIterator packetRequestIterator
    {
        get;
        private set;
    }

    public static AchieveManager achieveManager
    {
        get;
        private set;
    }

    public static NetworkEventHandler networkEventHandler
    {
        get;
        private set;
    }

    public static FacebookManager facebookManager
    {
        get;
        private set;
    }

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
    public static GPGSManager gpgsManager
    {
        get;
        private set;
    }
#endif

    public static IAPManager iapManager
    {
        get;
        private set;
    }

    public static NotificationManager notificationManager
    {
        get;
        private set;
    }

    #endregion
    
    void Awake()
    {
        Debug.Log(Application.temporaryCachePath);
        DontDestroyOnLoad(gameObject);
        Application.runInBackground = true;
        Screen.SetResolution(1280, 720, Screen.fullScreen);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        m_Entry = new Entry(this);
    }

    // Use this for initialization
    void Start()
    {
        CreateSingletonInstance();

        if (sceneManager != null)
        {
            sceneManager.LoadScene(Scene.TitleScene);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Kernel.entry != null)
        {
            Kernel.entry.account.Update();
            Kernel.entry.post.Update();
            Kernel.entry.treasure.Update();
            Kernel.entry.franchise.Update();
        }
    }

    void OnApplicationQuit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    void CreateSingletonInstance()
    {
        sceneManager = SceneManager.CreateInstance();
        canvasManager = CanvasManager.CreateInstance();
        dataLoader = DataLoader.CreateInstance();
        soundManager = SoundManager.CreateInstance();
        networkManager = NetworkManager.CreateInstance();
        textureManager = TextureManager.CreateInstance();
        uiManager = UIManager.CreateInstance();
        colorManager = ColorManager.CreateInstance();
        packetRequestIterator = PacketRequestIterator.CreateInstance();
        achieveManager = AchieveManager.CreateInstance();
        networkEventHandler = NetworkEventHandler.CreateInstance();
        facebookManager = FacebookManager.CreateInstance();
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
        gpgsManager = GPGSManager.CreateInstance();
#endif
        iapManager = IAPManager.CreateInstance();
        notificationManager = NotificationManager.CreateInstance();
    }

    public static void Reload()
    {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        for (int i = 0; i < gameObjects.Length; i++)
        {
            GameObject gameObject = gameObjects[i];
            if (gameObject != null)
            {
                if (IAPManager.Instance.gameObject == gameObject)
                {
                    continue;
                }

                DestroyImmediate(gameObject);
            }
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Kernel");
    }

    #region ILogger

    public void Log(string format, params object[] args)
    {
        Debug.Log(string.Format(format, args));
    }

    public void LogWarning(string format, params object[] args)
    {
        Debug.LogWarning(string.Format(format, args));
    }

    public void LogError(string format, params object[] args)
    {
        Debug.LogError(string.Format(format, args));
    }

    #endregion
}
