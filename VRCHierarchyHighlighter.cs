﻿/*  ヒエラルキを階層別に色分けするのと、VRC向けの重要コンポーネントがある場合にアイコンで可視化するやつ
 * 
 *  see also: http://baba-s.hatenablog.com/entry/2015/05/09/122713
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

using UnityEditor;
using UnityEngine;


public static class HierarchyIndentHelper
{
    private const string kResourceDirPath = "Assets/Editor/Resources/";
    private const string kResourceSuffix = ".png";
    private static readonly string[] kIconNames = {
        "DynamicBone",
        "SkinnedMeshRenderer",
        "VRC_AvatarDescriptor",
        //"Cloth",
        //"Rigidbody",
        //"Joint",
        //"Collider"
    };
    private static Dictionary<string, Texture2D> icon_resources_ = new Dictionary<string, Texture2D>();

    private static Texture2D LoadIconTex2DFromPNG(string path)
    {
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));
        byte[] binary = reader.ReadBytes((int)reader.BaseStream.Length);
        Texture2D tex = new Texture2D(16, 16);
        tex.LoadImage(binary);
        return tex;
    }

    private static void SetupIcons()
    {
        foreach (string name in kIconNames)
        {
            string filepath = kResourceDirPath + name + kResourceSuffix;
            Texture2D icon = LoadIconTex2DFromPNG(filepath);
            icon_resources_.Add(name, icon);
        }
    }

    [InitializeOnLoadMethod]
    private static void Startup()
    {
        SetupIcons();
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    private static void OnHierarchyWindowItemOnGUI
    (int instance_id, Rect target_rect)
    {
        var precision = 100.0f;
        var s = 0.7f;
        var v = 0.7f;
        var h = ((float)(target_rect.x) / precision) % 1.0f;
        var alpha = 0.2f;

        var background_color = UnityEngine.Color.HSVToRGB(h, s, v);
        var color = GUI.color;

        GUI.color = new Color(
            background_color.r,
            background_color.g,
            background_color.b,
            alpha
        );

        var rect = target_rect;
        rect.x = target_rect.x;
        rect.xMax = target_rect.xMax;
        GUI.Box(rect, "");

        var obj = EditorUtility.InstanceIDToObject(instance_id) as GameObject;
        if (obj != null)
        {
            var components = obj.GetComponents(typeof(Component));
            if (components != null)
            {
                foreach (Component component in components)
                {
                    foreach (var icon_info in icon_resources_)
                    {
                        if (component.ToString().Contains(icon_info.Key))
                        {
                            Color boxcolor = Color.white;
                            boxcolor.a = 1f;
                            GUI.color = boxcolor;
                            rect.x = 0; //target_rect.xMax - target_rect.height - 20; // 右寄せにする場合
                            rect.xMax = target_rect.xMax;
                            rect.width = 20;
                            rect.height = 20;

                            GUI.Label(rect, icon_info.Value);
                        }
                    }

                }
            }
        }

        GUI.color = color;
    }
}