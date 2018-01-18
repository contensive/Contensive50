
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//

public class Collection<T> {
    private List<T> objects = new List<T>();
    private List<string> keys = new List<string>();

    public void Add(T newObject, string key = null, object before = null, object after = null) {
        if (after != null) {
            if (after as string != null)
                Insert(newObject, keys.IndexOf(after as string) + 1, key);
            else
                Insert(newObject, (int)after, key);
        } else if (before != null) {
            if (before as string != null)
                Insert(newObject, keys.IndexOf(before as string), key);
            else
                Insert(newObject, (int)before - 1, key);
        } else
            Insert(newObject, objects.Count, key);
    }

    private void Insert(T newObject, int index, string key) {
        objects.Insert(index, newObject);
        keys.Insert(index, key);
    }

    public void Clear() {
        objects.Clear();
        keys.Clear();
    }

    public bool Contains(string key) {
        return keys.Contains(key);
    }

    public int Count {
        get {
            return objects.Count;
        }
    }

    public void Remove(string key) {
        RemoveAt(keys.IndexOf(key));
    }

    public void Remove(int positionOneBased) {
        RemoveAt(positionOneBased - 1);
    }

    private void RemoveAt(int index) {
        objects.RemoveAt(index);
        keys.RemoveAt(index);
    }

    public T this[int positionOneBased] {
        get {
            return objects[positionOneBased - 1];
        }
    }

    public T this[string key] {
        get {
            return objects[keys.IndexOf(key)];
        }
    }

    public IEnumerator<T> GetEnumerator() {
        return objects.GetEnumerator();
    }
}