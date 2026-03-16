using UnityEngine;

public class CameraRoomController : MonoBehaviour
{
    public float gecisHizi = 5f;
    private Vector3 anaOdaPos;
    private Vector3 hedefPos;

    void Awake()
    {
        anaOdaPos = transform.position;
        hedefPos = anaOdaPos;
    }

    void Update()
    {
        // Kamerayý hedef konuma yumuþakça kaydýrýr
        transform.position = Vector3.Lerp(transform.position, hedefPos, gecisHizi * Time.deltaTime);
    }

    public void OdayiDegistir(bool gizliGecitteMi)
    {
        LevelData veri = LevelManager.Instance.aktifLevel;

        if (veri != null && veri.gizliGecitOdasiVar)
        {
            hedefPos = gizliGecitteMi ? veri.gizliOdaPozisyonu : anaOdaPos;
        }
    }

    public void KameraSifirla()
    {
        hedefPos = anaOdaPos;
    }
}
