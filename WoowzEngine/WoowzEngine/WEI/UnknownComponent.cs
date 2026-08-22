using WEI.Editor;

namespace WEI;

public class UnknownComponent : WEI.Component{
    [WEESave] public string OriginalType = "";
    [WEEMultilineString(500)]
    [WEESave] public string RawJSONData = "";

    public override Dictionary<string, object> Serialize(){
        try{
            Dictionary<string, object> Data = WL.Serializer.FromJson(RawJSONData);
            Data[WL.Serializer.__Type] = OriginalType;
            return Data;
        }catch(Exception e){
            return new Dictionary<string, object>(){
                [WL.Serializer.__Type] = OriginalType,
                ["__Error"] = e.Message,
                ["__Raw"] = RawJSONData
            };
        }
    }
    
    public override void Deserialize(Dictionary<string, object> Data){
        if(Data.TryGetValue(WL.Serializer.__Type, out object? V_Type)){ OriginalType = V_Type.ToString()!; }
        
        RawJSONData = WL.Serializer.ToJson(Data);
    }
}