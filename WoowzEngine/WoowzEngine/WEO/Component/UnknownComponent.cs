using WEI_Attribute;

namespace WEO;

public class UnknownComponent : WEI.Component{
    [WE_Save] public string OriginalType = "";
    [WEEI_String_Multiline(500)]
    [WE_Save] public string RawJSONData  = "";

    public override Dictionary<string, object?> __Pack(){
        try{
            Dictionary<string, object?> Data = (Dictionary<string, object?>)WL.String.FromJSON(RawJSONData)!;
            Data[WL.Packer.PackType] = OriginalType;
            return Data;
        }catch(Exception e){
            return new Dictionary<string, object?>{
                [WL.Packer.PackType] = OriginalType,
                ["JSONError"] = e.Message,
                ["JSONData"] = RawJSONData
            };
        }
    }
    
    public override void __Unpack(Dictionary<string, object?> Data){
        OriginalType = WL.Packer.Get(Data, WL.Packer.PackType, "")!;
        
        RawJSONData = WL.String.ToJSON(Data);
    }
}