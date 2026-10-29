using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Sadece eklendiği level için karakterin başlangıç noktasını değiştirir ve
/// tag girilen objelerin konumunu ya da Y eksenindeki ölçeğini (scale) değiştirir.
/// Mobil cihazlar için GC yaratmaması adına profesyonelce optimize edilmiştir.
/// </summary>
public class LevelTransformModifier : MonoBehaviour
{
    [Header("Karakter Başlangıç Noktası (Sadece bu level için)")]
    [SerializeField] private bool _overridePlayerStartPos = false;
    [SerializeField] private Vector2 _newPlayerStartPosition;
    
    private Vector2 _originalStartPos;

    [System.Serializable]
    public class TagModifier
    {
        public string targetTag;
        
        [Header("Komple Ölçek (Scale) Değiştirme")]
        public bool modifyScale = false;
        [Tooltip("Objenin yeni boyutları (X, Y, Z).")]
        public Vector3 newScale = Vector3.one;

        [Header("Konum Değiştirme")]
        public bool modifyPosition = false;
        [Tooltip("True ise objeyi bu konuma ekler. False ise direkt bu konuma ışınlar.")]
        public bool isOffset = false; 
        public Vector3 newPositionOrOffset;
    }

    private struct ScriptState
    {
        public MonoBehaviour script;
        public FieldInfo startPosField;
        public object originalStartPos;
        public FieldInfo targetPosField;
        public object originalTargetPos;
    }

    private struct ObjectState
    {
        public GameObject obj;
        public bool scaleModified;
        public Vector3 originalScale;
        public bool posModified;
        public Vector3 originalPosition;
    }

    private List<ObjectState> _originalObjectStates = new List<ObjectState>();
    private List<ScriptState> _originalScriptStates = new List<ScriptState>();
    private HashSet<MonoBehaviour> _processedScripts = new HashSet<MonoBehaviour>();

    [Header("Tag ile Obje Değiştirme Listesi")]
    [SerializeField] private List<TagModifier> _tagModifiers = new List<TagModifier>();

    // GC oluşumunu engellemek için yeniden kullanılabilir (reusable) liste
    private readonly List<MonoBehaviour> _reusableScriptList = new List<MonoBehaviour>();
    private readonly List<MonoBehaviour> _reusableParentList = new List<MonoBehaviour>();

    private void Awake()
    {
        if (_tagModifiers == null) return;

        for (int i = 0; i < _tagModifiers.Count; i++)
        {
            var modifier = _tagModifiers[i];
            if (string.IsNullOrEmpty(modifier.targetTag)) continue;

            GameObject[] targets = GameObject.FindGameObjectsWithTag(modifier.targetTag);
            
            for (int j = 0; j < targets.Length; j++)
            {
                GameObject obj = targets[j];
                ObjectState state = new ObjectState
                {
                    obj = obj,
                    scaleModified = modifier.modifyScale,
                    posModified = modifier.modifyPosition
                };

                if (modifier.modifyScale)
                {
                    state.originalScale = obj.transform.localScale;
                    obj.transform.localScale = modifier.newScale;
                }

                if (modifier.modifyPosition)
                {
                    state.originalPosition = obj.transform.position;

                    if (modifier.isOffset)
                    {
                        obj.transform.position += modifier.newPositionOrOffset;
                    }
                    else
                    {
                        obj.transform.position = modifier.newPositionOrOffset;
                    }
                    
                    UpdateComponentStartPos(obj);
                }

                _originalObjectStates.Add(state);
            }
        }
    }

    private void Start()
    {
        if (_overridePlayerStartPos && PlayerController.Instance != null)
        {
            FieldInfo startPosField = typeof(PlayerController).GetField("startPos", BindingFlags.NonPublic | BindingFlags.Instance);
            if (startPosField != null)
            {
                _originalStartPos = (Vector2)startPosField.GetValue(PlayerController.Instance);
                startPosField.SetValue(PlayerController.Instance, _newPlayerStartPosition);
            }
            
            PlayerController.Instance.transform.position = _newPlayerStartPosition;
        }
    }

    private void UpdateComponentStartPos(GameObject obj)
    {
        // Mobil Optimizasyon: Her çağrıda yeni liste (new List) oluşturmak çöp (Garbage) yaratır ve cihazı yorar.
        // Bunun yerine önbelleklenmiş listeyi (reusable list) temizleyip non-allocating metodla dolduruyoruz.
        _reusableScriptList.Clear();
        
        obj.GetComponentsInChildren<MonoBehaviour>(true, _reusableScriptList);

        if (obj.transform.parent != null)
        {
            _reusableParentList.Clear();
            obj.GetComponentsInParent<MonoBehaviour>(true, _reusableParentList);
            _reusableScriptList.AddRange(_reusableParentList);
        }

        for (int i = 0; i < _reusableScriptList.Count; i++)
        {
            var script = _reusableScriptList[i];
            if (script == this || script == null || _processedScripts.Contains(script)) continue;
            
            System.Type type = script.GetType();
            
            FieldInfo startPosField = type.GetField("startPos", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (startPosField == null) startPosField = type.GetField("_startPos", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            
            if (startPosField != null)
            {
                ScriptState sState = new ScriptState();
                sState.script = script;
                sState.startPosField = startPosField;
                sState.originalStartPos = startPosField.GetValue(script);

                if (startPosField.FieldType == typeof(Vector3)) startPosField.SetValue(script, script.transform.position);
                else if (startPosField.FieldType == typeof(Vector2)) startPosField.SetValue(script, (Vector2)script.transform.position);
                
                FieldInfo targetPosField = type.GetField("targetPos", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (targetPosField == null) targetPosField = type.GetField("_targetPos", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                
                FieldInfo moveOffsetField = type.GetField("moveOffset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (moveOffsetField == null) moveOffsetField = type.GetField("_moveOffset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                
                if (targetPosField != null && moveOffsetField != null && moveOffsetField.FieldType == typeof(Vector3))
                {
                    sState.targetPosField = targetPosField;
                    sState.originalTargetPos = targetPosField.GetValue(script);

                    Vector3 offset = (Vector3)moveOffsetField.GetValue(script);
                    targetPosField.SetValue(script, script.transform.position + offset);
                }

                _originalScriptStates.Add(sState);
                _processedScripts.Add(script);
            }
        }
    }

    private void OnDisable()
    {
        if (_overridePlayerStartPos && PlayerController.Instance != null)
        {
            FieldInfo startPosField = typeof(PlayerController).GetField("startPos", BindingFlags.NonPublic | BindingFlags.Instance);
            if (startPosField != null)
            {
                startPosField.SetValue(PlayerController.Instance, _originalStartPos);
            }
        }

        // Değiştirilmiş script değerlerini eski haline getir
        for (int i = 0; i < _originalScriptStates.Count; i++)
        {
            var sState = _originalScriptStates[i];
            if (sState.script != null)
            {
                if (sState.startPosField != null)
                {
                    sState.startPosField.SetValue(sState.script, sState.originalStartPos);
                }
                if (sState.targetPosField != null)
                {
                    sState.targetPosField.SetValue(sState.script, sState.originalTargetPos);
                }
            }
        }

        // Değiştirilmiş objeleri eski konum/ölçeklerine getir
        for (int i = 0; i < _originalObjectStates.Count; i++)
        {
            var oState = _originalObjectStates[i];
            if (oState.obj != null)
            {
                if (oState.scaleModified)
                {
                    oState.obj.transform.localScale = oState.originalScale;
                }
                if (oState.posModified)
                {
                    oState.obj.transform.position = oState.originalPosition;
                }
            }
        }
        
        _originalScriptStates.Clear();
        _originalObjectStates.Clear();
        _processedScripts.Clear();
    }
}
