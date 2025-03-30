using UnityEngine;
using System.Collections.Generic;

public class Match3Grid : MonoBehaviour
{
    public int width = 8;  // Grid geniþliði
    public int height = 8; // Grid yüksekliði
    public GameObject[] gemPrefabs;  // Objeleri tanýmladýðýnýz prefablar
    public GameObject gridCellPrefab; // Grid hücrelerini temsil eden prefab (sprite)
    private GameObject[,] grid;  // Griddeki objeleri tutacak dizi
    private GameObject selectedGem;  // Seçilen ilk obje
    private GameObject targetGem;  // Seçilen ikinci obje
    private bool isSwapping = false;  // Yer deðiþtirme iþlemi olup olmadýðýný kontrol etmek için

    void Start()
    {
        grid = new GameObject[width, height];  // 8x8 grid oluþturuluyor
        GenerateGrid();  // Grid oluþturuluyor
    }

    // Gridi oluþturma fonksiyonu
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Grid hücresinin pozisyonunu belirliyoruz
                Vector2 position = new Vector2(x, y);

                // Her hücreye bir temel sprite (grid cell) ekliyoruz
                AddGridCell(x, y, position);

                // Sprite'leri spawn ediyoruz
                SpawnGem(x, y, position);
            }
        }
    }

    // Her grid hücresine temel bir sprite ekleme fonksiyonu
    void AddGridCell(int x, int y, Vector2 position)
    {
        // Grid hücresini spawn ediyoruz
        GameObject gridCell = Instantiate(gridCellPrefab, position, Quaternion.identity);
        gridCell.transform.parent = transform;  // Bu objeyi bu script'in objesine çocuk yapýyoruz

        // Grid hücresini grid dizisinde saklýyoruz
        grid[x, y] = gridCell;
    }

    // Her hücreye sprite spawn etme fonksiyonu
    void SpawnGem(int x, int y, Vector2 position)
    {
        // 5 farklý sprite'dan rastgele birini seçiyoruz
        int randomIndex = Random.Range(0, gemPrefabs.Length);

        // Seçilen sprite'ý spawn ediyoruz
        GameObject gem = Instantiate(gemPrefabs[randomIndex], position, Quaternion.identity);
        gem.transform.parent = transform;  // Gem objesini bu script'in objesine çocuk yapýyoruz

        // Opsiyonel: Sprite'lara Collider ekleyebiliriz (örneðin BoxCollider2D)
        gem.AddComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Mouse týklamasýyla objeleri seçiyoruz
        {
            SelectGem();
        }

        if (isSwapping)  // Eðer objeler yer deðiþtiriyorsa, iþlemi baþlatýyoruz
        {
            SwapGems();
        }
    }

    // Ýki objeyi seçme fonksiyonu
    void SelectGem()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null)
        {
            GameObject clickedGem = hit.gameObject;

            if (selectedGem == null)  // Ýlk obje seçildi
            {
                selectedGem = clickedGem;
            }
            else  // Ýkinci obje seçildi
            {
                targetGem = clickedGem;

                // Yer deðiþtirme iþlemini baþlat
                isSwapping = true;
            }
        }
    }

    // Ýki objeyi yer deðiþtirme fonksiyonu
    void SwapGems()
    {
        // Objelerin yer deðiþtirmesi için pozisyonlarýný doðrudan deðiþtiriyoruz
        Vector2 selectedPos = selectedGem.transform.position;
        Vector2 targetPos = targetGem.transform.position;

        // Objelerin pozisyonlarýný deðiþtiriyoruz
        selectedGem.transform.position = targetPos;
        targetGem.transform.position = selectedPos;

        // Yer deðiþtirme tamamlandý
        isSwapping = false;

        // Eþleþme kontrolünü yap
        CheckMatches();

        // Seçilen objeleri sýfýrlýyoruz
        selectedGem = null;
        targetGem = null;
    }

    // Eþleþmeleri kontrol etme fonksiyonu
    void CheckMatches()
    {
        List<Vector2Int> matchedPositions = new List<Vector2Int>();

        // Yatay eþleþmeleri kontrol et
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                if (grid[x, y] != null &&
                    grid[x + 1, y] != null &&
                    grid[x + 2, y] != null &&
                    grid[x, y].tag == grid[x + 1, y].tag &&
                    grid[x + 1, y].tag == grid[x + 2, y].tag)
                {
                    matchedPositions.Add(new Vector2Int(x, y));
                    matchedPositions.Add(new Vector2Int(x + 1, y));
                    matchedPositions.Add(new Vector2Int(x + 2, y));
                }
            }
        }

        // Dikey eþleþmeleri kontrol et
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                if (grid[x, y] != null &&
                    grid[x, y + 1] != null &&
                    grid[x, y + 2] != null &&
                    grid[x, y].tag == grid[x, y + 1].tag &&
                    grid[x, y + 1].tag == grid[x, y + 2].tag)
                {
                    matchedPositions.Add(new Vector2Int(x, y));
                    matchedPositions.Add(new Vector2Int(x, y + 1));
                    matchedPositions.Add(new Vector2Int(x, y + 2));
                }
            }
        }

        // Eþleþen objeleri yok et
        foreach (Vector2Int pos in matchedPositions)
        {
            Destroy(grid[pos.x, pos.y]); // Obje yok ediliyor
            grid[pos.x, pos.y] = null;   // Griddeki referansý kaldýr
        }

        // Obje kaydýrma iþlemi
        DropGems();

        // Yeni objeleri spawn et
        SpawnNewGems();
    }

    // Üstteki objeleri aþaðýya kaydýrma fonksiyonu
    void DropGems()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) // Eðer burada boþluk varsa
                {
                    for (int k = y + 1; k < height; k++)
                    {
                        if (grid[x, k] != null) // Yukarýdan bir obje varsa
                        {
                            grid[x, y] = grid[x, k]; // Obje aþaðý kaydýrýlýyor
                            grid[x, k] = null;      // Eski yerini boþalt
                            grid[x, y].transform.position = new Vector2(x, y); // Yeni pozisyona taþý
                            break;
                        }
                    }
                }
            }
        }
    }

    // Eksik kalan objeleri yukarýdan yeni ekleme fonksiyonu
    void SpawnNewGems()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) // Eðer boþ bir alan varsa
                {
                    Vector2 position = new Vector2(x, y);
                    int randomIndex = Random.Range(0, gemPrefabs.Length);
                    GameObject gem = Instantiate(gemPrefabs[randomIndex], position, Quaternion.identity);
                    gem.transform.parent = transform;
                    grid[x, y] = gem; // Griddeki referansý güncelle
                }
            }
        }
    }

}
