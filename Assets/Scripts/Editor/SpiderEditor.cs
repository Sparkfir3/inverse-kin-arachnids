using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpiderController), true)]
public class SpiderEditor : Editor {
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        SpiderController spider = target as SpiderController;

        /*if(GUILayout.Button("Enable Spider")) {
            if(!spider.Active)
                spider.Active = true;
        }*/
        if(GUILayout.Button("Disable Spider")) {
            if(spider.Active)
                spider.Active = false;
        }
    }

}