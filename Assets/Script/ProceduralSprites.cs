using UnityEngine;

// Простые спрайты, созданные кодом (не нужны отдельные картинки).
public static class ProceduralSprites
{
    private static Sprite _ring;

    // Кольцо диаметром 1 юнит — используется как щит врага.
    public static Sprite Ring
    {
        get
        {
            if (_ring == null)
            {
                const int size = 128;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                float center = (size - 1) / 2f;
                float outer = size / 2f - 1f;
                float inner = outer - 7f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                        float a = 0f;
                        if (d <= outer && d >= inner)
                        {
                            // мягкие края кольца
                            a = Mathf.Clamp01(Mathf.Min(outer - d, d - inner) / 2f);
                        }
                        else if (d < inner)
                        {
                            a = 0.12f; // лёгкая заливка внутри
                        }
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                }
                tex.Apply();
                _ring = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            }
            return _ring;
        }
    }
}
