using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PostEffectScript : MonoBehaviour
{
    public Material mat;
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //src is the fully render scene that you would normally send to the monitor
        // we are intercepting this so we can do more work before passing it on
        Graphics.Blit(src, dest, mat);
    }
}
