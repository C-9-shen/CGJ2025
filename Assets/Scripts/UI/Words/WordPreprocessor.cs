using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text;

public class WordPreprocessor
{
    public List<string> TagList = new List<string>();
    public List<List<string>> ParamList = new();
    public List<int> StartPosList = new List<int>();
    public List<int> EndPosList = new List<int>();
    
    public TMP_Text TextComponent;
    public TMP_InputField InputField;

    public int ProcessedTextLength = 0;
    public int UnProcessedTextLength = 0;
    public string ProcessedText = null;
    public string UnProcessedText = null;

    Link2Data L2D;
    List<int> TmpStartPosList = new List<int>();
    List<int> TmpEndPosList = new List<int>();
    List<int> StartPosCompute = new List<int>();
    List<int> EndPosCompute = new List<int>();
    List<int> StartPosListUnprocessed = new List<int>();
    List<int> EndPosListUnprocessed = new List<int>();
    char[] ChineseChars;

    public List<float> TypeStartTime = new List<float>();
    public List<float> TypeDelayTime = new List<float>();
    public List<int> TypeTypes = new List<int>();

    public bool initialized = false;
    public bool type_setting_initialized = false;
    public bool isTyping = false;
    public float TypeST = -1;

    TMP_TextInfo OInfo;
    Transform Parent_Transform;
    int lastOverIndex=-1;

    public void Reset(){
        TagList.Clear();
        ParamList.Clear();
        StartPosList.Clear();
        EndPosList.Clear();
        TmpStartPosList.Clear();
        TmpEndPosList.Clear();
        StartPosCompute.Clear();
        EndPosCompute.Clear();
        StartPosListUnprocessed.Clear();
        EndPosListUnprocessed.Clear();
        TypeStartTime.Clear();
        TypeDelayTime.Clear();
        TypeTypes.Clear();
        type_setting_initialized = false;
        glitch_LastTimeList.Clear();
        lastOverIndex = -1;
        initialized = false;
        isTyping = false;
        TypeST = -1;
        ProcessedTextLength = 0;
        UnProcessedTextLength = 0;
        ProcessedText = null;
        UnProcessedText = null;
    }

    public WordPreprocessor(){
        L2D = (Link2Data)Resources.Load("Words/Link2Data");
    }

    public int Tag2Index(string tag){
        for(int i = 0; i < L2D.wordBoxDataList.Count; i++)
            if(L2D.wordBoxDataList[i].ID == tag)
                return i;
        return -1;
    }

    public WordData Tag2Data(string tag){
        for(int i = 0; i < L2D.wordBoxDataList.Count; i++)
            if(L2D.wordBoxDataList[i].ID == tag)
                return L2D.wordBoxDataList[i];
        return null;
    }

    public WordData Index2Data(int index){
        return L2D.wordBoxDataList[index];
    }

    public void RemoveTag(int index){
        TagList.RemoveAt(index);
        ParamList.RemoveAt(index);
        StartPosList.RemoveAt(index);
        EndPosList.RemoveAt(index);
        StartPosListUnprocessed.RemoveAt(index);
        EndPosListUnprocessed.RemoveAt(index);
    }

    private TMP_TextInfo CloneTMP_TextInfo(TMP_TextInfo original)
    {
        TMP_TextInfo clone = new TMP_TextInfo();
        clone.characterInfo = (TMP_CharacterInfo[])original.characterInfo.Clone();
        clone.wordInfo = (TMP_WordInfo[])original.wordInfo.Clone();
        clone.linkInfo = (TMP_LinkInfo[])original.linkInfo.Clone();
        clone.lineInfo = (TMP_LineInfo[])original.lineInfo.Clone();
        clone.pageInfo = (TMP_PageInfo[])original.pageInfo.Clone();
        clone.meshInfo = new TMP_MeshInfo[original.meshInfo.Length];
        for (int i = 0; i < original.meshInfo.Length; i++)
        {
            clone.meshInfo[i] = new TMP_MeshInfo();
            clone.meshInfo[i].vertices = (Vector3[])original.meshInfo[i].vertices.Clone();
            clone.meshInfo[i].uvs0 = (Vector2[])original.meshInfo[i].uvs0.Clone();
            clone.meshInfo[i].uvs2 = (Vector2[])original.meshInfo[i].uvs2.Clone();
            clone.meshInfo[i].colors32 = (Color32[])original.meshInfo[i].colors32.Clone();
            clone.meshInfo[i].triangles = (int[])original.meshInfo[i].triangles.Clone();
        }
        return clone;
    }

    public string PreprocessText(string text){
        TagList.Clear();
        ParamList.Clear();
        StartPosList.Clear();
        EndPosList.Clear();
        initialized = false;
        if (text == null || text == "") return "";
        string res = text;
        res = ProcessTextRecursively_1(res,0); PosListCompute();
        res = ProcessTextRecursively_2(res,0); PosListCompute(); AddTmpPos();
        res = ProcessTextRecursively_3(res,0); PosListCompute(); AddTmpPos();
        for(int i = 0; i < StartPosList.Count; i++){
            StartPosListUnprocessed.Add(StartPosList[i]);
            EndPosListUnprocessed.Add(EndPosList[i]);
        }
        ProcessTextRecursively_3half(res,0); PosListCompute(); AddTmpPos();
        for(int i = 0; i < StartPosList.Count; i++){
            int temp = StartPosList[i];
            StartPosList[i] = StartPosListUnprocessed[i];
            StartPosListUnprocessed[i] = temp;
            temp = EndPosList[i];
            EndPosList[i] = EndPosListUnprocessed[i];
            EndPosListUnprocessed[i] = temp;
        }
        res = ProcessTextRecursively_4(res,0); PosListCompute(); AddTmpPos();
        UnProcessedTextLength = res.Length;
        UnProcessedText = res;
        ProcessedTextLength = RemoveAllTags(res).Length;
        ProcessedText = RemoveAllTags(res);
        SortTag();
        return res;
    }

    public void InitTagProcess(TMP_Text tar){
        Parent_Transform = tar.transform;
        TextComponent = tar;
        UniversalInit();
    }

    public void InitTagProcess(TMP_InputField tar){
        Parent_Transform = tar.textComponent.transform;
        InputField = tar;
        TextComponent = tar.textComponent;
        UniversalInit();
    }

    private void UniversalInit(){
        TextComponent.ForceMeshUpdate();
        GetChineseCharList();
        OInfo = CloneTMP_TextInfo(TextComponent.textInfo);
        for(int i = 0; i < EndPosList.Count; i++) EndPosList[i] = EndPosList[i]==-1?ProcessedTextLength:EndPosList[i];
        for(int i = 0; i < EndPosList.Count; i++) EndPosListUnprocessed[i] = EndPosListUnprocessed[i]==-1?UnProcessedTextLength:EndPosListUnprocessed[i];
        isTyping = false;
        for(int i = 0; i < TagList.Count; i++) if(TagList[i] == "type") isTyping = true;
    }

    public void GetChineseCharList(){
        string path = "Fonts/ch_text";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            string content = textAsset.text;
            ChineseChars = content.ToCharArray();
        }
        else
        {
            Debug.LogError("Failed to load the text file from Resources.");
        }
    }

    List<string> DecodeParam(string param){
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder currentParam = new StringBuilder();

        foreach (char c in param)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes){
                result.Add(currentParam.ToString());
                currentParam.Clear();
            }
            else currentParam.Append(c);
        }
        if (currentParam.Length > 0) result.Add(currentParam.ToString());
        return result;
    }

    private void PosListCompute(){
        // Debug.Log(StartPosList.Count+" "+StartPosCompute.Count);
        for(int i = 0; i < StartPosCompute.Count; i++){
            StartPosList[i] += StartPosCompute[i];
            EndPosList[i] += EndPosCompute[i];
            StartPosCompute[i] = 0;
            EndPosCompute[i] = 0;
        }
        int tar = StartPosList.Count - StartPosCompute.Count;
        for(int i = 0; i < tar; i++){
            StartPosCompute.Add(0);
            EndPosCompute.Add(0);
        }
    }

    private string ProcessTextRecursively_1(string text,int offset){
        System.Text.RegularExpressions.Regex regex_double_param = new(@"<(" + string.Join("|", MatchTag) + @")=(.*?)>(.*?)<\/\1>");
        
        System.Text.RegularExpressions.MatchCollection matches1 = regex_double_param.Matches(text);
        int localOffset = 0;
        for (int i = 0; i < matches1.Count ; i++)
        {
            System.Text.RegularExpressions.Match match = matches1[i];
            string tag = match.Groups[1].Value;
            if (System.Array.Exists(MatchTag, element => element == tag))
            {
            string innerText = ProcessTextRecursively_1(match.Groups[3].Value, match.Index + offset + localOffset);
            text = text.Substring(0, match.Index + localOffset) + innerText + text.Substring(match.Index + match.Length + localOffset);
            TagList.Add(tag);
            ParamList.Add(DecodeParam(match.Groups[2].Value));
            StartPosList.Add(match.Index + offset + localOffset);
            EndPosList.Add(match.Index + innerText.Length + offset + localOffset);
            localOffset += innerText.Length - match.Length;
            }
        }
        return text;
    }

    private string ProcessTextRecursively_2(string text,int offset){
        System.Text.RegularExpressions.Regex regex_single_param = new(@"<(" + string.Join("|", MatchTag) + @")=(.*?)>");
        System.Text.RegularExpressions.MatchCollection matches2 = regex_single_param.Matches(text);
        int localOffset = 0;
        for (int i = 0; i < matches2.Count ; i++)
        {
            System.Text.RegularExpressions.Match match = matches2[i];
            string tag = match.Groups[1].Value;
            if (System.Array.Exists(MatchTag, element => element == tag))
            {
                text = text.Substring(0, match.Index + localOffset) + text.Substring(match.Index + match.Length + localOffset);
                TagList.Add(tag);
                ParamList.Add(DecodeParam(match.Groups[2].Value));
                TmpStartPosList.Add(match.Index + offset + localOffset);
                TmpEndPosList.Add(-1);
                localOffset -= match.Length;
                for(int j = 0; j < StartPosList.Count; j++){
                    // Debug.Log(StartPosList.Count+" "+StartPosCompute.Count);
                    if(StartPosList[j] > match.Index) StartPosCompute[j] -= match.Length;
                    if(EndPosList[j] > match.Index) EndPosCompute[j] -= match.Length;
                }
            }
        }
        return text;
    }

    private string ProcessTextRecursively_3(string text,int offset){
        System.Text.RegularExpressions.Regex regex_double = new(@"<(.*?)>(.*?)<\/\1>");
        System.Text.RegularExpressions.MatchCollection matches3 = regex_double.Matches(text);
        int localOffset = 0;
        for (int i = 0; i < matches3.Count ; i++)
        {
            System.Text.RegularExpressions.Match match = matches3[i];
            string Tag = match.Groups[1].Value;
            if (System.Array.Exists(MatchTag, element => element == Tag))
            {
                string innerText = ProcessTextRecursively_3(match.Groups[2].Value, match.Index + offset + localOffset);
                text = text.Substring(0, match.Index + localOffset) + innerText + text.Substring(match.Index + match.Length + localOffset);
                TagList.Add(Tag);
                ParamList.Add(new List<string>());
                TmpStartPosList.Add(match.Index + offset + localOffset);
                TmpEndPosList.Add(match.Index + innerText.Length + offset + localOffset);
                localOffset += innerText.Length - match.Length;
                for(int j = 0; j < StartPosList.Count; j++){
                    if(StartPosList[j] > match.Index) StartPosCompute[j] -= match.Length-innerText.Length;
                    if(EndPosList[j] > match.Index) EndPosCompute[j] -= match.Length-innerText.Length;
            }
            }
        }
        return text;
    }

    private string ProcessTextRecursively_3half(string text,int offset){
        System.Text.RegularExpressions.Regex regex_single = new(@"<(.*?)>");
        System.Text.RegularExpressions.MatchCollection matches4 = regex_single.Matches(text);
        int localOffset = 0;
        foreach (System.Text.RegularExpressions.Match match in matches4)
        {
            string Tag = match.Groups[1].Value;
            if (System.Array.Exists(MatchTag, element => element == Tag))
            {
                text = text.Substring(0, match.Index + localOffset) + text.Substring(match.Index + match.Length + localOffset);
                TmpStartPosList.Add(match.Index + offset + localOffset);
                TmpEndPosList.Add(-1);
                localOffset -= match.Length;
                for(int i = 0; i < StartPosList.Count; i++){
                    if(StartPosList[i] > match.Index) StartPosCompute[i] -= match.Length;
                    if(EndPosList[i] > match.Index) EndPosCompute[i] -= match.Length;
                }
            }
            
        }
        return text;
    }

    private string ProcessTextRecursively_4(string text,int offset){
        System.Text.RegularExpressions.Regex regex_single = new(@"<(.*?)>");
        System.Text.RegularExpressions.MatchCollection matches4 = regex_single.Matches(text);
        int localOffset = 0;
        for (int i = 0; i < matches4.Count ; i++)
        {
            System.Text.RegularExpressions.Match match = matches4[i];
            string Tag = match.Groups[1].Value;
            if (System.Array.Exists(MatchTag, element => element == Tag))
            {
                text = text.Substring(0, match.Index + localOffset) + text.Substring(match.Index + match.Length + localOffset);
                TagList.Add(Tag);
                ParamList.Add(new List<string>());
                TmpStartPosList.Add(match.Index + offset + localOffset);
                TmpEndPosList.Add(-1);
            }
            localOffset -= match.Length;
            for(int j = 0; j < StartPosList.Count; j++){
                if(StartPosList[j] > match.Index) StartPosCompute[j] -= match.Length;
                if(EndPosList[j] > match.Index) EndPosCompute[j] -= match.Length;
            }
        }
        return text;
    }

    private string RemoveAllTags(string text)
    {
        System.Text.RegularExpressions.Regex regex = new(@"<.*?>");
        return regex.Replace(text, string.Empty);
    }

    public void AddTmpPos(){
        for(int i = 0; i < TmpStartPosList.Count; i++) StartPosList.Add(TmpStartPosList[i]);
        for(int i = 0; i < TmpEndPosList.Count; i++) EndPosList.Add(TmpEndPosList[i]);
        TmpStartPosList.Clear();
        TmpEndPosList.Clear();
    }

    public void SortTag(){
        List<int> indices = new List<int>();
        for (int i = 0; i < StartPosList.Count; i++) indices.Add(i);

        indices.Sort((a, b) =>
        {
            if (StartPosList[a] != StartPosList[b])
            return StartPosList[a].CompareTo(StartPosList[b]);
            if (EndPosList[a] != EndPosList[b])
            return EndPosList[b].CompareTo(EndPosList[a]);
            return 0;
        });

        List<string> sortedTagList = new List<string>();
        List<List<string>> sortedParamList = new List<List<string>>();
        List<int> sortedStartPosList = new List<int>();
        List<int> sortedEndPosList = new List<int>();
        List<int> sortedStartPosListUnprocessed = new List<int>();
        List<int> sortedEndPosListUnprocessed = new List<int>();

        foreach (int index in indices)
        {
            sortedTagList.Add(TagList[index]);
            sortedParamList.Add(ParamList[index]);
            sortedStartPosList.Add(StartPosList[index]);
            sortedEndPosList.Add(EndPosList[index]);
            sortedStartPosListUnprocessed.Add(StartPosListUnprocessed[index]);
            sortedEndPosListUnprocessed.Add(EndPosListUnprocessed[index]);
        }

        TagList = sortedTagList;
        ParamList = sortedParamList;
        StartPosList = sortedStartPosList;
        EndPosList = sortedEndPosList;
        StartPosListUnprocessed = sortedStartPosListUnprocessed;
        EndPosListUnprocessed = sortedEndPosListUnprocessed;
    }

    public void GetCharacterMeshBounds(int index, TMP_TextInfo textInfo, out Vector3 min, out Vector3 max)
    {
        min = Vector3.positiveInfinity;
        max = Vector3.negativeInfinity;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[index];
        if (!charInfo.isVisible)
        {
            min = Vector3.zero;
            max = Vector3.zero;
            return;
        }

        int vertexIndex = charInfo.vertexIndex;
        Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

        for (int i = 0; i < 4; i++)
        {
            Vector3 vertex = vertices[vertexIndex + i];
            min = Vector3.Min(min, vertex);
            max = Vector3.Max(max, vertex);
        }

        min += Parent_Transform.position;
        max += Parent_Transform.position;
    }

    public void ProcessTag(int pointerIndex,Vector4 Mask,out int ActIndex,out int WordDataId){
        ActIndex = pointerIndex;
        if(lastOverIndex != -1){
            if (ActIndex == -1) ActIndex = lastOverIndex;
            Vector3 min, max;
            GetCharacterMeshBounds(ActIndex, TextComponent.textInfo, out min, out max);
            Vector3 MouseWorldPos = Input.mousePosition;
            if (Mask.x != -1 && (min.x < Mask.x || min.y < Mask.y || max.x > Mask.z || max.y > Mask.w))
                ActIndex = -1;
            else if (MouseWorldPos.x < min.x || MouseWorldPos.x > max.x || MouseWorldPos.y < min.y || MouseWorldPos.y > max.y)
                ActIndex = -1;
            else
                lastOverIndex = ActIndex;
        }else{
            lastOverIndex = pointerIndex;
        }

        WordDataId = -1;
        if(ActIndex != -1){
            for(int i = 0; i < StartPosList.Count; i++){
                if(StartPosList[i] <= ActIndex && ActIndex <= EndPosList[i]){
                    if(TagList[i] == "wt")
                        WordDataId = Tag2Index(ParamList[i][0]);
                }
            }
        }

        TMP_TextInfo info;
        if (InputField != null) info = InputField.textComponent.textInfo;
        else info = TextComponent.textInfo;
        for(int i = 0; i <TagList.Count ; i++){
            if (ChangeText[System.Array.IndexOf(MatchTag, TagList[i])])
            ProcessFunctions[System.Array.IndexOf(MatchTag, TagList[i])](i,info,this);
        }
        // Change text first or animation will be laggy
        for(int i = 0; i <TagList.Count ; i++){
            if (!ChangeText[System.Array.IndexOf(MatchTag, TagList[i])])
            ProcessFunctions[System.Array.IndexOf(MatchTag, TagList[i])](i,info,this);
        }
        initialized = true;
        if (InputField != null) InputField.textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        else TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }


#region TagSetting


    string[] MatchTag = new string[] // <Tag=Param>...</Tag>
    { 
        "wt",                   
            // Word Tag link 2 other Box;       
            // Args=string tag
        "wave",                 
            // Make words waving;       
            // Args=float offset, float amplitude, float frequency
        "glitch",               
            // Make words replace with random words each frame;     
            // Args=float interval
        "glitch_cn",
            // Make words replace with random Chinese characters each frame;     
            // Args=float interval
        "type",              
            // Change typing speed and ways to type;
            // Args=float showspeed, float delayspeed, int types
            // types: 0->Fade(set delay speed to 0 to make it instant)
    };

    public delegate void ProcessDelegate(int index, TMP_TextInfo info, WordPreprocessor P);
    public static ProcessDelegate[] ProcessFunctions = new ProcessDelegate[] 
    {
        pass,
        waveProcess,
        glitchProcess,
        glitch_cnProcess,
        type_setting_process,
    };

    public static bool[] ChangeText = new bool[] 
    {
        false,
        false,
        true,
        true,
        false,
    };


#endregion
#region TagFunction


    public static void pass(int index, TMP_TextInfo info, WordPreprocessor P){
        return;
    }

    public static void waveProcess(int index, TMP_TextInfo info, WordPreprocessor P){
        List<string> args = P.ParamList[index];
        float offset_ec = 1;
        float amp = 1;
        float freq = 1;
        if (args.Count > 0) offset_ec = float.Parse(args[0]);
        if (args.Count > 1) amp = float.Parse(args[1]);
        if (args.Count > 2) freq = float.Parse(args[2]);

        for (int i = P.StartPosList[index]; i < P.EndPosList[index]; i++)
        {
            var charInfo = info.characterInfo[i];
            if (!charInfo.isVisible)
                continue;

            Vector3 offset = new Vector3(0, Mathf.Sin(Time.time * freq + freq*i*offset_ec) * amp, 0);


            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = info.meshInfo[charInfo.materialReferenceIndex].vertices;
            Vector3[] OrigVertices = P.OInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            vertices[vertexIndex + 0] = OrigVertices[vertexIndex + 0] + offset;
            vertices[vertexIndex + 1] = OrigVertices[vertexIndex + 1] + offset;
            vertices[vertexIndex + 2] = OrigVertices[vertexIndex + 2] + offset;
            vertices[vertexIndex + 3] = OrigVertices[vertexIndex + 3] + offset;

        }
    }

    List<float> glitch_LastTimeList = new List<float>();
    public static void glitchProcess(int index, TMP_TextInfo info, WordPreprocessor P) {
        List<string> args = P.ParamList[index];
        float interval = 0.2f;
        if (args.Count > 0) interval = float.Parse(args[0]);
        if (P.glitch_LastTimeList.Count <= index) {
            for (int i = P.glitch_LastTimeList.Count; i <= index; i++) {
                P.glitch_LastTimeList.Add(0);
            }
        }
        if (Time.time - P.glitch_LastTimeList[index] > interval) {
            
            P.glitch_LastTimeList[index] = Time.time;
            
            string originalText;
            if(P.InputField != null) originalText = P.InputField.text;
            else originalText = P.TextComponent.text;
            char[] charArray = originalText.ToCharArray();
            for (int i = P.StartPosListUnprocessed[index]; i < P.EndPosListUnprocessed[index]; i++) {
                if (i < 0 || i >= charArray.Length) continue;
                if (!info.characterInfo[i-P.StartPosListUnprocessed[index]+P.StartPosList[index]].isVisible)
                    continue;
                char randomChar = (char)Random.Range(33, 127); // ASCII 可打印字符范围
                charArray[i] = randomChar;
            }
            if (P.InputField != null) {
                P.InputField.text = new string(charArray);
            } else if (P.TextComponent != null) {
                P.TextComponent.text = new string(charArray);
            }
        }
    }

    public static void glitch_cnProcess(int index, TMP_TextInfo info, WordPreprocessor P){
        List<string> args = P.ParamList[index];
        float interval = 0.2f;
        if (args.Count > 0) interval = float.Parse(args[0]);
        if (P.glitch_LastTimeList.Count <= index) {
            for (int i = P.glitch_LastTimeList.Count; i <= index; i++) {
                P.glitch_LastTimeList.Add(0);
            }
        }
        if (Time.time - P.glitch_LastTimeList[index] > interval) {
            
            P.glitch_LastTimeList[index] = Time.time;
            string originalText;
            if(P.InputField != null) originalText = P.InputField.text;
            else originalText = P.TextComponent.text;
            char[] charArray = originalText.ToCharArray();
            
            for (int i = P.StartPosListUnprocessed[index]; i < P.EndPosListUnprocessed[index]; i++) {
                if (i < 0 || i >= charArray.Length) continue;
                if (!info.characterInfo[i-P.StartPosListUnprocessed[index]+P.StartPosList[index]].isVisible)
                    continue;
                char randomChar = P.ChineseChars[Random.Range(0, P.ChineseChars.Length)];
                charArray[i] = randomChar;
            }
            if (P.InputField != null) {
                P.InputField.text = new string(charArray);
            } else if (P.TextComponent != null) {
                P.TextComponent.text = new string(charArray);
            }
        }
    }

    public static void type_setting_process(int index, TMP_TextInfo info, WordPreprocessor P){
        if (!P.initialized){
            if(!P.type_setting_initialized){
                List<int> Tag2Remove = new List<int>();
                int count = P.TagList.Count;
                for(int i = 0; i < count; i++){
                    if(P.TagList[i] == "type"){
                        List<string> args = P.ParamList[i];
                        float showspeed = 0.1f;
                        float delayspeed = 0.1f;
                        int types = 0;
                        if (args.Count > 0) showspeed = float.Parse(args[0]);
                        if (args.Count > 1) delayspeed = float.Parse(args[1]);
                        if (args.Count > 2) types = int.Parse(args[2]);
                        P.TypeSetting(P.StartPosList[i],P.EndPosList[i],showspeed,delayspeed,types);
                        if (i!=index) Tag2Remove.Add(i);
                    }
                }
                for(int i = Tag2Remove.Count-1; i >= 0; i--) P.RemoveTag(Tag2Remove[i]);
                P.type_setting_initialized = true;
            }else return;
        }else{
            if (!P.isTyping) return;
            if(P.TypeST == -1) P.TypeST = Time.time;
            for(int i = 0; i < P.ProcessedTextLength; i++){
                float TT = Time.time - P.TypeST;
                TMP_CharacterInfo charInfo = info.characterInfo[i];
                if (!charInfo.isVisible) continue;
                int vertexIndex = charInfo.vertexIndex;
                Color32[] colors = info.meshInfo[charInfo.materialReferenceIndex].colors32;
                float t=1;
                if(TT >= P.TypeStartTime[i])
                    if(P.TypeDelayTime[i]==P.TypeStartTime[i]||TT >= P.TypeDelayTime[i]) t = 1;
                    else t = (TT - P.TypeStartTime[i])/(P.TypeDelayTime[i] - P.TypeStartTime[i]);
                else t = 0;
                for (int j = 0; j < 4; j++) colors[vertexIndex + j].a = (byte)(255 * t);
                if (i == P.ProcessedTextLength - 1 && TT >= P.TypeDelayTime[i]) P.isTyping = false;
            }
        }
    }


#endregion
#region SubTagFunction

    void TypeSetting(int index,int Eindex,float showspeed,float delayspeed,int type = 0){
        // Debug.Log(index+" "+Eindex+" "+showspeed+" "+delayspeed+" "+type);
        if (TypeStartTime.Count == 0)
            for(int i = 0; i < ProcessedTextLength; i++){
                TypeStartTime.Add(0);
                TypeDelayTime.Add(0);
                TypeTypes.Add(0);
            }
        float ST = TypeStartTime[index];
        for(int i = index; i < Eindex; i++){
            TypeStartTime[i] = ST + showspeed*(i-index);
            TypeDelayTime[i] = TypeStartTime[i] + delayspeed;
            TypeTypes[i] = type;    
        }
    }

    public void SkipType(){
        TypeST = -10000;
    }

#endregion

}