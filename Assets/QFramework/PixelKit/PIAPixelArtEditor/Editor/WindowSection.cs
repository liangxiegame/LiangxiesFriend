using UnityEngine;

namespace Piacenti.EditorTools { 

    public class WindowSection
    {

        private Rect _rect;
        private Texture2D _texture2D;

        public WindowSection(Rect rect, Color color)
        {
            _rect = rect;

            _texture2D = new Texture2D(1, 1);
            _texture2D.SetPixel(0, 0, color);
            _texture2D.Apply();
        }

        public WindowSection(Rect rect, Texture2D texture)
        {
            _rect = rect;
            _texture2D = texture;
            
        }

        public Rect GetRect()
        {
            return _rect;
        }

        public void SetRect(float width, float height)
        {
            _rect.width = width;
            _rect.height = height;
        }
        public void SetRect(Rect rect) {
            _rect = rect;
        }
        public void SetRect(float x, float y, float width, float height)
        {
            _rect.x = x;
            _rect.y = y;
            _rect.width = width;
            _rect.height = height;
        }
        public void SetTexture(Texture2D texture)
        {
            _texture2D = texture;
        }
        public Texture2D GetTexture()
        {
            return _texture2D;
        }
        public void RefreshTextureColor()
        {
            _texture2D.Apply();
        }
        public Color GetTextureColor(int x, int y)
        {
            return _texture2D.GetPixel(x, y);
        }

    }
}
