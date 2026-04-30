/// <summary>
/// Tüm sýfýrlanabilir mekanikler (Kutu, Anahtar, Buton) için ortak sözleþme.
/// (Common contract for all resettable mechanics.)
/// </summary>
public interface IResettable
{
    // Bu kimliðe sahip herkesin "ResetMechanic" diye bir fonksiyonu olmak ZORUNDA!
    void ResetMechanic();
}