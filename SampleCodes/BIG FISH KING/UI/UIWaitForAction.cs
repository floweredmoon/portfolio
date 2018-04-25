using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIWaitForAction : MonoBehaviour
{
    #region Static Fields
    private static Dictionary<GameObject, UIWaitForAction> mDict = new Dictionary<GameObject, UIWaitForAction>();
    #endregion

    #region Fields
    public UIPanel mPanel;
    public UISprite mSprite;
    public BoxCollider mBoxCollider;
    public UI2DSprite m2DSprite;
    #endregion

    #region Properties
    public GameObject parentGo
    {
        get;
        private set;
    }

    private int overrideDepth
    {
        set
        {
            if (mPanel != null)
            {
                mPanel.depth = value;
            }
        }
    }

    private int maximumSize
    {
        get;
        set;
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        if (m2DSprite != null)
        {
            // 1 : 1
            maximumSize = Mathf.Min(m2DSprite.width, m2DSprite.height);
        }
    }

    // Use this for initialization

    // Update is called once per frame
    /*
    private void OnDestroy()
    {
        if (parentGo != null && mDict.ContainsKey(parentGo))
        {
            mDict.Remove(parentGo);
        }
    }
    */
    #endregion

    public static GameObject Start(UIPanel panel)
    {
        GameObject gameObject = null;
        if (panel != null && panel.gameObject != null)
        {
            int depth = panel.depth + 1;
            int x = 0;
            int y = 0;
            if (panel.clipping != UIDrawCall.Clipping.None)
            {
                x = (int)panel.clipOffset.x;
                y = (int)panel.clipOffset.y;
            }
            int width = (int)panel.width;
            int height = (int)panel.height;

            gameObject = Start(panel.gameObject, depth, x, y, width, height);
        }

        return gameObject;
    }

    public static GameObject Start(UIWidget widget, int? overrideDepth = null)
    {
        GameObject gameObject = null;
        if (widget != null && widget.gameObject != null)
        {
            // NullReferenceException
            if (overrideDepth == null)
            {
                if (widget.panel != null)
                {
                    overrideDepth = widget.panel.depth + 1;
                }
                else
                {
                    overrideDepth = widget.depth;
                }
            }
            int width = widget.width;
            int height = widget.height;
            int x = (int)Mathf.Lerp(-(width * .5f), (width * .5f), widget.pivotOffset.x);
            int y = (int)Mathf.Lerp((height * .5f), -(height * .5f), widget.pivotOffset.y);

            gameObject = Start(widget.gameObject, overrideDepth ?? 0, (int)x, (int)y, width, height);
        }

        return gameObject;
    }

    public static GameObject Start(GameObject gameObject, int overrideDepth, int x, int y, int width, int height)
    {
        if (gameObject != null)
        {
            UIWaitForAction instance = null;
            if (!mDict.TryGetValue(gameObject, out instance))
            {
                instance = Instantiate();
                mDict.Add(gameObject, instance);
            }

            if (instance != null)
            {
                instance.parentGo = gameObject;
                instance.overrideDepth = overrideDepth;
                instance.SetSize(width, height);
                instance.transform.SetParent(gameObject.transform);
                instance.transform.localPosition = new Vector2(x, y);
                instance.transform.localScale = Vector3.one;
            }
        }

        return gameObject;
    }

    public static void Stop(Component component)
    {
        if (component != null && component.gameObject != null)
        {
            Stop(component.gameObject);
        }
    }

    public static void Stop(GameObject gameObject)
    {
        if (gameObject != null)
        {
            UIWaitForAction instance = null;
            if (mDict.TryGetValue(gameObject, out instance))
            {
                mDict.Remove(gameObject);
                Destroy(instance.gameObject);
            }
        }
    }

    private void SetSize(int width, int height)
    {
        if (mSprite != null)
        {
            mSprite.width = width;
            mSprite.height = height;
        }

        if (mBoxCollider != null)
        {
            mBoxCollider.size = new Vector3(width, height);
        }

        if (m2DSprite != null)
        {
            // 1 : 1
            int minimum = Mathf.Min(width, height, maximumSize);

            m2DSprite.width = minimum;
            m2DSprite.height = minimum;
        }
    }

    private static UIWaitForAction Instantiate()
    {
        UIWaitForAction component = null;
        GameObject gameObject = ResourcesManager.Instantiate("UI/UIWaitForAction");
        if (gameObject != null)
        {
            component = gameObject.GetComponent<UIWaitForAction>();
        }

        return component;
    }
}
