using System;
using System.Collections;
using System.Collections.Generic;

public sealed class SlangFilter
{

    private class Node
    {

        private Dictionary<char, Node> children
        {
            get;
            set;
        }

        public bool isLast
        {
            get;
            set;
        }

        public Node()
        {
            children = new Dictionary<char, Node>();
        }

        public Node FindChild(char value)
        {
            return children.ContainsKey(value) ? children[value] : null;
        }

        public Node AddChild(char value)
        {
            var childNode = FindChild(value);
            if (childNode == null)
                children.Add(value, childNode = new Node());

            return childNode;
        }
    }

    private Node rootNode
    {
        get;
        set;
    }

    public bool isInitialized
    {
        get;
        private set;
    }

    public SlangFilter()
    {
        rootNode = new Node();
    }

    public IEnumerator InitializeCorotuine(IEnumerable<string> slangs, Action<bool> callback)
    {
        if (slangs != null)
        {
            foreach (var slang in slangs)
            {
                AddSlang(slang);
                yield return null;
            }

            isInitialized = true;
        }

        callback.InvokeNullOk(isInitialized);
    }

    private void AddSlang(string slang)
    {
        var trimmedSlang = slang.Replace(" ", string.Empty);
        if (string.IsNullOrEmpty(trimmedSlang))
            return;

        var loweredSlang = trimmedSlang.ToLowerInvariant();
        var rootNode = this.rootNode;
        for (int i = 0; i < loweredSlang.Length; i++)
            if (rootNode != null)
                rootNode = rootNode.AddChild(loweredSlang[i]);

        if (rootNode != null)
            rootNode.isLast = true;
    }

    public bool ContainsSlang(string value)
    {
        return ContainsSlang(rootNode, value);
    }

    private bool ContainsSlang(Node rootNode, string value)
    {
        if (rootNode == null)
            return false;

        var trimmedSlang = value.Replace(" ", string.Empty);
        if (string.IsNullOrEmpty(trimmedSlang))
            return false;

        var loweredSlang = trimmedSlang.ToLowerInvariant();
        for (var i = 0; i < loweredSlang.Length; i++)
        {
            var currentNode = rootNode.FindChild(loweredSlang[i]);
            if (currentNode != null)
                if (currentNode.isLast)
                    return true;
                else
                    return ContainsSlang(currentNode, loweredSlang.Substring(i + 1));
            else
                return false;
        }

        return false;
    }
}
