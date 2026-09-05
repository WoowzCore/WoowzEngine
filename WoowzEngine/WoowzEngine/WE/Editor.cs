using WEI_Attribute;
using WEO;
using WLO;
using WLO.Math;

namespace WE;

public struct Editor{
    // todo, remove setter?
    public static bool IsEditor = false;
    
    // ----------------------------------------------------------------------

    private static readonly Dictionary<Type, Type>                   __DefaultProperties = [];
    private static readonly Dictionary<Type, WEEI_InspectorProperty> __Instances         = [];

    static Editor(){
        RegisterDefault<bool    , WEEI_Bool_Default>();
        RegisterDefault<int     , WEEI_Int_Default>();
        RegisterDefault<float   , WEEI_Float_Default>();
        RegisterDefault<string  , WEEI_String_Default>();
        RegisterDefault<Vector2F, WEEI_Vector2F_Default>();
        RegisterDefault<Vector3F, WEEI_Vector3F_Default>();
        RegisterDefault<Color4B , WEEI_Color4B_Default>();
        
        RegisterDefault(typeof(Enum   ), typeof(WEEI_Enum_Default));
        RegisterDefault(typeof(Asset<>), typeof(WEEI_Asset_Default));
    }

    public static void RegisterDefault<T, TA>() where TA : WEEI_InspectorProperty, new() => RegisterDefault(typeof(T), typeof(TA));

    public static void RegisterDefault(Type Type, Type AType){
        __DefaultProperties[Type] = AType;

        if(!__Instances.ContainsKey(AType)){
            __Instances[AType] = (WEEI_InspectorProperty)Activator.CreateInstance(AType)!;
        }
    }

    public static WEEI_InspectorProperty? GetDefault(Type Type){
        if(__DefaultProperties.TryGetValue(Type, out Type? AType)){
            return __Instances[AType];
        }

        if(Type.IsGenericType){
            Type GenericDefinition = Type.GetGenericTypeDefinition();
            if(__DefaultProperties.TryGetValue(GenericDefinition, out AType)){
                return __Instances[AType];
            }
        }

        if(Type.IsEnum){
            if(__DefaultProperties.TryGetValue(typeof(Enum), out AType)){
                return __Instances[AType];
            }
        }
        
        return null;
    }
}