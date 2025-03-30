using UnityEngine;
using System.Collections.Generic;

public class Match3Grid : MonoBehaviour
{
    public int width = 8;  // Grid geni�li�i
    public int height = 8; // Grid y�ksekli�i
    public GameObject[] gemPrefabs;  // Objeleri tan�mlad���n�z prefablar
    public GameObject gridCellPrefab; // Grid h�crelerini temsil eden prefab (sprite)
    private GameObject[,] grid;  // Griddeki objeleri tutacak dizi
    private GameObject selectedGem;  // Se�ilen ilk obje
    private GameObject targetGem;  // Se�ilen ikinci obje
    private bool isSwapping = false;  // Yer de�i�tirme i�lemi olup olmad���n� kontrol etmek i�in

    void Start()
    {
        grid = new GameObject[width, height];  // 8x8 grid olu�turuluyor
        GenerateGrid();  // Grid olu�turuluyor
    }

    // Gridi olu�turma fonksiyonu
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Grid h�cresinin pozisyonunu belirliyoruz
                Vector2 position = new Vector2(x, y);

                // Her h�creye bir temel sprite (grid cell) ekliyoruz
                AddGridCell(x, y, position);

                // Sprite'leri spawn ediyoruz
                SpawnGem(x, y, position);
            }
        }
    }

    // Her grid h�cresine temel bir sprite ekleme fonksiyonu
    void AddGridCell(int x, int y, Vector2 position)
    {
        // Grid h�cresini spawn ediyoruz
        GameObject gridCell = Instantiate(gridCellPrefab, position, Quaternion.identity);
        gridCell.transform.parent = transform;  // Bu objeyi bu script'in objesine �ocuk yap�yoruz

        // Grid h�cresini grid dizisinde sakl�yoruz
        grid[x, y] = gridCell;
    }

    // Her h�creye sprite spawn etme fonksiyonu
    void SpawnGem(int x, int y, Vector2 position)
    {
        // 5 farkl� sprite'dan rastgele birini se�iyoruz
        int randomIndex = Random.Range(0, gemPrefabs.Length);

        // Se�ilen sprite'� spawn ediyoruz
        GameObject gem = Instantiate(gemPrefabs[randomIndex], position, Quaternion.identity);
        gem.transform.parent = transform;  // Gem objesini bu script'in objesine �ocuk yap�yoruz

        // Opsiyonel: Sprite'lara Collider ekleyebiliriz (�rne�in BoxCollider2D)
        gem.AddComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Mouse t�klamas�yla objeleri se�iyoruz
        {
            SelectGem();
        }

        if (isSwapping)  // E�er objeler yer de�i�tiriyorsa, i�lemi ba�lat�yoruz
        {
            SwapGems();
        }
    }

    // �ki objeyi se�me fonksiyonu
    void SelectGem()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null)
        {
            GameObject clickedGem = hit.gameObject;

            if (selectedGem == null)  // �lk obje se�ildi
            {
                selectedGem = clickedGem;
            }
            else  // �kinci obje se�ildi
            {
                targetGem = clickedGem;

                // Yer de�i�tirme i�lemini ba�lat
                isSwapping = true;
            }
        }
    }

    // �ki objeyi yer de�i�tirme fonksiyonu
    void SwapGems()
    {
        // Objelerin yer de�i�tirmesi i�in pozisyonlar�n� do�rudan de�i�tiriyoruz
        Vector2 selectedPos = selectedGem.transform.position;
        Vector2 targetPos = targetGem.transform.position;

        // Objelerin pozisyonlar�n� de�i�tiriyoruz
        selectedGem.transform.position = targetPos;
        targetGem.transform.position = selectedPos;

        // Yer de�i�tirme tamamland�
        isSwapping = false;

        // E�le�me kontrol�n� yap
        CheckMatches();

        // Se�ilen objeleri s�f�rl�yoruz
        selectedGem = null;
        targetGem = null;
    }

    // E�le�meleri kontrol etme fonksiyonu
    void CheckMatches()
    {
        List<Vector2Int> matchedPositions = new List<Vector2Int>();

        // Yatay e�le�meleri kontrol et
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

        // Dikey e�le�meleri kontrol et
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

        // E�le�en objeleri yok et
        foreach (Vector2Int pos in matchedPositions)
        {
            Destroy(grid[pos.x, pos.y]); // Obje yok ediliyor
            grid[pos.x, pos.y] = null;   // Griddeki referans� kald�r
        }

        // Obje kayd�rma i�lemi
        DropGems();

        // Yeni objeleri spawn et
        SpawnNewGems();
    }

    // �stteki objeleri a�a��ya kayd�rma fonksiyonu
    void DropGems()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) // E�er burada bo�luk varsa
                {
                    for (int k = y + 1; k < height; k++)
                    {
                        if (grid[x, k] != null) // Yukar�dan bir obje varsa
                        {
                            grid[x, y] = grid[x, k]; // Obje a�a�� kayd�r�l�yor
                            grid[x, k] = null;      // Eski yerini bo�alt
                            grid[x, y].transform.position = new Vector2(x, y); // Yeni pozisyona ta��
                            break;
                        }
                    }
                }
            }
        }
    }

    // Eksik kalan objeleri yukar�dan yeni ekleme fonksiyonu
    void SpawnNewGems()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) // E�er bo� bir alan varsa
                {
                    Vector2 position = new Vector2(x, y);
                    int randomIndex = Random.Range(0, gemPrefabs.Length);
                    GameObject gem = Instantiate(gemPrefabs[randomIndex], position, Quaternion.identity);
                    gem.transform.parent = transform;
                    grid[x, y] = gem; // Griddeki referans� g�ncelle
                }
            }
        }
    }

}
