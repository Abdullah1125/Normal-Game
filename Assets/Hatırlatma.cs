/*



public class SabitLazerTuzagi : MonoBehaviour, IResettable

//startta
  if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

private void OnDestroy()
{
    // Obje silinirken LevelManager'ın listesini de temizliyoruz
    if (LevelManager.Instance != null)
    {
        // Eğer LevelManager'da RemoveResettable fonksiyonu yoksa aşağıya ekledim
        LevelManager.Instance.UnregisterResettable(this);
    }
}

public void ResetMechanic
 
 
 // butonlarda 
    [Header("Events (Olaylar)")]
    public UnityEvent OnButtonPressed; gibi bir şey eklenebilir hepsine 

 
 level başında sıfırlanabilcekler IResettable 
 
 
 Instance This fln görürsen unutma tag işini tag olursada singleton sistemi var

 
 
 
 
 
 */