using UnityEngine;

/**
 * TODO: Mostly cut and pasted from https://github.com/RyanNielson/PixelCamera2D
 *   I tried to trim it down to the essentials as well as make it work properly
 *   inside the Editor, but I'm currently unable to do that. You have to turn
 *   the camera game object on and off to refresh the camera in the Editor
 *   which is a pain in the ass. :p
 */
[ExecuteInEditMode]
public class PixelCamera : MonoBehaviour {

    public Camera pixelCamera;
    public Camera pixelCameraRenderer;
    public int baseWidth = 640;
    public int baseHeight = 360;
    public MeshRenderer quad;

    static RenderTexture _renderTexture;

    static void CreateNewRenderTexture(int width, int height) {
        if (!_renderTexture) {
            _renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point
            };
        }
    }

    void Update() {
        CreateNewRenderTexture(baseWidth, baseHeight);
        SetRenderTexture(_renderTexture);

        int nearestWidth = Screen.width / baseWidth * baseWidth;
        int nearestHeight = Screen.height / baseHeight * baseHeight;

        int xScaleFactor = nearestWidth / baseWidth;
        int yScaleFactor = nearestHeight / baseHeight;
        int scaleFactor = yScaleFactor < xScaleFactor ? yScaleFactor : xScaleFactor;
        float heightRatio = (baseHeight * (float)scaleFactor) / Screen.height;

        quad.transform.localScale = new Vector3(baseWidth / (float)baseHeight * heightRatio, 1f * heightRatio, 1f);
        Rect rect = pixelCamera.rect;
        rect = new Rect(GetCameraRectOffset(Screen.width), GetCameraRectOffset(Screen.height), rect.width, rect.height);
        pixelCameraRenderer.rect = rect;
    }

    void SetRenderTexture(RenderTexture renderTexture) {
        pixelCamera.targetTexture = renderTexture;
        quad.sharedMaterial.mainTexture = renderTexture;
    }

    float GetCameraRectOffset(int size) {
        return size % 2 == 0 ? 0 : 1f / size;
    }
}
