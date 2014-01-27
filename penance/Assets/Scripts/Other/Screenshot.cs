/* Credit Attribution
 * Author: jashan
 * http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
 */

using UnityEngine;
using System;
using System.Collections;


public class Screenshot : MonoBehaviour {
    public int resWidth;
    public int resHeight;
 
    private bool takeHiResShot;
 
	void Awake() {
		resWidth = Screen.width;
		resHeight = Screen.height;
		takeHiResShot = false;
	}
	
    public static string ScreenShotName() {
        return string.Format("{0}/Resources/screenshots/{1}.png", Application.dataPath, Guid.NewGuid().ToString());
    }
 
    public void TakeHiResShot() {
        takeHiResShot = true;
    }
 
	void LateUpdate() {
		takeHiResShot |= Input.GetKeyDown("k");
        if (takeHiResShot) {
			StartCoroutine(TakeScreenshot());
		}
	}
	
    IEnumerator TakeScreenshot() {
		yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName();
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
		takeHiResShot = false;
    }
}