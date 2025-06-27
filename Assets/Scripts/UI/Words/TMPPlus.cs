using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(StringEmbedder))]
public class TMPPlus : MonoBehaviour
{

    public WordData wordData = null;
    public bool AutoInit = true;
    public bool ForceScrollBar = false;
    public int LineCountToScroll = 4;

    public TMP_Text TC = null;
    public TMP_InputField IF = null;
    public Scrollbar SB = null;

    private WordPreprocessor PP = null;
    public StringEmbedder SE = null;

    public int CharacterOverIndex = -1;

    public int initProgress = -1;
    public bool initialized = false;

    public Vector3 HideBias = new Vector3(-10000, -10000, 0);

    void Start()
    {
        SE = GetComponent<StringEmbedder>();
        if (AutoInit) Init();
    }

    public void Init(WordData wd){
        wordData = wd;
        Init();
    }

    public void Init(string content,float size,bool autoSize = false,Vector2 boxSize = default)
    {
        if (wordData == null) wordData = ScriptableObject.CreateInstance<WordData>();
        wordData.WordContent = content;
        wordData.WordSize = size;
        wordData.AllowFontAutoSize = autoSize;
        if (boxSize != default) wordData.BoxSize = boxSize;
        Init();
    }

    public void Init()
    {
        Assert.IsNotNull(wordData, gameObject.name + "WordData is null!");
        TMP_TextEventHandler textEventHandler;
        TryGetComponent<TMP_TextEventHandler>(out textEventHandler);
        if (textEventHandler == null) textEventHandler = gameObject.AddComponent<TMP_TextEventHandler>();
        if (PP == null) PP = new WordPreprocessor();
        textEventHandler.onCharacterSelection.AddListener(OnCharacterSelection);
        TryGetComponent(out IF);
        if (IF != null) {
            TC = IF.textComponent;
            SB = IF.verticalScrollbar;
            transform.localPosition += HideBias;
            IF.text = PP.PreprocessText(SE.ProcessText(wordData.WordContent));
            if (wordData.WordContent == "") IF.text = " ";
            IF.pointSize = wordData.WordSize;
            PP.InitTagProcess(IF);
        }else {
            TryGetComponent(out TC);
            transform.localPosition += HideBias;
            TC.text = PP.PreprocessText(SE.ProcessText(wordData.WordContent));
            if (wordData.WordContent == "") TC.text = " ";
            TC.fontSize = wordData.WordSize;
            if (wordData.AllowFontAutoSize) TC.enableAutoSizing = true;
            PP.InitTagProcess(TC);
        }
        if (wordData.BoxSize.x > 0 && wordData.BoxSize.y > 0) {
            if (IF != null) IF.textComponent.rectTransform.sizeDelta = wordData.BoxSize;
            else TC.rectTransform.sizeDelta = wordData.BoxSize;
        }
        initProgress = 2;
        initialized = true;
    }

    void OnDestroy()
    {
        if (!initialized) return;
        TMP_TextEventHandler textEventHandler;
        TryGetComponent<TMP_TextEventHandler>(out textEventHandler);
        if (textEventHandler == null) textEventHandler = gameObject.AddComponent<TMP_TextEventHandler>();
        textEventHandler.onCharacterSelection.RemoveListener(OnCharacterSelection);
    }

    void Update()
    {
        if (initialized)
        {
            if(SB != null && ForceScrollBar) {
                if (Input.GetAxis("Mouse ScrollWheel") > 0) 
                    SB.value -= 1.0f/TC.textInfo.lineCount*LineCountToScroll;
                else if (Input.GetAxis("Mouse ScrollWheel") < 0) 
                    SB.value += 1.0f/TC.textInfo.lineCount*LineCountToScroll;
            }
            int wd_id = -1;
            PP.ProcessTag(CharacterOverIndex,-1*Vector4.one,out CharacterOverIndex,out wd_id);
            if (initProgress == 0) transform.localPosition -= HideBias;
            if (initProgress != -1) initProgress--;
        }
    }

    public void Reset(){
        if(PP!=null) PP.Reset();
        initialized = false;
    }

    public void OnCharacterSelection(char character, int characterIndex)
    {
        CharacterOverIndex = characterIndex;
    }
}
