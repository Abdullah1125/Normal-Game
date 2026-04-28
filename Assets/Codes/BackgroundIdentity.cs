using UnityEngine;

/// <summary>
/// Static reference provider for the background scene object.
/// (Sahnede duran arka plan objesi için statik referans saðlar.)
/// </summary>
public class BackgroundIdentity : MonoBehaviour
{
    // Tabelayý tüm prefablarýn görebileceði þekilde gökyüzüne asýyoruz
    public static GameObject Instance;

    void Awake()
    {
        // Sahne açýldýðý an "Arka plan benim!" diyerek adresi yazar
        Instance = this.gameObject;
    }
}