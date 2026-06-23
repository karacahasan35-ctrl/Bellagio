using UnityEngine;

public static class RestorationSpriteFactory
{
    public static Sprite GetSprite(string chainName, int level, bool isGenerator)
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        // Şeffaf arka plan
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        if (isGenerator)
        {
            DrawToolbox(pixels, size);
        }
        else if (chainName == "Tool")
        {
            if (level == 1) DrawBrush(pixels, size);        // Fırça
            else if (level == 2) DrawTrowel(pixels, size);  // Mala
            else if (level == 3) DrawHammer(pixels, size);  // Çekiç
            else DrawChisel(pixels, size);                  // Iskarpela (Lvl 4+)
        }
        else if (chainName == "Material")
        {
            if (level == 1) DrawMortarBag(pixels, size);    // Harç Kovası
            else if (level == 2) DrawTile(pixels, size);     // Karo
            else DrawMarble(pixels, size);                  // Yontulmuş Mermer (Lvl 3+)
        }
        else if (chainName == "Faucet")
        {
            DrawFaucet(pixels, size);
        }
        else if (chainName == "FlowerPot")
        {
            DrawFlowerPot(pixels, size);
        }
        else if (chainName == "Bench")
        {
            DrawBench(pixels, size);
        }
        else if (chainName == "Lantern")
        {
            DrawLantern(pixels, size);
        }
        else if (chainName == "AvatarCan")
        {
            DrawAvatarCan(pixels, size);
        }
        else if (chainName == "AvatarLeyla")
        {
            DrawAvatarLeyla(pixels, size);
        }
        else
        {
            // Fallback (Hata durumunda beyaz yuvarlak)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - size / 2f;
                    float dy = y - size / 2f;
                    if (dx * dx + dy * dy <= 40 * 40) pixels[y * size + x] = Color.white;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        sprite.name = isGenerator ? "Toolbox" : $"{chainName}_Lvl{level}";
        return sprite;
    }

    private static void FillRect(Color[] pixels, int size, int xStart, int yStart, int width, int height, Color color)
    {
        for (int y = yStart; y < yStart + height; y++)
        {
            if (y < 0 || y >= size) continue;
            for (int x = xStart; x < xStart + width; x++)
            {
                if (x < 0 || x >= size) continue;
                pixels[y * size + x] = color;
            }
        }
    }

    // 1. Restorasyon Alet Çantası (Generator)
    private static void DrawToolbox(Color[] pixels, int size)
    {
        Color brown = new Color(0.42f, 0.23f, 0.12f, 1f);
        Color darkBrown = new Color(0.3f, 0.15f, 0.08f, 1f);
        Color gold = new Color(0.9f, 0.75f, 0.15f, 1f);
        Color darkGray = new Color(0.25f, 0.25f, 0.28f, 1f);

        // Çanta Gövdesi
        FillRect(pixels, size, 24, 24, 80, 56, brown);
        FillRect(pixels, size, 24, 20, 80, 4, darkBrown); // Alt taban
        FillRect(pixels, size, 24, 64, 80, 16, darkBrown); // Kapak kısmı

        // Kilitler (Gold)
        FillRect(pixels, size, 40, 52, 10, 14, gold);
        FillRect(pixels, size, 78, 52, 10, 14, gold);

        // Kulp (Sap)
        FillRect(pixels, size, 44, 80, 40, 6, darkGray);
        FillRect(pixels, size, 44, 74, 6, 6, darkGray);
        FillRect(pixels, size, 78, 74, 6, 6, darkGray);
    }

    // 2. Restorasyon Fırçası (Tool Lvl 1)
    private static void DrawBrush(Color[] pixels, int size)
    {
        Color wood = new Color(0.72f, 0.48f, 0.28f, 1f);
        Color steel = new Color(0.6f, 0.6f, 0.65f, 1f);
        Color bristleColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Sap
        FillRect(pixels, size, 56, 64, 16, 44, wood);
        FillRect(pixels, size, 52, 100, 24, 8, wood); // Sap ucu

        // Metal Bilezik (Ferrule)
        FillRect(pixels, size, 44, 52, 40, 12, steel);

        // Fırça Kılları
        FillRect(pixels, size, 42, 20, 44, 32, bristleColor);
        // Kıl dokusu eklemek için dikey çizgiler
        for (int x = 44; x < 86; x += 4)
        {
            FillRect(pixels, size, x, 20, 2, 32, new Color(0.3f, 0.3f, 0.3f, 1f));
        }
    }

    // 3. Harç Malası (Tool Lvl 2)
    private static void DrawTrowel(Color[] pixels, int size)
    {
        Color wood = new Color(0.55f, 0.35f, 0.2f, 1f);
        Color steel = new Color(0.65f, 0.65f, 0.7f, 1f);
        Color darkSteel = new Color(0.45f, 0.45f, 0.5f, 1f);

        // Sap
        FillRect(pixels, size, 56, 20, 16, 36, wood);
        
        // Metal Kanca
        FillRect(pixels, size, 60, 56, 8, 12, darkSteel);

        // Üçgen Çelik Gövde (Mala ucu yukarı bakacak şekilde)
        for (int y = 68; y < 108; y++)
        {
            int heightFromBase = y - 68;
            int width = Mathf.RoundToInt(64f * (1f - (float)heightFromBase / 40f));
            int xStart = 64 - width / 2;
            
            FillRect(pixels, size, xStart, y, width, 1, steel);
            // Kenarlık gölgesi
            FillRect(pixels, size, xStart, y, 2, 1, darkSteel);
            FillRect(pixels, size, xStart + width - 2, y, 2, 1, darkSteel);
        }
    }

    // 4. Taşçı Çekici (Tool Lvl 3)
    private static void DrawHammer(Color[] pixels, int size)
    {
        Color wood = new Color(0.68f, 0.44f, 0.24f, 1f);
        Color steel = new Color(0.32f, 0.32f, 0.36f, 1f);
        Color lightSteel = new Color(0.5f, 0.5f, 0.55f, 1f);

        // Sap
        FillRect(pixels, size, 58, 20, 12, 70, wood);
        FillRect(pixels, size, 56, 80, 16, 12, steel); // Metal kama yeri

        // Çift Taraflı Ağır Çekiç Kafası
        FillRect(pixels, size, 28, 86, 72, 20, steel);
        FillRect(pixels, size, 24, 88, 4, 16, lightSteel); // Sol sivri/düz uç
        FillRect(pixels, size, 100, 88, 4, 16, lightSteel); // Sağ uç
    }

    // 5. Hassas Iskarpela (Tool Lvl 4+)
    private static void DrawChisel(Color[] pixels, int size)
    {
        Color steel = new Color(0.65f, 0.65f, 0.7f, 1f);
        Color darkSteel = new Color(0.4f, 0.4f, 0.45f, 1f);
        Color brass = new Color(0.8f, 0.65f, 0.2f, 1f);

        // Gövde (Metal)
        FillRect(pixels, size, 58, 40, 12, 60, steel);
        
        // Pirinç Başlık
        FillRect(pixels, size, 54, 100, 20, 10, brass);

        // Eğimli Keskin Uç
        for (int y = 20; y < 40; y++)
        {
            int step = y - 20;
            int width = 12 + step; // Uca doğru genişleyen/daralan yapı
            int xStart = 64 - width / 2;
            FillRect(pixels, size, xStart, y, width, 1, y < 26 ? darkSteel : steel);
        }
    }

    // 6. Doğal Kireç Harcı Kovası (Material Lvl 1)
    private static void DrawMortarBag(Color[] pixels, int size)
    {
        Color bucketColor = new Color(0.25f, 0.45f, 0.6f, 1f); // Mavi plastik kova
        Color mortarColor = new Color(0.85f, 0.85f, 0.8f, 1f); // Açık gri/bej kireç harcı
        Color black = new Color(0.1f, 0.1f, 0.12f, 1f);

        // Kova Gövdesi
        for (int y = 24; y < 76; y++)
        {
            float t = (float)(y - 24) / 52f;
            int width = Mathf.RoundToInt(44f + 16f * t); // Yukarı doğru genişleyen kova
            int xStart = 64 - width / 2;
            FillRect(pixels, size, xStart, y, width, 1, bucketColor);
        }

        // Kova Kenarlığı (Rim)
        FillRect(pixels, size, 34, 76, 60, 6, black);

        // Harç (Bucket Overflow)
        FillRect(pixels, size, 38, 80, 52, 8, mortarColor);
        FillRect(pixels, size, 46, 74, 8, 4, mortarColor); // Damlama efekti
    }

    // 7. Restorasyon Karosu (Material Lvl 2)
    private static void DrawTile(Color[] pixels, int size)
    {
        Color terracotta = new Color(0.8f, 0.38f, 0.2f, 1f);
        Color border = new Color(0.55f, 0.22f, 0.08f, 1f);
        Color patternColor = new Color(0.92f, 0.85f, 0.72f, 1f); // Açık sarı desen

        // Karo Tabanı
        FillRect(pixels, size, 20, 20, 88, 88, terracotta);
        
        // Çevre Kenarlık
        FillRect(pixels, size, 20, 20, 88, 4, border);
        FillRect(pixels, size, 20, 104, 88, 4, border);
        FillRect(pixels, size, 20, 20, 4, 88, border);
        FillRect(pixels, size, 104, 20, 4, 88, border);

        // İç Desen (Orta kesişen çizgiler ve kare motifler)
        FillRect(pixels, size, 62, 28, 4, 72, patternColor);
        FillRect(pixels, size, 28, 62, 72, 4, patternColor);
        
        // Köşe motifleri
        FillRect(pixels, size, 36, 36, 12, 12, patternColor);
        FillRect(pixels, size, 80, 36, 12, 12, patternColor);
        FillRect(pixels, size, 36, 80, 12, 12, patternColor);
        FillRect(pixels, size, 80, 80, 12, 12, patternColor);
    }

    // 8. Yontulmuş Mermer Blok (Material Lvl 3)
    private static void DrawMarble(Color[] pixels, int size)
    {
        Color marbleBase = new Color(0.95f, 0.95f, 0.95f, 1f);
        Color shadow = new Color(0.78f, 0.78f, 0.82f, 1f);
        Color veinColor = new Color(0.6f, 0.6f, 0.65f, 0.7f); // Damarlar

        // Blok Tabanı (3D Küp efekti vermek için gölgelendirmeli)
        FillRect(pixels, size, 20, 20, 88, 88, marbleBase);
        
        // Sağ ve Alt Gölgelendirme kenarlığı
        FillRect(pixels, size, 20, 20, 88, 6, shadow);
        FillRect(pixels, size, 102, 20, 6, 88, shadow);

        // Rastgele Damar Çizgileri (Marble Veins)
        // Sol alt - Sağ üst damar
        DrawVein(pixels, size, 28, 28, 92, 92, veinColor);
        // Çapraz küçük damar
        DrawVein(pixels, size, 40, 80, 80, 30, veinColor);
        // Ekstra damar
        DrawVein(pixels, size, 80, 96, 96, 60, veinColor);
    }

    private static void DrawVein(Color[] pixels, int size, int x0, int y0, int x1, int y1, Color color)
    {
        // Bresenham's Line Algorithm with light noise to represent realistic marble veins
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Damar pikseli yerleştir
            if (x0 >= 20 && x0 <= 104 && y0 >= 20 && y0 <= 104)
            {
                pixels[y0 * size + x0] = color;
                // Kalınlaştırma
                if (x0 + 1 < size) pixels[y0 * size + x0 + 1] = new Color(color.r, color.g, color.b, color.a * 0.5f);
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private static void DrawFaucet(Color[] pixels, int size)
    {
        Color copper = new Color(0.72f, 0.44f, 0.28f, 1f);
        Color darkCopper = new Color(0.48f, 0.25f, 0.12f, 1f);
        Color waterBlue = new Color(0.2f, 0.6f, 0.9f, 0.9f);

        // Yatay boru
        FillRect(pixels, size, 20, 56, 44, 16, copper);
        FillRect(pixels, size, 20, 52, 44, 4, darkCopper);

        // Dikey gövde
        FillRect(pixels, size, 56, 36, 16, 40, copper);
        FillRect(pixels, size, 56, 32, 16, 4, darkCopper);

        // Ağızlık (Spout)
        FillRect(pixels, size, 72, 36, 24, 16, copper);
        FillRect(pixels, size, 80, 20, 16, 16, copper);
        FillRect(pixels, size, 80, 16, 16, 4, darkCopper);

        // Vana Kolu
        FillRect(pixels, size, 44, 80, 40, 8, darkCopper);
        FillRect(pixels, size, 60, 72, 8, 8, darkCopper);

        // Damla
        FillRect(pixels, size, 86, 4, 4, 8, waterBlue);
    }

    private static void DrawFlowerPot(Color[] pixels, int size)
    {
        Color terracotta = new Color(0.82f, 0.42f, 0.24f, 1f);
        Color darkPot = new Color(0.6f, 0.28f, 0.14f, 1f);
        Color green = new Color(0.2f, 0.72f, 0.35f, 1f);
        Color red = new Color(0.92f, 0.25f, 0.3f, 1f);

        // Saksı Gövdesi
        FillRect(pixels, size, 36, 20, 56, 8, terracotta);
        FillRect(pixels, size, 40, 28, 48, 8, terracotta);
        FillRect(pixels, size, 44, 36, 40, 8, terracotta);
        
        // Saksı Ağzı
        FillRect(pixels, size, 32, 44, 64, 8, darkPot);

        // Bitki Sapı
        FillRect(pixels, size, 60, 52, 8, 36, green);
        
        // Yapraklar
        FillRect(pixels, size, 48, 60, 12, 12, green);
        FillRect(pixels, size, 68, 68, 12, 12, green);

        // Çiçek
        FillRect(pixels, size, 56, 88, 16, 16, red);
        FillRect(pixels, size, 60, 104, 8, 8, new Color(0.95f, 0.85f, 0.15f, 1f));
    }

    private static void DrawBench(Color[] pixels, int size)
    {
        Color wood = new Color(0.62f, 0.38f, 0.2f, 1f);
        Color iron = new Color(0.22f, 0.22f, 0.24f, 1f);

        // Sol ayak
        FillRect(pixels, size, 28, 20, 8, 36, iron);
        // Sağ ayak
        FillRect(pixels, size, 92, 20, 8, 36, iron);

        // Oturak
        FillRect(pixels, size, 20, 56, 88, 8, wood);

        // Sırt destek demirleri
        FillRect(pixels, size, 28, 64, 8, 32, iron);
        FillRect(pixels, size, 92, 64, 8, 32, iron);

        // Sırtlık
        FillRect(pixels, size, 20, 80, 88, 12, wood);
        FillRect(pixels, size, 20, 96, 88, 8, wood);
    }

    private static void DrawLantern(Color[] pixels, int size)
    {
        Color iron = new Color(0.2f, 0.2f, 0.22f, 1f);
        Color glass = new Color(0.9f, 0.9f, 0.7f, 0.4f);
        Color glow = new Color(1f, 0.8f, 0.15f, 1f);

        // Alt taban
        FillRect(pixels, size, 44, 20, 40, 8, iron);
        // Cam gövde
        FillRect(pixels, size, 48, 28, 32, 52, glass);
        // Işık
        FillRect(pixels, size, 56, 40, 16, 24, glow);

        // Direkler
        FillRect(pixels, size, 44, 28, 4, 52, iron);
        FillRect(pixels, size, 80, 28, 4, 52, iron);
        FillRect(pixels, size, 62, 28, 4, 52, iron);

        // Üst şapka
        FillRect(pixels, size, 40, 80, 48, 12, iron);
        FillRect(pixels, size, 58, 92, 12, 8, iron);
    }

    private static void DrawAvatarCan(Color[] pixels, int size)
    {
        Color skin = new Color(0.98f, 0.82f, 0.68f, 1f);
        Color hair = new Color(0.42f, 0.23f, 0.1f, 1f);
        Color hat = new Color(0.95f, 0.8f, 0.15f, 1f);
        Color jacket = new Color(0.18f, 0.38f, 0.72f, 1f);

        // Beden (Mavi Ceket)
        FillRect(pixels, size, 36, 20, 56, 28, jacket);
        FillRect(pixels, size, 54, 44, 20, 4, Color.white); // Yaka

        // Yüz
        FillRect(pixels, size, 48, 48, 32, 32, skin);

        // Gözler
        FillRect(pixels, size, 54, 64, 4, 4, Color.black);
        FillRect(pixels, size, 70, 64, 4, 4, Color.black);

        // Ağız
        FillRect(pixels, size, 58, 54, 12, 4, new Color(0.85f, 0.35f, 0.35f, 1f));

        // Saç
        FillRect(pixels, size, 44, 76, 40, 8, hair);

        // Baret
        FillRect(pixels, size, 32, 80, 64, 4, hat);
        FillRect(pixels, size, 40, 84, 48, 16, hat);
    }

    private static void DrawAvatarLeyla(Color[] pixels, int size)
    {
        Color skin = new Color(0.98f, 0.8f, 0.65f, 1f);
        Color hair = new Color(0.15f, 0.15f, 0.18f, 1f);
        Color jacket = new Color(0.82f, 0.22f, 0.28f, 1f);

        // Saç arka planı
        FillRect(pixels, size, 40, 44, 48, 32, hair);

        // Beden (Kırmızı Ceket)
        FillRect(pixels, size, 36, 20, 56, 28, jacket);
        FillRect(pixels, size, 54, 44, 20, 4, Color.white); // Yaka

        // Yüz
        FillRect(pixels, size, 48, 48, 32, 32, skin);

        // Gözlükler
        FillRect(pixels, size, 46, 62, 14, 8, Color.black);
        FillRect(pixels, size, 68, 62, 14, 8, Color.black);
        FillRect(pixels, size, 60, 64, 8, 4, Color.black);

        // Ağız
        FillRect(pixels, size, 58, 54, 12, 4, new Color(0.9f, 0.45f, 0.45f, 1f));

        // Saç üst/perçem
        FillRect(pixels, size, 44, 76, 40, 8, hair);
    }
}
