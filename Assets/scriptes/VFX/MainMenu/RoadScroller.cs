using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    [SerializeField] private Renderer roadRenderer;
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private string textureProperty = "_BaseMap"; // "_MainTex" en Built-in

    private Material mat;

    private void Start()
    {
        mat = roadRenderer.material;
    }

    private void Update()
    {
        Vector2 offset = mat.GetTextureOffset(textureProperty);
        offset.x -= scrollSpeed * Time.deltaTime;
        mat.SetTextureOffset(textureProperty, offset);
    }
}